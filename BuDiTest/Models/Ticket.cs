using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace BuDiTest.Models
{
    public class Ticket
    {
        [Key]
        public int TicketID { get; set; }

        [MaxLength(100)]
        public string Title { get; set; }       

        [MaxLength(200)]
        public string? Description { get; set; }

        [Display(Name = "Creation Date")]
        [DataType(DataType.Date)]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Raised By")]
        public string RaisedByEmployeeId { get; set; }

        [Display(Name = "Assigned To")]
        public string AssignedToEmployeeId { get; set; }
        public TicketStatus Status { get; set; }
        public string Type { get; set; }
        public string AttchmentName { get; set; }
        [NotMapped]
        [DataType(DataType.Upload)]
        public IFormFile Attachments { get; set; }
   
    }
    public enum TicketStatus
    {
        NotStarted,
        InProgress,
        Completed
    }
}

