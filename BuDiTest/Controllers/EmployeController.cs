using BuDiTest.Areas.Identity.Data;
using BuDiTest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System;
using System.Net.Mail;
using System.Net.Sockets;

namespace BuDiTest.Controllers
{
    [Authorize(Roles = "Employee")]
    public class EmployeController : Controller
    {
        private readonly ApplicationDbContext _Context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _hostEnvironment;
        public EmployeController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, IWebHostEnvironment hostEnvironment)
        {
            _Context = dbContext;
            _userManager = userManager;
            _hostEnvironment = hostEnvironment;
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
        [HttpGet]
        public IActionResult AssignedTo()
        {
            return View();
        }
        [HttpGet]
        public async Task<JsonResult> GetData(string assignedTo)
        {
            string userId = _userManager.GetUserId(User);
            int? DepId = await _Context.Users.Where(x => x.Id == userId).Select(c => c.DepartmentID).FirstOrDefaultAsync();

            if (assignedTo == "Team")
            {
                var usersWithPictures = await (from u in _Context.Users
                                               join ur in _Context.UserRoles on u.Id equals ur.UserId
                                               join r in _Context.Roles on ur.RoleId equals r.Id
                                               where r.Name != "SUPERADMIN"  && u.DepartmentID == DepId && u.Id != userId
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

                return Json(usersWithPictures);
            }
            else
            {
                var usersWithPictures = await (from u in _Context.Users
                                               join ur in _Context.UserRoles on u.Id equals ur.UserId
                                               join r in _Context.Roles on ur.RoleId equals r.Id
                                               where r.Name != "SUPERADMIN" && r.Name == "MANAGER" && u.DepartmentID != DepId
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

                return Json(usersWithPictures);
            }
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
                    Type = ticket.SelectedRadioButton,
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

        public IActionResult Tickets()
        {
            string userId = _userManager.GetUserId(User);
            var tickets = _Context.Tickets
                            .Where(x => x.AssignedToEmployeeId == userId && x.Type == "Team")
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

        public IActionResult OpenTickets()
        {
            string userId = _userManager.GetUserId(User);
            var tickets = _Context.Tickets
                            .Where(x => x.RaisedByEmployeeId == userId && x.Type == "Team")
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
                Notification notification = new Notification();
                notification.UserId = obj.RaisedByEmployeeId;
                notification.Message = "Status of Ticket Changed";
                notification.IsRead = false;
                _Context.Add(notification);
                _Context.SaveChanges();
                return RedirectToAction("Tickets");
            }
            catch (Exception ex)
            {
                return View("Error");
            }
        }
        public IActionResult Tasks()
        {
            string userId = _userManager.GetUserId(User);
            var tasks = _Context.Task
    .Where(x => x.AssignedToEmployeeId == userId)
    .Select(x => new TaskIndex
    {
        TaskID = x.TaskID,
        TaskName = x.TaskName,
        Description = x.Description,
        DueDate = x.DueDate,
        RaisedByEmployeeId = _Context.Users.Where(z => z.Id == x.RaisedByEmployeeId).Select(y => y.FirstName).FirstOrDefault(),
        Status = x.Status,
        Priority = x.Priority,
        AttchmentName = x.AttchmentName
    })
    .ToList();
            return View(tasks);
        }
        public IActionResult TaskDetail(int Id)
        {
            var taskDetail = _Context.Task.Where(x => x.TaskID == Id).FirstOrDefault();
            return View(taskDetail);
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
                Notification notification = new Notification();
                notification.UserId = obj.RaisedByEmployeeId;
                notification.Message = "Task Status Changed";
                notification.IsRead = false;
                _Context.Add(notification);
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

