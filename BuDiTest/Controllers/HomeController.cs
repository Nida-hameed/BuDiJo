using BuDiTest.Areas.Identity.Data;
using BuDiTest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BuDiTest.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _Context;
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext dbContext)
        {
            _logger = logger;
            _Context = dbContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public IActionResult MarkAsRead(int notificationId)
        {
            try
            {
                var obj = _Context.Notifications.Where(x => x.NotificationId == notificationId).FirstOrDefault();
                if (obj != null)
                {
                    obj.IsRead = true;  
                    _Context.Notifications.Update(obj); 
                    _Context.SaveChanges(); 
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return View("Error");
            }
        }

    }
}