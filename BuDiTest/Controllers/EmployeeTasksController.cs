using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BuDiTest.Areas.Identity.Data;
using BuDiTest.Models;

namespace BuDiTest.Controllers
{
    public class EmployeeTasksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmployeeTasksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: EmployeeTasks
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.EmployeeTasks.Include(e => e.Employee).Include(e => e.Task);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: EmployeeTasks/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.EmployeeTasks == null)
            {
                return NotFound();
            }

            var employeeTask = await _context.EmployeeTasks
                .Include(e => e.Employee)
                .Include(e => e.Task)
                .FirstOrDefaultAsync(m => m.EmployeeTaskID == id);
            if (employeeTask == null)
            {
                return NotFound();
            }

            return View(employeeTask);
        }

        // GET: EmployeeTasks/Create
        public IActionResult Create()
        {
            ViewData["EmployeeID"] = new SelectList(_context.Employees, "EmployeeID", "Email");
            ViewData["TaskID"] = new SelectList(_context.Task, "TaskID", "TaskID");
            return View();
        }

        // POST: EmployeeTasks/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EmployeeTaskID,RaisedByEmployeeID,EmployeeID,TaskID")] EmployeeTask employeeTask)
        {
            if (ModelState.IsValid)
            {
                _context.Add(employeeTask);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["EmployeeID"] = new SelectList(_context.Employees, "EmployeeID", "Email", employeeTask.EmployeeID);
            ViewData["TaskID"] = new SelectList(_context.Task, "TaskID", "TaskID", employeeTask.TaskID);
            return View(employeeTask);
        }

        // GET: EmployeeTasks/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.EmployeeTasks == null)
            {
                return NotFound();
            }

            var employeeTask = await _context.EmployeeTasks.FindAsync(id);
            if (employeeTask == null)
            {
                return NotFound();
            }
            ViewData["EmployeeID"] = new SelectList(_context.Employees, "EmployeeID", "Email", employeeTask.EmployeeID);
            ViewData["TaskID"] = new SelectList(_context.Task, "TaskID", "TaskID", employeeTask.TaskID);
            return View(employeeTask);
        }

        // POST: EmployeeTasks/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EmployeeTaskID,RaisedByEmployeeID,EmployeeID,TaskID")] EmployeeTask employeeTask)
        {
            if (id != employeeTask.EmployeeTaskID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(employeeTask);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeTaskExists(employeeTask.EmployeeTaskID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["EmployeeID"] = new SelectList(_context.Employees, "EmployeeID", "Email", employeeTask.EmployeeID);
            ViewData["TaskID"] = new SelectList(_context.Task, "TaskID", "TaskID", employeeTask.TaskID);
            return View(employeeTask);
        }

        // GET: EmployeeTasks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.EmployeeTasks == null)
            {
                return NotFound();
            }

            var employeeTask = await _context.EmployeeTasks
                .Include(e => e.Employee)
                .Include(e => e.Task)
                .FirstOrDefaultAsync(m => m.EmployeeTaskID == id);
            if (employeeTask == null)
            {
                return NotFound();
            }

            return View(employeeTask);
        }

        // POST: EmployeeTasks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.EmployeeTasks == null)
            {
                return Problem("Entity set 'ApplicationDbContext.EmployeeTasks'  is null.");
            }
            var employeeTask = await _context.EmployeeTasks.FindAsync(id);
            if (employeeTask != null)
            {
                _context.EmployeeTasks.Remove(employeeTask);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeTaskExists(int id)
        {
          return (_context.EmployeeTasks?.Any(e => e.EmployeeTaskID == id)).GetValueOrDefault();
        }
    }
}
