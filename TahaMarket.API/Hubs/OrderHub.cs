using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace TahaMarket.Infrastructure.Hubs
{
    public class OrderHub : Hub
    {
        // Admin connection ID (temporary static, later can be dynamic)
        public static string AdminConnectionId { get; set; }

        // Method to notify Admin of new order
        public async Task SendOrderNotification(string message)
        {
            if (!string.IsNullOrEmpty(AdminConnectionId))
            {
                await Clients.Client(AdminConnectionId).SendAsync("ReceiveOrder", message);
            }
        }

        // Track Admin connection
        public override Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var userType = httpContext.Request.Query["userType"];

            if (userType == "Admin")
            {
                AdminConnectionId = Context.ConnectionId;
            }

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(System.Exception? exception)
        {
            if (Context.ConnectionId == AdminConnectionId)
            {
                AdminConnectionId = null;
            }
            return base.OnDisconnectedAsync(exception);
        }
    }
}