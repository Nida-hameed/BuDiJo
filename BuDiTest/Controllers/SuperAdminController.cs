using BuDiTest.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Text.Encodings.Web;
using System.Text;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using BuDiTest.EmailSender;
using Microsoft.Extensions.Hosting;
using BuDiTest.Models;

namespace BuDiTest.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class SuperAdminController : Controller
    {

        private readonly ApplicationDbContext _Context;

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IMailSender _sender;
        public SuperAdminController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, IMailSender email, IWebHostEnvironment hostEnvironment)
        {
            _Context = dbContext;
            _userManager = userManager;
            _sender = email;
            _hostEnvironment = hostEnvironment;
        }
        public async Task<IActionResult> IndexAsync()
        {

            string userId = _userManager.GetUserId(User);
            int? DeptID = await _Context.Users.Where(x => x.Id == userId).Select(c => c.DepartmentID).FirstOrDefaultAsync();
            // Get the total number of tickets for the department
            ViewBag.TotalTickets = (from t in _Context.Tickets
                                    join u in _Context.Users on t.RaisedByEmployeeId equals u.Id
                                    join ur in _Context.UserRoles on u.Id equals ur.UserId
                                    join r in _Context.Roles on ur.RoleId equals r.Id
                                    select t).Count();

            // Get the number of pending tickets for the team
            ViewBag.PendingTickets = (from t in _Context.Tickets
                                      join u in _Context.Users on t.RaisedByEmployeeId equals u.Id
                                      join ur in _Context.UserRoles on u.Id equals ur.UserId
                                      join r in _Context.Roles on ur.RoleId equals r.Id
                                      where (t.Status == TicketStatus.InProgress || t.Status == TicketStatus.NotStarted)
                                      select t).Count();

            // Get the number of solved tickets for the team
            ViewBag.SolvedTickets = (from t in _Context.Tickets
                                     join u in _Context.Users on t.RaisedByEmployeeId equals u.Id
                                     join ur in _Context.UserRoles on u.Id equals ur.UserId
                                     join r in _Context.Roles on ur.RoleId equals r.Id
                                     where t.Status == TicketStatus.Completed
                                     select t).Count();
            return View();
        }

        public IActionResult UserIndex()
        {
            var getUser = (from user in _Context.Users
                           join userRoles in _Context.UserRoles on user.Id equals userRoles.UserId
                           join roles in _Context.Roles on userRoles.RoleId equals roles.Id
                           where roles.Name == "Manager"
                           select new Admin()
                           {
                               FirstName = user.FirstName,
                               LastName = user.LastName,
                               Email = user.Email,
                               Role = roles.Name,                              
                               Id = user.Id,
                               Mobile=user.PhoneNumber,
                               Image=user.ImgUrl
                           }).ToList();
            return View(getUser);
        }


        public IActionResult AddManager()
        {
            var getEmployee = _Context.Departments.ToList();
            if (getEmployee == null || getEmployee.Count == 0)
            {

                ViewBag.Department = null;
            }
            else
            {

                ViewBag.Department = new SelectList(_Context.Departments, "DepartmentID", "DepartmentName");
            }

            return View();
        }
        [HttpPost]

        public async Task<IActionResult> AddManager(User admin)
        {
            var check = admin.Email.ToString();        
            
            var available = _userManager.Users.Where(x => x.Email == check).FirstOrDefault();
            if (available == null)
            {
                string dateTime = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                string webRootPath = _hostEnvironment.WebRootPath;
                string fileName = Path.GetFileNameWithoutExtension(admin.Picture.FileName);
                string extension = Path.GetExtension(admin.Picture.FileName);
                string uniqueFileName = fileName + "_" + dateTime + extension;
                admin.ImgUrl = uniqueFileName;
                string filePath = Path.Combine(webRootPath, "UserImages", uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    admin.Picture.CopyTo(fileStream);
                }
                string userID = _userManager.GetUserId(User);

                var user = new ApplicationUser()
                {
                    FirstName = admin.FirstName,
                    LastName = admin.LastName,
                    Email = admin.Email,
                    UserName = admin.Email,
                    CreateDateTime = System.DateTime.Now,                 
                    CreatedBy = userID,
                    PhoneNumber=admin.MobileNumber,
                    ImgUrl=admin.ImgUrl,
                    DepartmentID = admin.DepartmentID,
                };

                var userEmail = admin.Email;
                var UserPassword = admin.Password;

                IdentityResult result = await _userManager.CreateAsync(user, admin.Password);
                var userId = await _userManager.GetUserIdAsync(user);
                
                var message = new Message(user.Email, "Your Login Details",
                $"Here is Your credentials:<br/> your Email is: {userEmail} <br/> your password is:  {UserPassword}", "");
                _sender.MessageSend(message);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Manager");
                    return RedirectToAction("UserIndex");
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "User Email already Taken!");

                return View();

            }

            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<IActionResult> EditManager(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var model = new User
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email=user.Email,
                MobileNumber = user.PhoneNumber
            };

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> EditManager(string id, User model)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.PhoneNumber = model.MobileNumber;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return RedirectToAction("UserIndex");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteManager(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                return new JsonResult("Manager Delete Successfully");
            }

            else
            {
                return new JsonResult("System Error! Manager is not Delete Successfully");

            }
        }
       
        public IActionResult EmployeeIndex()
        {
            var getEmployee = (from user in _Context.Users
                           join userRoles in _Context.UserRoles on user.Id equals userRoles.UserId
                           join roles in _Context.Roles on userRoles.RoleId equals roles.Id
                           where roles.Name == "Employee"
                           select new Admin()
                           {
                               FirstName = user.FirstName,
                               LastName = user.LastName,
                               Email = user.Email,
                               Role = roles.Name,
                               Id = user.Id,
                               Mobile = user.PhoneNumber,
                               Image = user.ImgUrl
                           }).ToList();
            return View(getEmployee);
        }
        
        public IActionResult AddEmployee()
        {
            var managerlist = (from user in _Context.Users
                               join userRoles in _Context.UserRoles on user.Id equals userRoles.UserId
                               join Role in _Context.Roles on userRoles.RoleId equals Role.Id
                               where Role.Name == "Manager"
                               select user).ToList();

            if (managerlist == null || managerlist.Count == 0)
            {

                ViewBag.Itemss = null;
            }
            else
            {

                var usersWithPictures = (from user in _Context.Users
                                         join userRoles in _Context.UserRoles on user.Id equals userRoles.UserId
                                         join role in _Context.Roles on userRoles.RoleId equals role.Id
                                         where role.Name == "Manager"
                                         select new UserWithPicture
                                         {
                                             Id = user.Id,
                                             FullName = $"{user.FirstName} {user.LastName}",
                                             PicturePath = user.ImgUrl // Replace with the actual property name for the image path in your model
                                         }).ToList();

                foreach (var user in usersWithPictures)
                {
                    var imagePath = Path.Combine("~/UserImages/", user.PicturePath.TrimStart('\\', '/'));
                    user.PicturePath = Url.Content(imagePath);
                }

                ViewBag.UsersWithPictures = usersWithPictures;

            }
           
            var getEmployee = _Context.Departments.ToList();
            if (getEmployee == null || getEmployee.Count == 0)
            {

                ViewBag.Department = null;
            }
            else
            {

                ViewBag.Department = new SelectList(_Context.Departments, "DepartmentID", "DepartmentName");
            }

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> AddEmployee(User admin)
        {
            var check = admin.Email.ToString();
            var available = _userManager.Users.Where(x => x.Email == check).FirstOrDefault();
            if (available == null)
            {
                
                    string dateTime = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    string webRootPath = _hostEnvironment.WebRootPath;
                    string fileName = Path.GetFileNameWithoutExtension(admin.Picture.FileName);
                    string extension = Path.GetExtension(admin.Picture.FileName);
                    string uniqueFileName = fileName + "_" + dateTime + extension;
                    admin.ImgUrl = uniqueFileName;
                    string filePath = Path.Combine(webRootPath, "UserImages", uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        admin.Picture.CopyTo(fileStream);
                    }
                    string userID = _userManager.GetUserId(User);


                var user = new ApplicationUser()
                {
                    FirstName = admin.FirstName,
                    LastName = admin.LastName,
                    Email = admin.Email,
                    UserName = admin.Email,
                    CreateDateTime = System.DateTime.Now,
                    CreatedBy = userID,
                    PhoneNumber = admin.MobileNumber,
                    DepartmentID=admin.DepartmentID,
                    ImgUrl= admin.ImgUrl
                };

                var userEmail = admin.Email;
                var UserPassword = admin.Password;

                IdentityResult result = await _userManager.CreateAsync(user, admin.Password);
                var userId = await _userManager.GetUserIdAsync(user);

                var message = new Message(user.Email, "Your Login Details",
                $"Here is Your credentials:<br/> your Email is: {userEmail} <br/> your password is:  {UserPassword}", "");
                _sender.MessageSend(message);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Employee");
                    return RedirectToAction("EmployeeIndex");
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "User Email already Taken!");

                return View();

            }

            return RedirectToAction("Index");
        }
        public async Task<IActionResult> EditEmployee(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var model = new User
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email=user.Email,
                MobileNumber = user.PhoneNumber
            };

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> EditEmployee(string id, User model)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.MobileNumber;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return RedirectToAction("EmployeeIndex");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteEmployee(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);


            if (result.Succeeded)
            {
                return new JsonResult("Employee Delete Successfully");
            }

            else
            {
                return new JsonResult("System Error! Employee is not Delete Successfully");

            }
        }
        public IActionResult OpenTasks()
        {
            string userid = _userManager.GetUserId(User);
            var DepatmentId = (from user in _Context.Users where user.Id == userid select user.DepartmentID).FirstOrDefault();
            var OpenTasks = _Context.Task.Where(x => x.Status == Task_Status.InProgress).Select(x => new TaskIndex
                             {
                                 TaskID = x.TaskID,
                                 TaskName = x.TaskName,
                                 Description = x.Description,
                                 DueDate = x.DueDate,
                                 RaisedByEmployeeId = _Context.Users.Where(z => z.Id == x.RaisedByEmployeeId).Select(y => y.FirstName).FirstOrDefault(),
                                 AssignedToEmployeeId = _Context.Users.Where(z => z.Id == x.AssignedToEmployeeId).Select(y => y.FirstName).FirstOrDefault(),
                                 Status = x.Status,
                                 Priority = x.Priority,
                                 AttchmentName = x.AttchmentName
                             }).ToList();

            return View(OpenTasks);
        }
        public IActionResult NotStarted()
        {
            string userid = _userManager.GetUserId(User);
            var DepatmentId = (from user in _Context.Users where user.Id == userid select user.DepartmentID).FirstOrDefault();
            var OpenTasks = _Context.Task.Where(x => x.Status == Task_Status.NotStarted).Select(x => new TaskIndex
                             {
                                 TaskID = x.TaskID,
                                 TaskName = x.TaskName,
                                 Description = x.Description,
                                 DueDate = x.DueDate,
                                 RaisedByEmployeeId = _Context.Users.Where(z => z.Id == x.RaisedByEmployeeId).Select(y => y.FirstName).FirstOrDefault(),
                                 AssignedToEmployeeId = _Context.Users.Where(z => z.Id == x.AssignedToEmployeeId).Select(y => y.FirstName).FirstOrDefault(),
                                 Status = x.Status,
                                 Priority = x.Priority,
                                 AttchmentName = x.AttchmentName
                             }).ToList();

            return View(OpenTasks);
        }
        public IActionResult Completed()
        {
            string userid = _userManager.GetUserId(User);
            var DepatmentId = (from user in _Context.Users where user.Id == userid select user.DepartmentID).FirstOrDefault();
            var OpenTasks = _Context.Task.Where(x => x.Status == Task_Status.Completed).Select(x => new TaskIndex
                             {
                                 TaskID = x.TaskID,
                                 TaskName = x.TaskName,
                                 Description = x.Description,
                                 DueDate = x.DueDate,
                                 RaisedByEmployeeId = _Context.Users.Where(z => z.Id == x.RaisedByEmployeeId).Select(y => y.FirstName).FirstOrDefault(),
                                 AssignedToEmployeeId = _Context.Users.Where(z => z.Id == x.AssignedToEmployeeId).Select(y => y.FirstName).FirstOrDefault(),
                                 Status = x.Status,
                                 Priority = x.Priority,
                                 AttchmentName = x.AttchmentName
                             }).ToList();

            return View(OpenTasks);
        }
        public IActionResult TaskDetail(int Id)
        {
            var taskDetail = _Context.Task.Where(x => x.TaskID == Id).FirstOrDefault();
            return View(taskDetail);
        }
        //    public IActionResult Tickets()
        //    {

        //        var tickets = _Context.Tickets

        //.Select(x => new TicketsIndex
        //{
        //    TicketID = x.TicketID,
        //    Title = x.Title,
        //    Description = x.Description,
        //    CreatedDate = x.CreatedDate,
        //    RaisedByEmployeeId = _Context.Users.Where(z => z.Id == x.RaisedByEmployeeId).Select(y => y.FirstName).FirstOrDefault(),
        //    Status = x.Status,
        //    AttchmentName = x.AttchmentName
        //})
        //.ToList();
        //        return View(tickets);
        //    }
        public IActionResult OpenTickets()
        {
            string userid = _userManager.GetUserId(User);
            var DepatmentId = (from user in _Context.Users where user.Id == userid select user.DepartmentID).FirstOrDefault();
            var OpenTickets = _Context.Tickets
                               .Where(x => x.Status == TicketStatus.InProgress).Select(x => new TicketsIndex
                               {
                                   TicketID = x.TicketID,
                                   Title = x.Title,
                                   Description = x.Description,
                                   CreatedDate = x.CreatedDate,
                                   RaisedByEmployeeId = _Context.Users.Where(z => z.Id == x.RaisedByEmployeeId).Select(y => y.FirstName).FirstOrDefault(),
                                   Status = x.Status,
                                   AssignedTo = _Context.Users.Where(z => z.Id == x.AssignedToEmployeeId).Select(y => y.FirstName).FirstOrDefault(),
                                   AttchmentName = x.AttchmentName
                               }).ToList();

            return View(OpenTickets);
        }
        public IActionResult NotStartedTickets()
        {
            string userid = _userManager.GetUserId(User);
            var DepatmentId = (from user in _Context.Users where user.Id == userid select user.DepartmentID).FirstOrDefault();
            var OpenTickets = _Context.Tickets
                               .Where(x => x.Status == TicketStatus.NotStarted).Select(x => new TicketsIndex
                               {
                                   TicketID = x.TicketID,
                                   Title = x.Title,
                                   Description = x.Description,
                                   CreatedDate = x.CreatedDate,
                                   RaisedByEmployeeId = _Context.Users.Where(z => z.Id == x.RaisedByEmployeeId).Select(y => y.FirstName).FirstOrDefault(),
                                   AssignedTo = _Context.Users.Where(z => z.Id == x.AssignedToEmployeeId).Select(y => y.FirstName).FirstOrDefault(),
                                   Status = x.Status,
                                   AttchmentName = x.AttchmentName
                               }).ToList();

            return View(OpenTickets);
        }
        public IActionResult CompletedTickets()
        {
            string userid = _userManager.GetUserId(User);
            var DepatmentId = (from user in _Context.Users where user.Id == userid select user.DepartmentID).FirstOrDefault();
            var OpenTickets =_Context.Tickets
                               .Where(x => x.Status == TicketStatus.Completed).Select(x => new TicketsIndex
                               {
                                   TicketID = x.TicketID,
                                   Title = x.Title,
                                   Description = x.Description,
                                   CreatedDate = x.CreatedDate,
                                   RaisedByEmployeeId = _Context.Users.Where(z => z.Id == x.RaisedByEmployeeId).Select(y => y.FirstName).FirstOrDefault(),
                                   Status = x.Status,
                                   AssignedTo = _Context.Users.Where(z => z.Id == x.AssignedToEmployeeId).Select(y => y.FirstName).FirstOrDefault(),
                                   AttchmentName = x.AttchmentName
                               }).ToList();

            return View(OpenTickets);
        }
        public IActionResult Detail(int Id)
        {
            var ticktetDetail=_Context.Tickets.Where(x=>x.TicketID==Id).FirstOrDefault();
            return View(ticktetDetail);
        }
        public IActionResult GetPdf(int id)
        {
            var getPdf = _Context.Tickets.Where(x => x.TicketID == id).Select(x => x.AttchmentName).FirstOrDefault();
            var memory = DownloadFile(getPdf, "wwwroot");
            return File(memory.ToArray(), "application/pdf", getPdf);
        }
        public IActionResult GetTaskPdf(int id)
        {
            var getPdf = _Context.Task.Where(x => x.TaskID == id).Select(x => x.AttchmentName).FirstOrDefault();
            var memory = DownloadFile(getPdf, "wwwroot");
            return File(memory.ToArray(), "application/pdf", getPdf);
        }
        private MemoryStream DownloadFile(string filename, string uploadPath)
        {
            var path = uploadPath + "/" + filename;
            var memory = new MemoryStream();
            if (System.IO.File.Exists(path))
            {
                using (var fileStream = new System.IO.FileStream(path, System.IO.FileMode.Open))
                {
                    fileStream.CopyTo(memory);
                }
            }
            memory.Position = 0;
            return memory;
        }
    }
}
