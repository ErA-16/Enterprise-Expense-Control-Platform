using Business_Operations_Expense_Control_Platform.Hubs;
using Business_Operations_Expense_Control_Platform.Data;
using Business_Operations_Expense_Control_Platform.Dtos;
using Business_Operations_Expense_Control_Platform.Models;
using Business_Operations_Expense_Control_Platform.Services.Exceptions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Business_Operations_Expense_Control_Platform.Services
{
    public class ExpenseService : IExpenseService
    {
        private readonly DatabaseContext _db;
        private readonly ILogger<ExpenseService> _logger;
        private readonly IHubContext<NotificationHub> _hubContext;

        private static readonly string[] AllowedContentTypes =
        {
            "image/jpeg",
            "image/jpg",
            "image/png",
            "application/pdf"
        };

        public ExpenseService(
            DatabaseContext db,
            ILogger<ExpenseService> logger,
            IHubContext<NotificationHub> hubContext)
        {
            _logger = logger;
            _db = db;
            _hubContext = hubContext;
        }

        private async Task<(bool Success, string? FilePath, string? Error)> SaveUploadedFileAsync(
            IFormFile file,
            string subfolder)
        {
            if (!AllowedContentTypes.Contains(file.ContentType))
            {
                return (false, null, "Only JPG, PNG, or PDF files are allowed");
            }

            var uploadsFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "uploads",
                subfolder);

            Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return (
                true,
                Path.Combine("uploads", subfolder, fileName),
                null
            );
        }

        private Notification AddNotification(
            int senderId,
            int receiverId,
            string message,
            int? requestId = null)
        {
            var notification = new Notification
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Message = message,
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                RequestId = requestId
            };

            _db.Notifications.Add(notification);

            return notification;
        }

        private async Task PushNotificationAsync(Notification notification)
        {
            var dto = new NotificationDisplayDto
            {
                Id = notification.Id,
                Message = notification.Message,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                RequestId = notification.RequestId
            };

            await _hubContext.Clients
                .User(notification.ReceiverId.ToString())
                .SendAsync("ReceiveNotification", dto);
        }

        public async Task<ExpenseRequestDisplayInfoDto> CreateExpenseAsync(
            CreateExpenseDto dto,
            int currentUserId)
        {
            _logger.LogInformation(
                "Processing expense request creation for User ID: {UserId}",
                currentUserId);

            var user = await _db.Users.FindAsync(currentUserId);

            if (user == null)
            {
                _logger.LogWarning(
                    "Expense creation failed. Profile with ID {UserId} not found.",
                    currentUserId);

                throw new UserNotFoundException("Profile not found");
            }

            var newRequest = new ExpenseRequest
            {
                Title = dto.Title,
                Reason = dto.Reason,
                Amount = dto.Amount,
                DepartmentId = user.DepartmentId,
                RequesterId = currentUserId,
                DateAndTime = DateTime.UtcNow,
                Status = ExpenseRequestStatus.Pending
            };

            if (dto.Receipt != null)
            {
                var (success, filePath, error) =
                    await SaveUploadedFileAsync(dto.Receipt, "receipts");

                if (!success)
                {
                    throw new FileUploadException(
                        $"Receipt upload failed: {error}");
                }

                newRequest.SubmissionReceiptPath = filePath;
            }

            var auditAction = new RequestAudit
            {
                Request = newRequest,
                Action = RequestAuditAction.Created,
                ActorId = currentUserId,
                Actor = user,
                Timestamp = DateTime.UtcNow
            };

            _db.ExpenseRequests.Add(newRequest);
            _db.RequestAudits.Add(auditAction);

            var managers = await _db.Users
                .Where(u =>
                    u.DepartmentId == user.DepartmentId &&
                    u.Role == UserRole.Manager)
                .ToListAsync();

            await _db.SaveChangesAsync();

            var notifications = new List<Notification>();

            foreach (var manager in managers)
            {
                var notification = AddNotification(
                    senderId: currentUserId,
                    receiverId: manager.Id,
                    message:
                        $"New request: {newRequest.Title} — ₦{newRequest.Amount} from {user.FirstName} {user.LastName}",
                    requestId: newRequest.Id
                );

                notifications.Add(notification);
            }

            await _db.SaveChangesAsync();

            foreach (var notification in notifications)
            {
                await PushNotificationAsync(notification);
            }

            _logger.LogInformation(
                "Expense request ID {RequestId} initialized successfully.",
                newRequest.Id);

            return new ExpenseRequestDisplayInfoDto
            {
                Id = newRequest.Id,
                Title = newRequest.Title,
                Amount = newRequest.Amount,
                Reason = newRequest.Reason,
                DateAndTime =
                    newRequest.DateAndTime.ToString("yyyy-MM-dd HH:mm:ss"),
                Status = newRequest.Status
            };
        }

        public async Task<List<ExpenseRequestDisplayInfoDto>> GetMyOwnRequestsAsync(
            int currentUserId)
        {
            _logger.LogInformation(
                "User {Id} is fetching their own personal requests.",
                currentUserId);

            var user = await _db.Users.FindAsync(currentUserId);

            if (user == null)
            {
                _logger.LogInformation(
                    "User {Id} not found",
                    currentUserId);

                throw new UserNotFoundException("User not found");
            }

            var requests = await _db.ExpenseRequests
                .Where(u => u.RequesterId == currentUserId)
                .Select(r => new ExpenseRequestDisplayInfoDto
                {
                    Id = r.Id,
                    Title = r.Title,
                    Amount = r.Amount,
                    Reason = r.Reason,
                    DateAndTime =
                        r.DateAndTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    Status = r.Status,
                    ApprovedById = r.ApprovedById,
                    ApprovedAt = r.ApprovedAt.HasValue
                        ? r.ApprovedAt.Value.ToString("yyyy-MM-dd HH:mm:ss")
                        : null,
                    RejectedById = r.RejectedById,
                    RejectedAt = r.RejectedAt.HasValue
                        ? r.RejectedAt.Value.ToString("yyyy-MM-dd HH:mm:ss")
                        : null,
                    RejectedByRole = r.RejectedByRole.HasValue
                        ? r.RejectedByRole.Value.ToString()
                        : null,
                    PaidById = r.PaidById,
                    PaidAt = r.PaidAt.HasValue
                        ? r.PaidAt.Value.ToString("yyyy-MM-dd HH:mm:ss")
                        : null,
                    ManagerComment = r.ManagerComment,
                })
                .ToListAsync();

            return requests;
        }

        public async Task<List<ExpenseRequestDisplayInfoDto>> GetDepartmentRequestsAsync(
            int userDepartmentId)
        {
            _logger.LogInformation(
                "Manager is fetching all requests for Department {DeptId}.",
                userDepartmentId);

            var departmentExists =
                await _db.Departments.AnyAsync(d => d.Id == userDepartmentId);

            if (!departmentExists)
            {
                _logger.LogWarning(
                    "Department request retrieval failed. Department {DeptId} not found.",
                    userDepartmentId);

                throw new DepartmentNotFoundException(
                    $"Department with ID {userDepartmentId} does not exist.");
            }

            var requests = await _db.ExpenseRequests
                .Where(u => u.DepartmentId == userDepartmentId)
                .Select(r => new ExpenseRequestDisplayInfoDto
                {
                    Id = r.Id,
                    Title = r.Title,
                    Amount = r.Amount,
                    Reason = r.Reason,
                    DateAndTime =
                        r.DateAndTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    Status = r.Status,
                    ApprovedById = r.ApprovedById,
                    ApprovedAt = r.ApprovedAt.HasValue
                        ? r.ApprovedAt.Value.ToString("yyyy-MM-dd HH:mm:ss")
                        : null,
                    RejectedById = r.RejectedById,
                    RejectedAt = r.RejectedAt.HasValue
                        ? r.RejectedAt.Value.ToString("yyyy-MM-dd HH:mm:ss")
                        : null,
                    RejectedByRole = r.RejectedByRole.HasValue
                        ? r.RejectedByRole.Value.ToString()
                        : null,
                    PaidById = r.PaidById,
                    PaidAt = r.PaidAt.HasValue
                        ? r.PaidAt.Value.ToString("yyyy-MM-dd HH:mm:ss")
                        : null,
                    ManagerComment = r.ManagerComment,
                })
                .ToListAsync();

            return requests;
        }

        public async Task<bool> ProcessRequestAsync(
            int id,
            ManagerReviewExpenseDto reviewDto,
            int currentManagerId,
            int currentManagerDeptId)
        {
            _logger.LogInformation(
                "Manager ID {ManagerId} processing review for Expense Request ID {RequestId}",
                currentManagerId,
                id);

            var manager = await _db.Users.FindAsync(currentManagerId);

            if (manager == null)
            {
                _logger.LogWarning(
                    "Review failed. Manager profile with ID {Id} not found.",
                    currentManagerId);

                throw new ManagerWithIdNotFoundException(
                    "No manager with this Id found");
            }

            var expenseRequest = await _db.ExpenseRequests.FindAsync(id);

            if (expenseRequest == null)
            {
                _logger.LogWarning(
                    "Review failed. Expense request ID {Id} does not exist.",
                    id);

                throw new ExpenseNotFoundException("Request not found");
            }

            if (expenseRequest.Status != ExpenseRequestStatus.Pending)
            {
                _logger.LogWarning(
                    "Review failed. Request ID {Id} has already been processed with status {Status}.",
                    id,
                    expenseRequest.Status);

                throw new ExpenseAlreadyProcessingException(
                    "This request has already been processed");
            }

            if (expenseRequest.DepartmentId != currentManagerDeptId)
            {
                _logger.LogWarning(
                    "Access Denied. Manager {ManagerId} attempted to review an expense in Department {TargetDeptId} instead of their own Department {ManagerDeptId}",
                    currentManagerId,
                    expenseRequest.DepartmentId,
                    currentManagerDeptId);

                throw new UnauthorizedExpenseReviewException(
                    "You do not have permission to view or review expenses outside your department.");
            }

            if (expenseRequest.RequesterId == currentManagerId)
            {
                _logger.LogWarning(
                    "Access Denied. Manager {ManagerId} attempted to self-approve/reject their own request ID {RequestId}",
                    currentManagerId,
                    id);

                throw new UnauthorizedExpenseReviewException(
                    "Self-approval is strictly prohibited. Your requests must be reviewed by another administrator.");
            }

            RequestAuditAction auditAction;

            if (reviewDto.IsApproved)
            {
                expenseRequest.Status = ExpenseRequestStatus.Approved;
                expenseRequest.ApprovedById = currentManagerId;
                expenseRequest.ApprovedAt = DateTime.UtcNow;
                expenseRequest.RejectedById = null;
                expenseRequest.RejectedAt = null;
                expenseRequest.RejectedByRole = null;

                auditAction = RequestAuditAction.Approved;
            }
            else
            {
                expenseRequest.Status = ExpenseRequestStatus.Rejected;
                expenseRequest.RejectedById = currentManagerId;
                expenseRequest.RejectedAt = DateTime.UtcNow;
                expenseRequest.RejectedByRole =
                    Models.RejectionRole.Manager;

                auditAction = RequestAuditAction.Rejected;
            }

            expenseRequest.ManagerComment = reviewDto.ManagerComment;

            var audit = new RequestAudit
            {
                Request = expenseRequest,
                Action = auditAction,
                ActorId = currentManagerId,
                Timestamp = DateTime.UtcNow
            };

            _db.RequestAudits.Add(audit);

            var managerMessage = reviewDto.IsApproved
                ? $"Your request '{expenseRequest.Title}' was approved"
                : $"Your request '{expenseRequest.Title}' was rejected";

            var notification = AddNotification(
                currentManagerId,
                expenseRequest.RequesterId,
                managerMessage,
                expenseRequest.Id);

            await _db.SaveChangesAsync();

            await PushNotificationAsync(notification);

            _logger.LogInformation(
                "Successfully recorded review decision for request ID: {RequestId} by Manager ID: {ManagerId}",
                id,
                currentManagerId);

            return true;
        }

        public async Task<List<ExpenseRequestDisplayInfoDto>> GetAprovedRequestsAsync()
        {
            var requests = await _db.ExpenseRequests
                .Where(r => r.Status == ExpenseRequestStatus.Approved)
                .Select(e => new ExpenseRequestDisplayInfoDto
                {
                    Id = e.Id,
                    Title = e.Title,
                    Amount = e.Amount,
                    Reason = e.Reason,
                    DateAndTime =
                        e.DateAndTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    Status = e.Status,
                    ApprovedById = e.ApprovedById,
                    ApprovedAt = e.ApprovedAt.HasValue
                        ? e.ApprovedAt.Value.ToString("yyyy-MM-dd HH:mm:ss")
                        : null,
                    RejectedById = e.RejectedById,
                    RejectedAt = e.RejectedAt.HasValue
                        ? e.RejectedAt.Value.ToString("yyyy-MM-dd HH:mm:ss")
                        : null,
                    RejectedByRole = e.RejectedByRole.HasValue
                        ? e.RejectedByRole.Value.ToString()
                        : null,
                    PaidById = e.PaidById,
                    PaidAt = e.PaidAt.HasValue
                        ? e.PaidAt.Value.ToString("yyyy-MM-dd HH:mm:ss")
                        : null,
                    ManagerComment = e.ManagerComment,
                })
                .ToListAsync();

            return requests;
        }

        public async Task<bool> ProcessFinanceReviewAsync(
            int id,
            FinanceReviewExpenseDto reviewDto,
            int financeUserId)
        {
            _logger.LogInformation(
                "Finance User ID {FinanceId} is reviewing Expense Request ID {RequestId}",
                financeUserId,
                id);

            var user = await _db.Users.FindAsync(financeUserId);

            if (user == null)
            {
                _logger.LogWarning(
                    "Finance processing failed. User ID {Id} not found.",
                    financeUserId);

                throw new UserNotFoundException(
                    "Finance profile not found.");
            }

            var selectedExpense =
                await _db.ExpenseRequests.FirstOrDefaultAsync(
                    e => e.Id == id &&
                         e.Status == ExpenseRequestStatus.Approved);

            if (selectedExpense == null)
            {
                _logger.LogWarning(
                    "Finance review failed. Request ID {Id} not found or not in Approved status.",
                    id);

                throw new ExpenseNotFoundException(
                    "Request not found or not awaiting finance review.");
            }

            RequestAuditAction auditAction;

            if (reviewDto.IsApproved)
            {
                selectedExpense.Status =
                    ExpenseRequestStatus.Processing;

                selectedExpense.RejectedById = null;
                selectedExpense.RejectedAt = null;
                selectedExpense.RejectedByRole = null;

                auditAction = RequestAuditAction.Processing;
            }
            else
            {
                selectedExpense.Status =
                    ExpenseRequestStatus.Rejected;

                selectedExpense.RejectedById = financeUserId;
                selectedExpense.RejectedAt = DateTime.UtcNow;
                selectedExpense.RejectedByRole =
                    Models.RejectionRole.Finance;

                auditAction = RequestAuditAction.Rejected;
            }

            var audit = new RequestAudit
            {
                Request = selectedExpense,
                Action = auditAction,
                ActorId = financeUserId,
                Timestamp = DateTime.UtcNow
            };

            _db.RequestAudits.Add(audit);

            var financeMessage = reviewDto.IsApproved
                ? $"Your request '{selectedExpense.Title}' is being processed for payment"
                : $"Your request '{selectedExpense.Title}' was rejected by Finance";

            var notification = AddNotification(
                financeUserId,
                selectedExpense.RequesterId,
                financeMessage,
                selectedExpense.Id);

            await _db.SaveChangesAsync();

            await PushNotificationAsync(notification);

            _logger.LogInformation(
                "Finance choice updated successfully for Request ID: {RequestId}",
                id);

            return true;
        }

        public async Task<List<ExpenseRequestDisplayInfoDto>> GetAllProcessingRequestsAsync()
        {
            _logger.LogInformation(
                "Finance team is retrieving all expense requests currently in 'Processing' status.");

            var requests = await _db.ExpenseRequests
                .Where(r => r.Status == ExpenseRequestStatus.Processing)
                .Select(e => new ExpenseRequestDisplayInfoDto
                {
                    Id = e.Id,
                    Title = e.Title,
                    Amount = e.Amount,
                    Reason = e.Reason,
                    DateAndTime =
                        e.DateAndTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    Status = e.Status,
                    ApprovedById = e.ApprovedById,
                    ApprovedAt = e.ApprovedAt.HasValue
                        ? e.ApprovedAt.Value.ToString("yyyy-MM-dd HH:mm:ss")
                        : null,
                    RejectedById = e.RejectedById,
                    RejectedAt = e.RejectedAt.HasValue
                        ? e.RejectedAt.Value.ToString("yyyy-MM-dd HH:mm:ss")
                        : null,
                    RejectedByRole = e.RejectedByRole.HasValue
                        ? e.RejectedByRole.Value.ToString()
                        : null,
                    PaidById = e.PaidById,
                    PaidAt = e.PaidAt.HasValue
                        ? e.PaidAt.Value.ToString("yyyy-MM-dd HH:mm:ss")
                        : null,
                    ManagerComment = e.ManagerComment,
                    PaymentMethod = e.PaymentMethod,
                })
                .ToListAsync();

            return requests;
        }

        public async Task<bool> MakePaymentAsync(
            int id,
            MakePaymentDto dto,
            int financeUserId)
        {
            _logger.LogInformation(
                "Finance User ID {FinanceId} is processing final payout for Expense Request ID {RequestId}.",
                financeUserId,
                id);

            var user = await _db.Users.FindAsync(financeUserId);

            if (user == null)
            {
                _logger.LogWarning(
                    "Payment registration failed. Finance profile with ID {Id} not found.",
                    financeUserId);

                throw new UserNotFoundException(
                    "Finance profile not found.");
            }

            var selectedExpense =
                await _db.ExpenseRequests.FirstOrDefaultAsync(
                    e => e.Id == id &&
                         e.Status == ExpenseRequestStatus.Processing);

            if (selectedExpense == null)
            {
                throw new ExpenseNotFoundException(
                    "Request not found or not ready for payment.");
            }

            if (dto.PaymentProof == null ||
                dto.PaymentProof.Length == 0)
            {
                throw new UploadProofOfPaymentException(
                    "Payment proof is required");
            }

            var (success, filePath, error) =
                await SaveUploadedFileAsync(
                    dto.PaymentProof,
                    "payment-proofs");

            if (!success)
            {
                _logger.LogWarning(
                    "Payment proof document upload failed: {Error}",
                    error);

                throw new FileUploadException(
                    $"Payment proof upload failed: {error}");
            }

            selectedExpense.PaymentProofPath = filePath;

            selectedExpense.Status = ExpenseRequestStatus.Paid;
            selectedExpense.PaidById = financeUserId;
            selectedExpense.PaidAt = DateTime.UtcNow;
            selectedExpense.PaymentMethod = dto.PaymentMethod;

            var audit = new RequestAudit
            {
                Request = selectedExpense,
                Action = RequestAuditAction.Paid,
                ActorId = financeUserId,
                Timestamp = DateTime.UtcNow
            };

            _db.RequestAudits.Add(audit);

            var notification = AddNotification(
                financeUserId,
                selectedExpense.RequesterId,
                $"Your request '{selectedExpense.Title}' has been paid via {dto.PaymentMethod}.",
                selectedExpense.Id
            );

            await _db.SaveChangesAsync();

            await PushNotificationAsync(notification);

            _logger.LogInformation(
                "Expense request ID {RequestId} has been marked as PAID successfully by Finance user {FinanceId}.",
                id,
                financeUserId);

            return true;
        }

        public async Task<List<RequestAuditDisplayDto>> GetRequestHistoryAsync(
            int id,
            int currentUserId,
            string currentRole)
        {
            _logger.LogInformation(
                "User ID {UserId} ({Role}) is retrieving the audit trail history for Request ID {RequestId}.",
                currentUserId,
                currentRole,
                id);

            var user = await _db.Users.FindAsync(currentUserId);

            if (user == null)
            {
                _logger.LogWarning(
                    "Audit fetch failed. User ID {Id} does not exist.",
                    currentUserId);

                throw new UserNotFoundException(
                    "User profile not found.");
            }

            var expenseRequest =
                await _db.ExpenseRequests.FindAsync(id);

            if (expenseRequest == null)
            {
                _logger.LogWarning(
                    "Audit fetch failed. Expense request ID {Id} not found.",
                    id);

                throw new ExpenseNotFoundException(
                    "Request not found");
            }

            if (currentRole == "Employee" &&
                expenseRequest.RequesterId != currentUserId)
            {
                _logger.LogWarning(
                    "Access Denied. Employee ID {UserId} attempted to view audit history for Request ID {RequestId} owned by User {OwnerId}",
                    currentUserId,
                    id,
                    expenseRequest.RequesterId);

                throw new UnauthorizedAccessException(
                    "You do not have permission to view this request's audit trail.");
            }

            var history = await _db.RequestAudits
                .Where(a => a.RequestId == id)
                .OrderBy(a => a.Timestamp)
                .Select(a => new RequestAuditDisplayDto
                {
                    Action = a.Action,
                    ActorId = a.ActorId,
                    ActorName =
                        a.Actor.FirstName + " " + a.Actor.LastName,
                    Timestamp =
                        a.Timestamp.ToString("yyyy-MM-dd HH:mm:ss")
                })
                .ToListAsync();

            return history;
        }

        public async Task<List<ExpenseByDepartmentDisplayDto>>
            GetExpenseReportByDepartmentAsync(int? managerDeptId)
        {
            _logger.LogInformation(
                "Generating department expense aggregation reports. Restriction scope applied: {Scope}",
                managerDeptId.HasValue
                    ? $"Department ID {managerDeptId.Value}"
                    : "Global (All Departments)");

            var departmentsQuery =
                _db.Departments.AsQueryable();

            if (managerDeptId.HasValue)
            {
                departmentsQuery =
                    departmentsQuery.Where(
                        d => d.Id == managerDeptId.Value);
            }

            var reports = await departmentsQuery
                .Select(d => new ExpenseByDepartmentDisplayDto
                {
                    Name = d.Name,
                    Total = _db.ExpenseRequests
                        .Where(e =>
                            e.DepartmentId == d.Id &&
                            e.Status == ExpenseRequestStatus.Paid)
                        .Sum(e => (decimal?)e.Amount) ?? 0m
                })
                .ToListAsync();

            return reports;
        }

        public async Task<List<ExpenseRequestDisplayInfoDto>>
            GetPaidRequestsAsync()
        {
            return await _db.ExpenseRequests
                .Where(r => r.Status == ExpenseRequestStatus.Paid)
                .Select(e => new ExpenseRequestDisplayInfoDto
                {
                    Id = e.Id,
                    Title = e.Title,
                    Amount = e.Amount,
                    Reason = e.Reason,
                    DateAndTime =
                        e.DateAndTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    Status = e.Status,
                    ApprovedById = e.ApprovedById,
                    ApprovedAt = e.ApprovedAt.HasValue
                        ? e.ApprovedAt.Value.ToString("yyyy-MM-dd HH:mm:ss")
                        : null,
                    PaidById = e.PaidById,
                    PaidAt = e.PaidAt.HasValue
                        ? e.PaidAt.Value.ToString("yyyy-MM-dd HH:mm:ss")
                        : null,
                    PaymentMethod = e.PaymentMethod,
                    ManagerComment = e.ManagerComment,
                })
                .ToListAsync();
        }
    }
}