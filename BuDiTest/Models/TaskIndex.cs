using BuDiTest.Models;

namespace BuDiTest.Models
{
    public class TaskIndex
    {
        public int TaskID { get; set; }

        public string TaskName { get; set; }

        public string Description { get; set; }

        public DateTime DueDate { get; set; }

        public string RaisedByEmployeeId { get; set; }

        public string AssignedToEmployeeId { get; set; }

        public string AttchmentName { get; set; }

        public Task_Status Status { get; set; }

        public Task_Priority Priority { get; set; }
    }
}
