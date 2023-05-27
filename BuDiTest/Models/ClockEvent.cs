using System.ComponentModel.DataAnnotations;

namespace BuDiTest.Models
{
    public class ClockEvent
    {
        [Key]
        public int ClockEventID { get; set; }
        public DateTime ClockInTime { get; set; }
        public DateTime? ClockOutTime { get; set; }
        public ClockEventType Type { get; set; }

        // One-to-Many: ClockEvent to Employee (ClockIn)
        public string UserId { get; set; }
    }
    public enum ClockEventType
    {
        ClockIn,
        ClockOut
    }

}
