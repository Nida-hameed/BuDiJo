using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BuDiTest.Areas.Identity.Data;
using BuDiTest.Models;
using Microsoft.AspNetCore.Identity;

namespace BuDiTest.Controllers
{
    public class ClockEventsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ClockEventsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public IActionResult Clock()
        {
        return View();
        }
        [HttpGet]
        public async Task<IActionResult> LatestClockTime()
        {
            string UserId = _userManager.GetUserId(User);
            var latestClockIn = await GetData(UserId);

            if (latestClockIn == null)
            {
                return new JsonResult(new
                {
                    data = ""
                });
            }
            else if (!string.IsNullOrEmpty(latestClockIn.ClockInTime.ToString()) && !string.IsNullOrEmpty(latestClockIn.ClockOutTime.ToString()))
            {
                return new JsonResult(new
                {
                    data = latestClockIn,
                    message = "exist"
                });
            }

            return new JsonResult(new
            {
                data = latestClockIn
            });
        }

        // GET: ClockEvents
        [HttpPost]
        public async Task<IActionResult> ClockIn()
        {
            string UserId = _userManager.GetUserId(User);
            var latestClockIn = await GetData(UserId);
            var currentDate = System.DateTime.Now.Date;
            if (latestClockIn != null)
            {
                if (!string.IsNullOrEmpty(latestClockIn.ClockInTime.ToString()) && !string.IsNullOrEmpty(latestClockIn.ClockOutTime.ToString()))
                {
                    if (latestClockIn.ClockInTime.Date == currentDate)
                    {
                        return new JsonResult(new
                        {
                            success = false,

                        });
                    }
                }
            }
            var clockEvent = new ClockEvent
            {
                UserId = UserId,
                ClockInTime = DateTime.Now,
                Type = ClockEventType.ClockIn
            };
            await _context.ClockEvents.AddAsync(clockEvent);
            await _context.SaveChangesAsync();

            return new JsonResult(new
            {
                success = true,
                message = "You are successfully clocked in!"
            });
        }

        [HttpPost]
        public async Task<IActionResult> ClockOut()
        {
            string UserId = _userManager.GetUserId(User);
            var latestClockIn = await GetData(UserId);

            if (latestClockIn != null)
            {
                if (!string.IsNullOrEmpty(latestClockIn.ClockInTime.ToString()) && !string.IsNullOrEmpty(latestClockIn.ClockOutTime.ToString()))
                {
                    return new JsonResult(new
                    {
                        success = false
                    });
                }
            }

            var clockEvent = await _context.ClockEvents
                    .Where(ce => ce.UserId == UserId && ce.ClockOutTime == null)
                    .OrderByDescending(ce => ce.ClockInTime)
                    .FirstOrDefaultAsync();

            if (clockEvent == null)
            {
                return NotFound("No open clock event found for this employee.");
            }

            clockEvent.ClockOutTime = DateTime.Now;
            _context.Update(clockEvent);
            await _context.SaveChangesAsync();

            return new JsonResult(new
            {
                success = true,
                message = "You are successfully clocked out!"
            });
        }

        private async Task<ClockEvent> GetData(string UserId)
        {
            var latestClockIn = await _context.ClockEvents
                .Where(ce => ce.UserId == UserId)
                .OrderBy(ce => ce.ClockInTime)
                .OrderBy(ce => ce.ClockOutTime)
                .FirstOrDefaultAsync();
            return latestClockIn;
        }
    }
}
