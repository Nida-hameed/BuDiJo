using Microsoft.Build.Framework;

namespace BuDiTest.Models
{
    public class AssignTicket
    {
        public string Title { get; set; }

        public string? Description { get; set; }

        public DateTime CreatedDate { get; set; }

        public string RaisedByEmployeeId { get; set; }
        [Required]
        public string UserId { get; set; }

        public string Status { get; set; }
        public string SelectedRadioButton { get; set; }
        public string AttchmentName { get; set; }
        public IFormFile Attachments { get; set; }
    }
}
