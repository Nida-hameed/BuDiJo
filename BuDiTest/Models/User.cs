using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace BuDiTest.Models
{
    public class User
    {
        [Display(Name = "User First Name")]
        [Required]
        public string FirstName { get; set; }
        [Display(Name = "User Last Name")]
        [Required]
        public string LastName { get; set; }
        [Display(Name = "User Email")]
        [Required]
        public string Email { get; set; }

        [Display(Name = "User Password")]
        [Required]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[!@#$%^&*()_+\-=[\]{};':""\\|,.<>/?]).{8,}$", ErrorMessage = "The password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character. It should be at least 8 characters long.")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
        public string ReturnUrl { get; set; }
        [Display(Name = "User Email ")]
        [Required]
        public string UserId { get; set; }

        [Display(Name = "Select User Role ")]
        [Required]
        public string UserRole { get; set; }


        [Display(Name = "User Mobile Number")]
        [Required]
        public string MobileNumber { get; set; }

        [Display(Name = "Department ID")]
        [Required]
        public int DepartmentID { get; set; }
        [Display(Name = "User Image")]
        [Required]
        public string ImgUrl { get; set; }
        [NotMapped]
        public IFormFile Picture { get; set; }
    }
}
