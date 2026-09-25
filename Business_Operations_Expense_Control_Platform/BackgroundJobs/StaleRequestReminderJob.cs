    using Business_Operations_Expense_Control_Platform.Data;
    using Business_Operations_Expense_Control_Platform.Models;
    using Microsoft.EntityFrameworkCore;

    namespace Business_Operations_Expense_Control_Platform.BackgroundJobs
    {
        public class StaleRequestReminderJob : BackgroundService
        {
            private readonly IServiceScopeFactory _scopeFactory;
            private readonly ILogger<StaleRequestReminderJob> _logger;

            public StaleRequestReminderJob(IServiceScopeFactory scopeFactory, ILogger<StaleRequestReminderJob> logger)
            {
                _scopeFactory = scopeFactory;
                _logger = logger;
            }

            protected override async Task ExecuteAsync(CancellationToken stoppingToken)
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                        var cutoff = DateTime.UtcNow.AddHours(-12);

                        var staleRequests = await db.ExpenseRequests
                            .Where(r => r.Status == ExpenseRequestStatus.Pending
                                     && r.DateAndTime <= cutoff
                                     && r.ReminderSentAt == null)
                            .ToListAsync(stoppingToken);

                        foreach (var request in staleRequests)
                        {
                            var managers = await db.Users
                                .Where(u => u.DepartmentId == request.DepartmentId && u.Role == UserRole.Manager)
                                .ToListAsync(stoppingToken);

                            foreach (var manager in managers)
                            {
                                db.Notifications.Add(new Notification
                                {
                                    SenderId = request.RequesterId,
                                    ReceiverId = manager.Id,
                                    Message = $"Reminder: request '{request.Title}' has been pending for over 12 hours",
                                    IsRead = false,
                                    CreatedAt = DateTime.UtcNow,
                                    RequestId = request.Id
                                });
                            }

                            request.ReminderSentAt = DateTime.UtcNow;
                        }

                        await db.SaveChangesAsync(stoppingToken);
                        _logger.LogInformation("Stale request reminder job ran, found {Count} stale request(s).", staleRequests.Count);
                    }

                    await Task.Delay(TimeSpan.FromHours(3), stoppingToken);
                }
            }
        }
    }