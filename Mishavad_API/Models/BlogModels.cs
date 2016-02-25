using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization; //for [Data Contract]
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Mishavad_API.Models
{
    public enum PostStatus {
        Draft = 0,
        Published = 1
    }
    public class BlogPostCategory {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class BlogPostTitle
    {
        [Key]
        public string Name { get; set; }
    }

    public class BlogPostAltTitleMap
    {
        [Key]
        [Column(Order = 1)]
        public string BlogPostTitleName { get; set; }
        [Key]
        [Column(Order = 2)]
        public int BlogPostId { get; set; }
        public virtual BlogPostTitle Title { get; set; }
        public virtual BlogPost Post { get; set; }
    }

    [DataContract]
    public class BlogPost
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        [Index(IsUnique = true)]
        [StringLength(300)]//need this for index
        public string Slug { get; set; }

        [DataMember]
        public PostStatus Status { get; set; }

        [DataMember]
        public string Title { get; set; }

        /// <summary>
        /// Used along with Title to make title tag in the head: 
        /// Title – Secondary Title | Website name
        /// </summary>
        [DataMember]
        public string SecondaryTitle { get; set; }

        public int? CreatedById { get; set; }
        [NotMapped]
        [DataMember]
        public string Category {
            get {
                return (BlogPostCategory == null) ? null : BlogPostCategory.Name;
            }
        }

        [DataMember]
        public string Content { get; set; }

        [NotMapped]
        public string Excerpt
        {
            get
            {
                return Content.Substring(0, 500);
            }
        }

        /// <summary>
        /// a string of tags joined by the '|' character
        /// </summary>
        [DataMember]
        public string Tags { get; set; }

        [DataMember]
        public string FootNote { get; set; }

        [DataMember]
        public int? ImageFileServerId { get; set; }
        [DataMember]
        public string ImageFilePath { get; set; }

        /// <summary>
        /// Computes Fullpath of Image
        /// </summary>
        [DataMember]
        [NotMapped]
        [JsonProperty(PropertyName = "image")]
        public string ImageFullPath
        {
            get
            {
                return 
                    Helpers.FileServerTokenManager.GetFullPath (ImageFileServer, ImageFilePath);
            }
        }
        public int? BlogPostCategoryId { get; set; }

        public bool MembersOnly { get; set; }

        [Required, DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreatedDateUtc { get; set; }

        public virtual BlogPostCategory BlogPostCategory { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
        public virtual FileServer ImageFileServer { get; set; }
        public virtual IList<BlogPostAltTitleMap> AlternativeTitles { get; set; }
    }
}