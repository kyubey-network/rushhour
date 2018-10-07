using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Andoromeda.RushHour.Models
{
    public enum UserPlan
    {
        Free,
        Pro,
        Premium
    }

    public class User
    {
        /// <summary>
        /// Phone number
        /// </summary>
        [MaxLength(16)]
        public string Id { get; set; }

        [MaxLength(64)]
        public string Email { get; set; }

        public bool IsEmailValidated { get; set; }

        public double SmsAlertBar { get; set; }

        public double VoiceAlertBar { get; set; }

        public UserPlan Plan { get; set; }
    }
}
