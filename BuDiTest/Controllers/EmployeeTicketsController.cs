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
    public class EmployeeTicketsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmployeeTicketsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: EmployeeTickets
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.EmployeeTickets.Include(e => e.Employee).Include(e => e.Ticket);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: EmployeeTickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.EmployeeTickets == null)
            {
                return NotFound();
            }

            var employeeTicket = await _context.EmployeeTickets
                .Include(e => e.Employee)
                .Include(e => e.Ticket)
                .FirstOrDefaultAsync(m => m.EmployeeTicketID == id);
            if (employeeTicket == null)
            {
                return NotFound();
            }

            return View(employeeTicket);
        }

        // GET: EmployeeTickets/Create
        public IActionResult Create()
        {
            ViewData["EmployeeID"] = new SelectList(_context.Employees, "EmployeeID", "Email");
            ViewData["TicketID"] = new SelectList(_context.Tickets, "TicketID", "TicketID");
            return View();
        }

        // POST: EmployeeTickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EmployeeTicketID,RaisedByEmployeeID,EmployeeID,TicketID")] EmployeeTicket employeeTicket)
        {
            if (ModelState.IsValid)
            {
                _context.Add(employeeTicket);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["EmployeeID"] = new SelectList(_context.Employees, "EmployeeID", "Email", employeeTicket.EmployeeID);
            ViewData["TicketID"] = new SelectList(_context.Tickets, "TicketID", "TicketID", employeeTicket.TicketID);
            return View(employeeTicket);
        }

        // GET: EmployeeTickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.EmployeeTickets == null)
            {
                return NotFound();
            }

            var employeeTicket = await _context.EmployeeTickets.FindAsync(id);
            if (employeeTicket == null)
            {
                return NotFound();
            }
            ViewData["EmployeeID"] = new SelectList(_context.Employees, "EmployeeID", "Email", employeeTicket.EmployeeID);
            ViewData["TicketID"] = new SelectList(_context.Tickets, "TicketID", "TicketID", employeeTicket.TicketID);
            return View(employeeTicket);
        }

        // POST: EmployeeTickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EmployeeTicketID,RaisedByEmployeeID,EmployeeID,TicketID")] EmployeeTicket employeeTicket)
        {
            if (id != employeeTicket.EmployeeTicketID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(employeeTicket);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeTicketExists(employeeTicket.EmployeeTicketID))
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
            ViewData["EmployeeID"] = new SelectList(_context.Employees, "EmployeeID", "Email", employeeTicket.EmployeeID);
            ViewData["TicketID"] = new SelectList(_context.Tickets, "TicketID", "TicketID", employeeTicket.TicketID);
            return View(employeeTicket);
        }

        // GET: EmployeeTickets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.EmployeeTickets == null)
            {
                return NotFound();
            }

            var employeeTicket = await _context.EmployeeTickets
                .Include(e => e.Employee)
                .Include(e => e.Ticket)
                .FirstOrDefaultAsync(m => m.EmployeeTicketID == id);
            if (employeeTicket == null)
            {
                return NotFound();
            }

            return View(employeeTicket);
        }

        // POST: EmployeeTickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.EmployeeTickets == null)
            {
                return Problem("Entity set 'ApplicationDbContext.EmployeeTickets'  is null.");
            }
            var employeeTicket = await _context.EmployeeTickets.FindAsync(id);
            if (employeeTicket != null)
            {
                _context.EmployeeTickets.Remove(employeeTicket);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeTicketExists(int id)
        {
          return (_context.EmployeeTickets?.Any(e => e.EmployeeTicketID == id)).GetValueOrDefault();
        }
    }
}
