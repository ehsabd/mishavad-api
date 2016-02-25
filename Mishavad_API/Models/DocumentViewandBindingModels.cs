using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.ComponentModel.DataAnnotations;
namespace Mishavad_API
{
    public class Document_AddBM
    {
        [Required]
        public string Description { get; set; }
        public string Base64Image { get; set; }
    }

   
}