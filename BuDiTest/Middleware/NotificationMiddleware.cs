using System.Security.Claims;
using System.Threading.Tasks;
using BuDiTest.Areas.Identity.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace BuDiTest.Middleware
{
    public class NotificationMiddleware
    {
        private readonly RequestDelegate _next;

        public NotificationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            using var scope = context.RequestServices.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            if (context.User.Identity.IsAuthenticated)
            {
                string userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var notifications = dbContext.Notifications
                    .Where(n => n.UserId == userId && n.IsRead==false)
                    .ToList();

                context.Items["Notifications"] = notifications;
            }

            await _next(context);
        }
    }
}
