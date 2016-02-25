using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using System.Runtime.Serialization;

namespace Mishavad_API.Models
{
   
        public class Visit
        {
            public long Id { get; set; }
            public DateTime LastVisit { get; set; } //e.g. 2015-03-14 23:40:26
            public DateTime LastCounter { get; set; } // 2015-03-14
            public int Count { get; set; }
        }
        public class Visitor
        {
            public DateTime LastCounter { get; set; } // 2015-03-14
            //referred
            //agent
            //platform
            //version
            //UAstring ??
            //IP
            //location e.g. IR
            //hits
            //honeypot


        }
    [DataContract]
        public class VisitReferrer
        {
            public int Id { get; set; }
            //NOTE: The amount of IP data become huge after a while. This really saves space
            [Required, MinLength(4), MaxLength(16)]
            public byte[] IP { get; set; }

            [NotMapped]

            public IPAddress IPAddress
            {
                get { return new IPAddress(IP); }
                set { IP = value.GetAddressBytes(); }
            }
            [DataMember]
            public string IPAddressString
            {
                get { return IPAddress.ToString(); }
            } 

            [DataMember]
            public string Referrer { get; set; }

            [DataMember]
            public string Url { get; set; }
        }
    
}