using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization; //for [Data Contract]
namespace Mishavad_API.Models
{
    [DataContract]
    public class Document:ISoftDelete
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Description { get; set; }

        [Required]
        public string FilePath { get; set; }

        public int FileServerId { get; set; }    
       
        public virtual FileServer FileServer { get; set; }

        public DateTime? RemovedFlagUtc { get; set; }

        [BF_Idx(ContextGeneratedOption.None)]
        public int BF_Idx { get; set; }
    }

    public class UserDocumentMap
    {
        [Key, ForeignKey("Document")]
        public int DocumentId { get; set; }
        public int UserId {get;set;}
        public virtual ApplicationUser User { get; set; }
        public virtual Document Document { get; set; }
    }

    public class CampaignDocumentMap
    {
        [Key, ForeignKey("Document")]
        public int DocumentId { get; set; }
        public int CampaignId { get; set; }
        public virtual Campaign Campaign { get; set; }
        public virtual Document Document { get; set; }
    }

}