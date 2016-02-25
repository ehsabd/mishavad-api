using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mishavad_API.Models
{
    public class Operation {
        public int Id { get; set; }
        public int CreatedById { get; set; }
        public string Entity { get; set; }// انتیتی که نیاز به تغییر دارد
        public string Property { get; set; } // خصیصه ای که نیاز به تغییر دارد
        public string OldValue { get; set; }// مقدار قبلی
        public string NewValue { get; set; }// مقدار جدید
        public DateTime CreatedDateUtc { get; set; }

        public virtual ApplicationUser CreatedBy { get; set; }
    }

    public enum AuthStatus {
            Pending = 0,
            Rejected = 1,
            Accepted = 2
    }

    public class OperationAuth {
        public int Id { get; set; }
        public int OperationId { get; set; }
        
        public int AuthUserId { get; set; }
        /// <summary>
        /// Reasons for acceptance or rejection
        /// </summary>
        public string ReasonsForAction { get; set; }
        public AuthStatus Status { get; set; }
        public DateTime CreatedDateUtc { get; set; }
        public bool IsDone { get; set; }

        public virtual Operation Operation { get; set; }
        public virtual ApplicationUser AuthUser { get; set; }
     }
}