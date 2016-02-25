using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Mishavad_API.Models
{
    public class ContactBM
    {
        [Required]
        public string Message { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
    }
}