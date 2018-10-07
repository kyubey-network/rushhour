using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Andoromeda.RushHour.Models
{
    public class Binding
    {
        [MaxLength(16)]
        [ForeignKey("User")]
        public string UserId { get; set; }

        public virtual User User { get; set; }

        [MaxLength(16)]
        public string Account { get; set; }
    }
}
