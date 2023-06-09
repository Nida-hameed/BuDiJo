﻿#nullable disable
using BuDiTest.Areas.Identity.Data;
using BuDiTest.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BuDiTest.Areas.Identity.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);
    }

    public DbSet<Employee> Employees { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<EmployeeTicket> EmployeeTickets { get; set; }
    public DbSet<EmployeeTask> EmployeeTasks { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<ClockEvent> ClockEvents { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<MyTask> Task { get; set; }
}
