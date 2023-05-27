using BuDiTest.Areas.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BuDiTest.Services
{
    public class GetData
    {
        private readonly ApplicationDbContext _context;
        private IWebHostEnvironment _env;

        public GetData(
            ApplicationDbContext context,
            IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        public ApplicationUser GetUserByEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentException("Email cannot be null or empty.");
            }
            var user = _context.Users
                .Where(u => u.Email == email)
                .FirstOrDefault();
            return user;
        }
        public ImageModel GetImageById(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Name cannot be null or empty.");
            }
            var image = _context.Users
                .Where(u => u.Email == name)
                .Select(u => new ImageModel
                {
                    ProfileImage = u.ImgUrl ?? "images.png"
                })
                .FirstOrDefault();
            return image;
        }
    }
}
