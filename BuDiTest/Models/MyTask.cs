using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace BuDiTest.Models
{
    public class MyTask
    {
        [Key]
        public int TaskID { get; set; }

        [MaxLength(100)]
        [Display(Name = "Task Name")]
        public string TaskName { get; set; }

        [MaxLength(200)]
        public string Description { get; set; }

        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }

        [Display(Name = "Raised By")]
        public string RaisedByEmployeeId { get; set; }

        [Display(Name = "Assigned To")]
        public string AssignedToEmployeeId { get; set; }

        public string AttchmentName { get; set; }
        [NotMapped]
        [DataType(DataType.Upload)]
        public IFormFile Attachments { get; set; }

        [Display(Name = "Task Status")]
        public Task_Status Status { get; set; }

        [Display(Name = "Task Priority")]
        public Task_Priority Priority { get; set; }


    }
    public enum Task_Priority
    {
        High,
        Medium,
        Low
    }

    public enum Task_Status
    {
        NotStarted,
        InProgress,
        Completed
    }
}

