using BuDiTest.Areas.Identity.Data;
using BuDiTest.EmailSender;
using BuDiTest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Net.Sockets;

namespace BuDiTest.Controllers
{
    [Authorize(Roles = "Manager")]
    public class ManagerController : Controller
    {
        private readonly ApplicationDbContext _Context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IMailSender _sender;
        private readonly RoleManager<IdentityRole> _roleManager;
        public ManagerController(
            ApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager,
            IMailSender email,
            IWebHostEnvironment hostEnvironment,
            RoleManager<IdentityRole> roleManager)
        {
            _Context = dbContext;
            _userManager = userManager;
            _sender = email;
            _hostEnvironment = hostEnvironment;
            _roleManager = roleManager;
        }
        public async Task<IActionResult> Index()
        {
            string userId = _userManager.GetUserId(User);
            int? DeptID = await _Context.Users.Where(x => x.Id == userId).Select(c => c.DepartmentID).FirstOrDefaultAsync();
            // Get the total number of tickets for the department
            ViewBag.TotalTickets = (from t in _Context.Tickets
                                    join u in _Context.Users on t.RaisedByEmployeeId equals u.Id
                                    join ur in _Context.UserRoles on u.Id equals ur.UserId
                                    join r in _Context.Roles on ur.RoleId equals r.Id
                                    where r.Name != "SUPERADMIN" && u.DepartmentID == DeptID
                                    select t).Count();

            // Get the number of pending tickets for the team
            ViewBag.PendingTickets = (from t in _Context.Tickets
                                      join u in _Context.Users on t.RaisedByEmployeeId equals u.Id
                                      join ur in _Context.UserRoles on u.Id equals ur.UserId
                                      join r in _Context.Roles on ur.RoleId equals r.Id
                                      where r.Name != "SUPERADMIN" && u.DepartmentID == DeptID && (t.Status == TicketStatus.InProgress || t.Status == TicketStatus.NotStarted)
                                      select t).Count();

            // Get the number of solved tickets for the team
            ViewBag.SolvedTickets = (from t in _Context.Tickets
                                     join u in _Context.Users on t.RaisedByEmployeeId equals u.Id
                                     join ur in _Context.UserRoles on u.Id equals ur.UserId
                                     join r in _Context.Roles on ur.RoleId equals r.Id
                                     where r.Name != "SUPERADMIN" && u.DepartmentID == DeptID && t.Status == TicketStatus.Completed
                                     select t).Count();

            // Get the personal solving for the tickets of the employee
            ViewBag.PersonalSolving = (from t in _Context.Tickets
                                       join u in _Context.Users on t.RaisedByEmployeeId equals u.Id
                                       join ur in _Context.UserRoles on u.Id equals ur.UserId
                                       join r in _Context.Roles on ur.RoleId equals r.Id
                                       where r.Name != "SUPERADMIN" && u.DepartmentID == DeptID && t.Status == TicketStatus.Completed && t.AssignedToEmployeeId == userId
                                       select t).Count();

            return View();
        }

        public IActionResult EmployeeIndex()
        {
            string userid = _userManager.GetUserId(User);
            string AdminId = _Context.Users.Where(x => x.CreatedBy == null).Select(x => x.Id).FirstOrDefault();
            var getEmployee = (from u in _Context.Users
                               join ur in _Context.UserRoles on u.Id equals ur.UserId
                               join r in _Context.Roles on ur.RoleId equals r.Id
                               join d in _Context.Departments on u.DepartmentID equals d.DepartmentID
                               where (u.CreatedBy == userid ||u.CreatedBy == AdminId )&& r.Name == "Employee"
                               select new Admin()
                               {
                                   FirstName = u.FirstName,
                                   LastName = u.LastName,
                                   Email = u.Email,
                                   Role = r.Name,
                                   Id = u.Id,
                                   Mobile = u.PhoneNumber,
                                   Image = u.ImgUrl
                               }).ToList();
            return View(getEmployee);
        }

