using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Andoromeda.RushHour.Models
{
    public enum AlertStatus
    {
        Pending,
        Delivered,
        Failed
    }

    public enum AlertType
    {
        EmailOnly,
        Sms,
        Voice
    }

    public class Alert
    {
        public Guid Id { get; set; }

        [MaxLength(16)]
        [ForeignKey("User")]
        public string UserId { get; set; }

        public virtual User User { get; set; }

        [MaxLength(16)]
        public string Account { get; set; }

        [MaxLength(64)]
        public string TransactionId { get; set; }

        public DateTime CreatedTime { get; set; } = DateTime.UtcNow;

        public DateTime? DeliveredTime { get; set; }

        public AlertStatus Status { get; set; }

        public AlertType Type { get; set; }
    }
}
