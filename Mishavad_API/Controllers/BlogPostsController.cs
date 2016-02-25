using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Mishavad_API.Models;

using System.Web.Http.OData;
using System.Web.Http.OData.Query;
using System.Web.Http.OData.Extensions;

namespace Mishavad_API.Controllers
{
    [RoutePrefix("api/BlogPosts")]
    public class BlogPostsController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();


        // GET: api/BlogPosts
        [Route("")]
        [ResponseType(typeof(PageResult<object>))]
        public IHttpActionResult GetBlogPosts(ODataQueryOptions<BlogPost> options)
        {
            IQueryable<object> results;
            ODataQuerySettings settings = new ODataQuerySettings();
            settings.PageSize = 5;
            settings.EnsureStableOrdering = false;
            //let EF query the table without messing with Excerpt
            var posts = db.BlogPosts.Where(p => p.Status == PostStatus.Published).OrderByDescending(p => p.Id).Include(p => p.ImageFileServer);
            posts = options.ApplyTo(posts, settings) as IQueryable<BlogPost>;
            posts.Load();

            results = posts.AsQueryable().Select(p=>new {
                    CategoryId = p.BlogPostCategoryId,
                    Excerpt = p.Excerpt,
                    DateUtc = p.CreatedDateUtc,
                    Category = p.Category,
                    Slug = p.Slug,
                    Image = p.ImageFullPath,
                    Tags = p.Tags,
                    Title = p.Title
                });

            
            return Ok(new PageResult<object>(
               results as IEnumerable<object>,
               Request.ODataProperties().NextLink,
               Request.ODataProperties().TotalCount));
        }

        [Route("")]
        public IQueryable<object> GetBlogPosts(string tag)
        {
            return db.BlogPosts.Where(p => p.Status == PostStatus.Published && p.Tags.Contains(tag))
                .OrderByDescending(p => p.Id)
                .Include(p => p.ImageFileServer);
        }

        [Route("")]
        [ResponseType(typeof(IQueryable<BlogPost>))]
        public IHttpActionResult GetBlogPosts(bool newest)
        {
            if (newest)
            {
                var startDateUtc = DateTime.UtcNow.Date.AddDays(
                   -1 * int.Parse(System.Configuration.ConfigurationManager.AppSettings["blog_newposts_days"]
                    ));
                var newPosts = db.BlogPosts.Where(p => p.Status == PostStatus.Published && p.CreatedDateUtc > startDateUtc)
                    .OrderByDescending(p=>p.CreatedDateUtc)
                    .Select(p => new { p.Title, p.Slug });

                return Ok(newPosts);
               
            }
            else {
                return BadRequest();
            }
        }

        // GET: api/BlogPosts/5
        [ResponseType(typeof(BlogPost))]
        [Route("{id_or_slug?}")]
        public async Task<IHttpActionResult> GetBlogPost(string id_or_slug)
        {
            BlogPost blogPost;
            int id;
            if (int.TryParse(id_or_slug, out id))
            {
                blogPost =await db.BlogPosts.FindAsync(id);
            }
            else {
                var query = db.BlogPosts.Where(p => p.Slug == id_or_slug).Select(c => c);
                blogPost = (await query.CountAsync() == 1) ? await query.FirstAsync() : null;
            }

            if (blogPost == null)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    ReasonPhrase = "Not Found - Wrong Id or Slug"
                });
            }
                       
            return Ok(blogPost);
        }
        

        // POST [Update]: api/BlogPosts/5
        [ResponseType(typeof(void))]
        [Authorize]
        [HttpPost]
        [Route("{id}", Name = "UpdateBlogPost")]
        public async Task<IHttpActionResult> UpdateBlogPost(int id, BlogPost blogPost)
        {
            //TODO:The user should have author rights and matches with the author
           
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != blogPost.Id)
            {
                return BadRequest();
            }

            db.Entry(blogPost).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BlogPostExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }
        

        // POST: api/BlogPosts
        [ResponseType(typeof(BlogPost))]
        [Route("", Name = "NewBlogPost")]
        [HttpPost]
        public async Task<IHttpActionResult> NewBlogPost([FromBody] BlogPostBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (model.Content == null) {
                return BadRequest("No Content");
            }
            if ( model.Content.Length < 500) {
                return BadRequest("error: Check api controller code please!");
            }

            var blogPost = new BlogPost
            {
                BlogPostCategoryId = model.CategoryId,
                Content = model.Content,
                CreatedById = model.CreatedById,
                FootNote = model.FootNote,
                ImageFilePath = model.ImageFilePath,
                ImageFileServerId = model.ImageFileServerId,
                Slug = Helpers.SlugHelper.GenerateSlug(model.Title),
                Tags = model.Tags,
                Title = model.Title
            };
            db.BlogPosts.Add(blogPost);
            await db.SaveChangesAsync();
            //Since we are using attribute routing. we have to name our route or get rid of it at all!
            // see http://stackoverflow.com/questions/29636095/web-api-2-post-urlhelper-link-must-not-return-null
            //return CreatedAtRoute("DefaultApi", new { id = blogPost.Id }, blogPost);
            return Created<int>("DefaultApi", blogPost.Id);

        }


        // GET: api/BlogPosts/5/html
      
        [Route("{id_or_slug?}/html")]
        public async Task<HttpResponseMessage> GetBlogPostHtml(string id_or_slug)
        {
            BlogPost blogPost;
            int id;
            if (int.TryParse(id_or_slug, out id))
            {
                blogPost = await db.BlogPosts.FindAsync(id);
            }
            else {
                var query = db.BlogPosts.Where(p => p.Slug == id_or_slug).Select(c => c);
                blogPost = (await query.CountAsync() == 1) ? await query.FirstAsync() : null;
            }

            if (blogPost == null)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    ReasonPhrase = "Not Found - Wrong Id or Slug"
                });
            }

            var html = string.Format(@"
                <!DOCTYPE html>
                <html>
                    <head>
                        <title>{0}</title>
                        <meta charset=""UTF-8"">
                        <meta name=""description"" content=""{1}"">
                        <meta property=""og:site_name"" content=""Pandool"">
                        <meta name=""telegram:channel"" content=""@pandool"">
                        <meta property=""og:image"" content=""{2}"">
                    </head>
                    <body>
                        <article>
                        <figure data-block=""Photo"">
                            <img src=""{2}"" alt=""{0}"">
                        </figure>
                        {3}
                        </article>
                    </body>
                </html>

                ", blogPost.Title,blogPost.Excerpt,blogPost.ImageFullPath, blogPost.Content);

            var response = new HttpResponseMessage();
            response.Content = new StringContent(html);
            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/html");
            return response;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool BlogPostExists(int id)
        {
            return db.BlogPosts.Count(e => e.Id == id) > 0;
        }
    }
}