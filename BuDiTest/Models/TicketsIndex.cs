using BuDiTest.Models;

namespace BuDiTest.Models
{
    public class TicketsIndex
    {
        public int TicketID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public string RaisedByEmployeeId { get; set; }
        public string AssignedTo { get; set; }
        public TicketStatus Status { get; set; }
        public string AttchmentName { get; set; }

    }
}
