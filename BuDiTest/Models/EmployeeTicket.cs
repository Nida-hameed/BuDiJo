using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace BuDiTest.Models
{
    public class EmployeeTicket
    {
        [Key]
        public int EmployeeTicketID { get; set; }

        [Display(Name = "Raised By")]
        public int RaisedByEmployeeID { get; set; }
        [NotMapped]
        public Employee? RaisedByEmployee { get; set; }

        [Display(Name = "Assigned To")]
        public int EmployeeID { get; set; }
        public Employee? Employee { get; set; }

        public int TicketID { get; set; }
        public Ticket? Ticket { get; set; }
    }
}