        public IActionResult AddEmployee()
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
        public async Task<IActionResult> AddEmployee(User admin)
        {
            var available = _userManager.Users.Where(x => x.Email == admin.Email.ToString()).FirstOrDefault();
            if (available == null)
            {
                string userID = _userManager.GetUserId(User);
                int? DeptID = await _Context.Users.Where(x => x.Id == userID).Select(c => c.DepartmentID).FirstOrDefaultAsync();

                var user = new ApplicationUser()
                {
                    FirstName = admin.FirstName,
                    LastName = admin.LastName,
                    Email = admin.Email,
                    UserName = admin.Email,
                    CreateDateTime = DateTime.Now,
                    CreatedBy = userID,
                    PhoneNumber = admin.MobileNumber,
                    DepartmentID = DeptID,
                    ImgUrl = await SavePicture(admin.Picture) ?? ""
                };

                var userEmail = admin.Email;
                var UserPassword = admin.Password;

                var result = await _userManager.CreateAsync(user, admin.Password);

                var message = new Message(user.Email, "Your Login Details",
                $"Here is Your credentials:<br/> your Email is: {userEmail} <br/> your password is:  {UserPassword}", "");
                _sender.MessageSend(message);
                if (result.Succeeded)
                {
                    if (!await _roleManager.RoleExistsAsync("Employee"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole("Employee"));
                    }
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

        private async Task<string> SavePicture(IFormFile picture)
        {
            var fileName = $"{Guid.NewGuid().ToString()}{Path.GetExtension(picture.FileName)}";
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "UserImages", fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await picture.CopyToAsync(stream);
            }

            return fileName;
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
                Email = user.Email,
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
        public IActionResult Tickets()
        {
            string userId = _userManager.GetUserId(User);
            var tickets = _Context.Tickets
                        .Where(x => x.AssignedToEmployeeId == userId && x.Type == "otherTeam")
                        .Select(x => new TicketsIndex
                        {
                            TicketID = x.TicketID,
                            Title = x.Title,
                            Description = x.Description,
                            CreatedDate = x.CreatedDate,
                            RaisedByEmployeeId = _Context.Users.Where(z => z.Id == x.RaisedByEmployeeId).Select(y => y.FirstName).FirstOrDefault(),
                            Status = x.Status,
                            AttchmentName = x.AttchmentName
                        })
                        .ToList();
            return View(tickets);
        }
    
        public IActionResult OpenTickets()
        {
            string userId = _userManager.GetUserId(User);
            var tickets = _Context.Tickets
                            .Where(x => x.Type == "Team" && x.Status==TicketStatus.InProgress)
                            .Select(x => new TicketsIndex
                            {
                                TicketID = x.TicketID,
                                Title = x.Title,
                                Description = x.Description ?? "",
                                CreatedDate = x.CreatedDate,
                                RaisedByEmployeeId = _Context.Users.Where(z => z.Id == x.RaisedByEmployeeId).Select(y => y.FirstName).FirstOrDefault() ?? "",
                                Status = x.Status,
                                AttchmentName = x.AttchmentName
                            })
                            .ToList();
            return View(tickets);
        }

        public IActionResult Detail(int Id)
        {
            var ticktetDetail = _Context.Tickets.Where(x => x.TicketID == Id).FirstOrDefault();
            return View(ticktetDetail);
        }
        [HttpGet]
        public async Task<IActionResult> AssignedTo()
        {
            string userId = _userManager.GetUserId(User);
            int? DepId = await _Context.Users.Where(x => x.Id == userId).Select(c => c.DepartmentID).FirstOrDefaultAsync();

            var usersWithPictures = await(from u in _Context.Users
                                          join ur in _Context.UserRoles on u.Id equals ur.UserId
                                          join r in _Context.Roles on ur.RoleId equals r.Id
                                          where r.Name != "SUPERADMIN" && r.Name != "MANAGER" && u.DepartmentID == DepId
                                          select new UserWithPicture
                                          {
                                              Id = u.Id,
                                              FullName = $"{u.FirstName} {u.LastName}",
                                              PicturePath = u.ImgUrl ?? ""
                                          }).ToListAsync();

            foreach (var user in usersWithPictures)
            {
                var imagePath = Path.Combine("~/UserImages/", user.PicturePath.TrimStart('\\', '/'));
                user.PicturePath = Url.Content(imagePath);
            }

            ViewBag.UsersWithPictures = usersWithPictures;
            return View();
        }
        [HttpPost]
        public IActionResult AssignedTo(AssignTicket ticket)
        {
            try
            {
                string RaisedById = _userManager.GetUserId(User);

                if (ticket.Attachments != null && ticket.Attachments.Length > 0)
                {
                    var filePath = Path.Combine(
                        Directory.GetCurrentDirectory(), "wwwroot", "Attachments",
                        ticket.Attachments.FileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        ticket.Attachments.CopyToAsync(fileStream);
                    }

                    ticket.AttchmentName = "/Attachments/" + ticket.Attachments.FileName;
                }
                var obj = new Ticket()
                {
                    Title = ticket.Title,
                    Description = ticket.Description,
                    CreatedDate = System.DateTime.Now,
                    RaisedByEmployeeId = RaisedById,
                    AttchmentName = ticket.AttchmentName,
                    AssignedToEmployeeId = ticket.UserId,
                    Status = TicketStatus.NotStarted,
                    Type = "Team",
                };
                _Context.Add(obj);
                _Context.SaveChanges();
                Notification notification = new Notification();
                notification.UserId = ticket.UserId;
                notification.Message = "A New Ticket Is Assigned";
                notification.IsRead = false;
                _Context.Add(notification);
                _Context.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return View("Error");
            }
        }

        [HttpGet]
        public IActionResult AssignTicket(int ticketId)
        {
            string userId = _userManager.GetUserId(User);
            var ticket = _Context.Tickets.Find(ticketId);
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
        [HttpPost]
        public IActionResult AssignTicket(Ticket ticket)
        {
            try
            {
                string RaisedById = _userManager.GetUserId(User);
                var obj = _Context.Tickets.Where(x => x.TicketID == ticket.TicketID).FirstOrDefault();
                obj.AssignedToEmployeeId = ticket.AssignedToEmployeeId;
                obj.Status = TicketStatus.InProgress;
                obj.Type = "Team";
                obj.CreatedDate = System.DateTime.Now;
                obj.RaisedByEmployeeId = RaisedById;
                _Context.Update(obj);
                _Context.SaveChanges();
                Notification notification = new Notification();
                notification.UserId = ticket.AssignedToEmployeeId;
                notification.Message = "A New Tickeck Is Assigned";
                notification.IsRead = false;
                _Context.Add(notification);
                _Context.SaveChanges();
                return RedirectToAction("Index");
            }

            catch (Exception ex)
            {
                return View("Error");
            }
        }


        [HttpGet]
        public IActionResult AssignTask()
        {
            string userId = _userManager.GetUserId(User);
            var usersWithPictures = (from user in _Context.Users
                                     join userRoles in _Context.UserRoles on user.Id equals userRoles.UserId
                                     join role in _Context.Roles on userRoles.RoleId equals role.Id
                                     where role.Name == "Employee" && user.CreatedBy == userId
                                     select new UserWithPicture
                                     {
                                         Id = user.Id,
                                         FullName = $"{user.FirstName} {user.LastName}",
                                         PicturePath = user.ImgUrl,
                                         DepartmentID = user.DepartmentID// Replace with the actual property name for the image path in your model
                                     }).ToList();
            foreach (var user in usersWithPictures)
            {
                var imagePath = Path.Combine("~/UserImages/", user.PicturePath.TrimStart('\\', '/'));
                user.PicturePath = Url.Content(imagePath);
            }

            ViewBag.UsersWithPictures = usersWithPictures;
            return View();
        }
        [HttpPost]
        public IActionResult AssignTask(MyTask task)
        {
            try
            {
                string RaisedById = _userManager.GetUserId(User);
                if (task.Attachments != null && task.Attachments.Length > 0)
                {
                    var filePath = Path.Combine(
                        Directory.GetCurrentDirectory(), "wwwroot", "TaskAttachments",
                        task.Attachments.FileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        task.Attachments.CopyToAsync(fileStream);
                    }

                    task.AttchmentName = "/TaskAttachments/" + task.Attachments.FileName;
                }
                task.RaisedByEmployeeId = RaisedById;
                task.AttchmentName = task.AttchmentName;
                task.Status = Task_Status.NotStarted;
                _Context.Add(task);
                _Context.SaveChanges();
                Notification obj = new Notification();
                obj.UserId = task.AssignedToEmployeeId;
                obj.Message = "A New Task Is Assigned";
                obj.IsRead = false;
                _Context.Add(obj);
                _Context.SaveChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                return View("Error");
            }
        }
        [HttpPost]
        public IActionResult ClockIn()
        {
            try
            {
                ClockEvent obj = new ClockEvent();
                obj.ClockInTime = DateTime.Now;
                obj.UserId = _userManager.GetUserId(User);
                obj.Type = ClockEventType.ClockIn;
                _Context.Add(obj);
                _Context.SaveChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                return View("Error");
            }
        }
        [HttpPost]
        public IActionResult ClockOut()
        {
            try
            {
                string userId = _userManager.GetUserId(User);
                var clockEvent = _Context.ClockEvents.Where(x => x.UserId == userId && x.ClockOutTime == null).FirstOrDefault();
                clockEvent.ClockOutTime = DateTime.Now;
                clockEvent.Type = ClockEventType.ClockOut;
                _Context.Update(clockEvent);
                _Context.SaveChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                return View("Error");
            }
        }
        public IActionResult OpenTasks()
        {
            string userid = _userManager.GetUserId(User);
            var DepatmentId = (from user in _Context.Users where user.Id == userid select user.DepartmentID).FirstOrDefault();
            var OpenTasks = (from task in _Context.Task
                             where _Context.Users
                                   .Where(user => user.DepartmentID == DepatmentId)
                                   .Select(user => user.Id)
                                   .Contains(task.AssignedToEmployeeId)
                             select task).Where(x => x.Status == Task_Status.InProgress).Select(x => new TaskIndex
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
            var OpenTasks = (from task in _Context.Task
                             where _Context.Users
                                   .Where(user => user.DepartmentID == DepatmentId)
                                   .Select(user => user.Id)
                                   .Contains(task.AssignedToEmployeeId)
                             select task).Where(x => x.Status == Task_Status.NotStarted).Select(x => new TaskIndex
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
            var OpenTasks = (from task in _Context.Task
                             where _Context.Users
                                   .Where(user => user.DepartmentID == DepatmentId)
                                   .Select(user => user.Id)
                                   .Contains(task.AssignedToEmployeeId)
                             select task).Where(x => x.Status == Task_Status.Completed).Select(x => new TaskIndex
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
        public IActionResult NotStartedTickets()
        {
            string userid = _userManager.GetUserId(User);
            var DepatmentId = (from user in _Context.Users where user.Id == userid select user.DepartmentID).FirstOrDefault();
            var OpenTickets = (from ticket in _Context.Tickets
                               where _Context.Users
                                     .Where(user => user.DepartmentID == DepatmentId)
                                     .Select(user => user.Id)
                                     .Contains(ticket.AssignedToEmployeeId)
                               select ticket).Where(x => x.Status == TicketStatus.NotStarted).Select(x => new TicketsIndex
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
            var OpenTickets = (from ticket in _Context.Tickets
                               where _Context.Users
                                     .Where(user => user.DepartmentID == DepatmentId)
                                     .Select(user => user.Id)
                                     .Contains(ticket.AssignedToEmployeeId)
                               select ticket).Where(x => x.Status == TicketStatus.Completed).Select(x => new TicketsIndex
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
        [HttpGet]
        public IActionResult ChangeStatus(int ticketId)
        {
            var obj = _Context.Tickets.Where(x => x.TicketID == ticketId).FirstOrDefault();
            return View(obj);
        }

        [HttpPost]
        public IActionResult ChangeStatus(Ticket ticket)
        {
            try
            {
                string RaisedById = _userManager.GetUserId(User);
                var obj = _Context.Tickets.Where(x => x.TicketID == ticket.TicketID).FirstOrDefault();
                obj.Status = ticket.Status;
                _Context.Update(obj);
                _Context.SaveChanges();
                return RedirectToAction("Tickets");
            }
            catch (Exception ex)
            {
                return View("Error");
            }
        }
        [HttpGet]
        public IActionResult TaskStatus(int taskId)
        {
            var obj = _Context.Task.Where(x => x.TaskID == taskId).FirstOrDefault();
            return View(obj);
        }
        [HttpPost]
        public IActionResult TaskStatus(MyTask task)
        {
            try
            {
                string RaisedById = _userManager.GetUserId(User);
                var obj = _Context.Task.Where(x => x.TaskID == task.TaskID).FirstOrDefault();
                obj.Status = task.Status;
                _Context.Update(obj);
                _Context.SaveChanges();
                return RedirectToAction("Tasks");
            }
            catch (Exception ex)
            {
                return View("Error");
            }
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


