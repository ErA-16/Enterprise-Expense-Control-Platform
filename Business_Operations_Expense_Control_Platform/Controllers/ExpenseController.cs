using Business_Operations_Expense_Control_Platform.Data;
using Business_Operations_Expense_Control_Platform.Dtos;
using Business_Operations_Expense_Control_Platform.Models;
using Business_Operations_Expense_Control_Platform.Services;
using Business_Operations_Expense_Control_Platform.Services.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Business_Operations_Expense_Control_Platform.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExpenseController : ControllerBase
    {
        private readonly ILogger<ExpenseController> _logger;
        private readonly IExpenseService _expenseService;
        public ExpenseController(ILogger<ExpenseController> logger, IExpenseService expenseService)
        {
            _expenseService = expenseService;
            _logger = logger;
        }


        [HttpPost("submit-request")]
        [Authorize(Roles = "Manager,Employee")]
        public async Task<IActionResult> CreateExpense(CreateExpenseDto dto)
        {
            var userClaimId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userClaimId))
            {
                return Unauthorized();
            }

            int currentUserId = int.Parse(userClaimId);

            try
            {
                var result = await _expenseService.CreateExpenseAsync(dto, currentUserId);
                return Ok(new { message = "Expense request created successfully!", Data = result });
            }
            catch (UserNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (FileUploadException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected server error occurred during expense initialization processing.");
                return StatusCode(500, new { message = "An internal server error occurred while processing your expense request." });
            }
        }

        [HttpGet("myrequests")]
        [Authorize(Roles = "Employee,Manager")]
        public async Task<ActionResult<List<ExpenseRequestDisplayInfoDto>>> GetMyOwnRequests()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized();
            }

            int currentUserId = int.Parse(userIdClaim);

            try
            {
                var result = await _expenseService.GetMyOwnRequestsAsync(currentUserId);
                return Ok(result);
            }
            catch (UserNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected server error occurred during expense initialization processing.");
                return StatusCode(500, new { message = "An internal server error occurred while processing your expense request." });
            }
        }

        [HttpGet("manager/pending-requests")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<List<ExpenseRequestDisplayInfoDto>>> GetDepartmentRequests()
        {
            var deptClaim = User.FindFirstValue("DepartmentId");

            if (string.IsNullOrEmpty(deptClaim))
            {
                return Unauthorized();
            }

            var userDepartmentId = int.Parse(deptClaim);

            try
            {
                var requests = await _expenseService.GetDepartmentRequestsAsync(userDepartmentId);
                return Ok(requests);
            }
            catch (DepartmentNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred while fetching department expense records.");
                return StatusCode(500, new { message = "An internal server error occurred while processing department records." });
            }
        }

        [HttpPost("manager/{id:int}/process")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> ProcessRequest(int id, ManagerReviewExpenseDto reviewDto)
        {
            var managerIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var managerDeptClaim = User.FindFirstValue("DepartmentId");

            if (string.IsNullOrEmpty(managerIdClaim) || string.IsNullOrEmpty(managerDeptClaim))
            {
                return Unauthorized();
            }

            int currentManagerId = int.Parse(managerIdClaim);
            int currentManagerDeptId = int.Parse(managerDeptClaim);

            try
            {
                await _expenseService.ProcessRequestAsync(id, reviewDto, currentManagerId, currentManagerDeptId);

                return Ok(new { Message = $"Request has been successfully {(reviewDto.IsApproved ? "Approved" : "Rejected")}." });
            }

            catch (ManagerWithIdNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ExpenseNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedExpenseReviewException)
            {
                return Forbid();
            }
            catch (ExpenseAlreadyProcessingException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected server crash occurred while a manager attempted to review expense request ID {Id}.", id);
                return StatusCode(500, new { message = "An internal server error occurred while finalizing the review." });
            }
        }
        
        [HttpGet("finance/pending-requests")]
        [Authorize(Roles = "Finance")]
        public async Task<ActionResult<List<ExpenseRequestDisplayInfoDto>>> GetAprovedRequests()
        {
            try
            {
                var requests = await _expenseService.GetAprovedRequestsAsync();

                if (!requests.Any())
                {
                    return NotFound(new { message = "No new requests has been approved by manager" });
                }

                return requests;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected server error occurred while finance fetched approved requests.");
                return StatusCode(500, new { message = "An internal server error occurred while processing requests." });
            }
        }

        [HttpPost("finance/{id:int}/process")]
        [Authorize(Roles = "Finance")]
        public async Task<IActionResult> ProcessRequest(int id, FinanceReviewExpenseDto reviewDto)
        {
            var currentFinanceUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(currentFinanceUserId))
            {
                return Unauthorized();
            }

            var financeUserId = int.Parse(currentFinanceUserId);

            try
            {
                var user = await _expenseService.ProcessFinanceReviewAsync(id, reviewDto, financeUserId);
                return Ok(new { Message = $"Request has been successfully {(reviewDto.IsApproved ? "Approved for processing" : "Rejected by Finance")}." });
            }
            catch (ExpenseNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected server error occurred while finance fetched approved requests.");
                return StatusCode(500, new { message = "An internal server error occurred while processing requests." });
            }
        }

        [HttpGet("requests/processing-requests")]
        [Authorize(Roles = "Finance")]
        public async Task<ActionResult<List<ExpenseRequestDisplayInfoDto>>> GetAllProcessingRequests()
        {
            try
            {
                var requests = await _expenseService.GetAllProcessingRequestsAsync();

                if (!requests.Any())
                {
                    return NotFound(new { message = "No processing requests found." });
                }

                return Ok(requests);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "An unexpected server error occurred while finance fetched approved requests.");
                return StatusCode(500, new { message = "An internal server error occurred while processing requests." });
            }
        }

        [HttpPost("finance/{id:int}/pay")]
        [Authorize(Roles = "Finance")]
        public async Task<IActionResult> MakePayment(int id, MakePaymentDto dto)
        {
            var currentFinanceUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(currentFinanceUserId))
            {
                return Unauthorized();
            }

            var financeUserId = int.Parse(currentFinanceUserId);

            try
            {
                await _expenseService.MakePaymentAsync(id, dto, financeUserId);
                return Ok(new { Message = "Payment recorded and request marked as Paid." });
            }
            catch (ExpenseNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UploadProofOfPaymentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (FileUploadException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected server error occurred while finance attempted payout processing for Request ID {Id}.", id);
                return StatusCode(500, new { message = "An internal server error occurred while processing requests." });
            }
        }

        [HttpGet("{id:int}/history")]
        [Authorize(Roles = "Employee,Manager,Finance")]
        public async Task<ActionResult<List<RequestAuditDisplayDto>>> GetRequestHistory(int id)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var roleClaim = User.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrEmpty(userIdClaim) || string.IsNullOrEmpty(roleClaim))
            {
                return Unauthorized();
            }

            int currentUserId = int.Parse(userIdClaim);

            try
            {
                var history = await _expenseService.GetRequestHistoryAsync(id, currentUserId, roleClaim);

                return Ok(history);
            }
            catch (UserNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ExpenseNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected server error occurred while retrieving history for Request ID {Id}.", id);
                return StatusCode(500, new { message = "An internal server error occurred while processing request history." });
            }
        }

        [HttpGet("reports/expenses/by-department")]
        [Authorize(Roles = "Admin,Manager,Finance")]
        public async Task<ActionResult<List<ExpenseByDepartmentDisplayDto>>> GetExpenseReportByDepartment()
        {
            var isAdminOrFinance = User.IsInRole("Admin") || User.IsInRole("Finance");
            int? managerDeptId = null;

            if (!isAdminOrFinance)
            {
                var managerDepartmentClaim = User.FindFirstValue("DepartmentId");
                if (string.IsNullOrEmpty(managerDepartmentClaim))
                {
                    return Forbid();
                }
                managerDeptId = int.Parse(managerDepartmentClaim);
            }

            try
            {
                var reports = await _expenseService.GetExpenseReportByDepartmentAsync(managerDeptId);
                return Ok(reports);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected server crash occurred while compiling department expense reports data analytics.");
                return StatusCode(500, new { message = "An internal server error occurred while building the expense reports data." });
            }
        }

        [HttpGet("finance/paid-requests")]
        [Authorize(Roles = "Finance")]
        public async Task<ActionResult<List<ExpenseRequestDisplayInfoDto>>> GetPaidRequests()
        {
            try
            {
                var requests = await _expenseService.GetPaidRequestsAsync();
                return requests.Any() ? Ok(requests) : NotFound(new { message = "No paid requests found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected server crash occurred while compiling department expense reports data analytics.");
                return StatusCode(500, new { message = "An internal server error occurred while building the expense reports data." });
            }
        }
    }
}