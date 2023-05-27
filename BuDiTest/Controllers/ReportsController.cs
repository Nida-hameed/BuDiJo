using BuDiTest.Areas.Identity.Data;
using BuDiTest.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BuDiTest.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _Context;
        private readonly UserManager<ApplicationUser> _userManager;
        public ReportsController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            _Context = dbContext;
            _userManager = userManager;
        }

        public IActionResult Index()
        {

            string userId = _userManager.GetUserId(User);
            var getuser = _Context.Users.Find(userId);
            var employeeslist = (from user in _Context.Users
                                 join userRoles in _Context.UserRoles on user.Id equals userRoles.UserId
                                 join role in _Context.Roles on userRoles.RoleId equals role.Id
                                 join department in _Context.Departments on user.DepartmentID equals department.DepartmentID
                                 where role.Name == "Employee" && department.DepartmentID == user.DepartmentID
                                 select user).ToList();
            employeeslist = employeeslist.Where(x => x.DepartmentID == getuser.DepartmentID).ToList();
            if (employeeslist == null || employeeslist.Count == 0)
            {
          
            }
            else
            {
                var usersWithPictures = (from user in _Context.Users
                                         join userRoles in _Context.UserRoles on user.Id equals userRoles.UserId
                                         join role in _Context.Roles on userRoles.RoleId equals role.Id
                                         where role.Name == "Employee"
                                         select new UserWithPicture
                                         {
                                             Id = user.Id,
                                             FullName = $"{user.FirstName} {user.LastName}",
                                             PicturePath = user.ImgUrl,
                                             DepartmentID = user.DepartmentID// Replace with the actual property name for the image path in your model
                                         }).ToList();
                usersWithPictures = usersWithPictures.Where(x => x.DepartmentID == getuser.DepartmentID).ToList();
                foreach (var user in usersWithPictures)
                {
                    var imagePath = Path.Combine("~/UserImages/", user.PicturePath.TrimStart('\\', '/'));
                    user.PicturePath = Url.Content(imagePath);
                }

                ViewBag.UsersWithPictures = usersWithPictures;

            }
            return View();
        }
        [HttpGet]
        public IActionResult Report()
        
        {

            string userId = _userManager.GetUserId(User);
            var getuser = _Context.Users.Find(userId);
            var employeeslist = (from user in _Context.Users
                                 join userRoles in _Context.UserRoles on user.Id equals userRoles.UserId
                                 join role in _Context.Roles on userRoles.RoleId equals role.Id
                                 join department in _Context.Departments on user.DepartmentID equals department.DepartmentID
                                 where (role.Name == "Employee" && department.DepartmentID == user.DepartmentID) ||
                                       (role.Name == "Manager" && department.DepartmentID == user.DepartmentID)
                                 select user).ToList();
            if (employeeslist == null || employeeslist.Count == 0)
            {

            }
            else
            {
                var usersWithPictures = (from user in _Context.Users
                                         join userRoles in _Context.UserRoles on user.Id equals userRoles.UserId
                                         join role in _Context.Roles on userRoles.RoleId equals role.Id
                                         where (role.Name == "Employee") ||(role.Name=="Manager")
                                         select new UserWithPicture
                                         {
                                             Id = user.Id,
                                             FullName = $"{user.FirstName} {user.LastName}",
                                             PicturePath = user.ImgUrl,
                                             // Replace with the actual property name for the image path in your model
                                         }).ToList();

                foreach (var user in usersWithPictures)
                {
                    var imagePath = Path.Combine("~/UserImages/", user.PicturePath.TrimStart('\\', '/'));
                    user.PicturePath = Url.Content(imagePath);
                }

                ViewBag.UsersWithPictures = usersWithPictures;

            }
            return View();
        }

        #region API CALL
        public IActionResult GetReport(DateTime? StartDate, DateTime? EndDate, string UserId)
        {
            if (StartDate == null)
            {
                StartDate = DateTime.Today;
            }
            if (EndDate == null)
            {
                EndDate = DateTime.Now;
            }
            ViewBag.StartDate = StartDate.Value.ToString("s");
            ViewBag.EndDate = EndDate.Value.ToString("s");
            var report = _Context.ClockEvents.Where(x =>x.ClockInTime >= StartDate && x.ClockInTime <= EndDate).ToList();
            if (UserId != null)
            {
                report = report.Where(x => x.UserId == UserId).ToList();
            }
            List<Report> reports = new List<Report>();

            foreach (var item in report)
            {
                Report obj = new Report();
                obj.ClockIn = item.ClockInTime.ToString("hh:mm:ss tt");
                obj.ClockOut = item.ClockOutTime?.ToString("hh:mm:ss tt") ?? "N/A";
                string id = item.UserId;
                obj.Name = _Context.Users.Where(x => x.Id == id).Select(x => x.FirstName).FirstOrDefault();
                reports.Add(obj);
            }

            return Json(new { data = reports });
        }   
        

        public IActionResult GetAdminReport(DateTime? StartDate, DateTime? EndDate, string UserId)
        {
          
            if (StartDate == null)
            {
                StartDate = DateTime.Today;
            }
            if (EndDate == null)
            {
                EndDate = DateTime.Now;
            }
            ViewBag.StartDate = StartDate.Value.ToString("s");
            ViewBag.EndDate = EndDate.Value.ToString("s");
            var report = _Context.ClockEvents.Where(x =>x.ClockInTime >= StartDate && x.ClockInTime <= EndDate).ToList();
            if (UserId != null)
            {
                report = report.Where(x => x.UserId == UserId).ToList();
            }
            List<Report> reports = new List<Report>();

            foreach (var item in report)
            {
                Report obj = new Report();
                obj.ClockIn = item.ClockInTime.ToString("hh:mm:ss tt");
                obj.ClockOut = item.ClockOutTime?.ToString("hh:mm:ss tt") ?? "N/A";
                string id = item.UserId;
                obj.Name = _Context.Users.Where(x => x.Id == id).Select(x => x.FirstName).FirstOrDefault();
                reports.Add(obj);
            }

            return Json(new { data = reports });
        }
        #endregion
    }
}
