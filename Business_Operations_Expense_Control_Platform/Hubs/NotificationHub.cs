using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Business_Operations_Expense_Control_Platform.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {

    }

}
