#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace BuDiTest.Areas.Identity.Data;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{

    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime CreateDateTime { get; set; }
    public string? CreatedBy { get; set; }
    public int? DepartmentID { get; set; }
    public string? ImgUrl { get; set; }
    [NotMapped]
    public string FullName => $"{FirstName} {LastName} ({ImgUrl})";
}

