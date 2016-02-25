using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.ComponentModel.DataAnnotations.Schema;

namespace Mishavad_API.Models
{
    public enum FileServerTokenType { 
       AvatarImageUpload = 1,
       CampaignImageUpload = 2,
       DocumentUpload = 3,
       RewardImageUpload = 4,
       DocumentDownload = 103
    }

    /*NOTE: We do not use another table to store Resources because tokens are generally short-time and can be removed readily*/
    public class FileServerToken
    {
        public long Id { get; set; }
        public string AccountNumber { get; set; }
        public FileServerTokenType FileTokenType { get; set; }
        public DateTime TokenExpDateUtc { get; set; }
        public string TokenHash { get; set; }
        public string Resource { get; set; }
        public int? EntryId { get; set; }
    
    }

    public class FileServer{
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id{get;set;}
        public string ServerIP {get;set;}
        public string ServerUri { get; set; }
    }
}