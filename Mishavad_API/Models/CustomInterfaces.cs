using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mishavad_API.Models
{
        interface ISoftDelete
        {
            DateTime? RemovedFlagUtc { get; set; }
        }
    
}