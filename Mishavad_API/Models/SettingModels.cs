using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Mishavad_API.Models
{
    public class Setting
    {
        [Key]
        public string Name { get; set; }
        public string Value { get; set; }
        [NotMapped]
        public int IntValue
        {
            get
            {
                return int.Parse(Value);
            }
        }
    }
    public class PublicSetting:Setting
    {
        
    }

    public class PrivateSetting:Setting
    {
    }
        
}