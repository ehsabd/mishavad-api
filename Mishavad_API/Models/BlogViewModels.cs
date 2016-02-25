using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mishavad_API.Models
{
    public class CompleteBlogPostVM {
        public int Id { get; set; }
        public string Slug { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Category { get; set; }
        public string Content { get; set; }
        public string Tags { get; set; }
        public string FootNote { get; set; }
        public string Image { get; set; }
        public int? CategoryId { get; set; }
        public DateTime DateUtc { get; set; }
    }

    public class BlogPostBindingModel {
        public string Slug { get; set; }
        public string Title { get; set; }
        public int? CreatedById { get; set; }
        public string Content { get; set; }
        public string Tags { get; set; }
        public string FootNote { get; set; }
        public int? CategoryId { get; set; }
        public int? ImageFileServerId { get; set; }
        public string ImageFilePath { get; set; }
    }
}