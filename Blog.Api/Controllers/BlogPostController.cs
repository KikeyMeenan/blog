using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blog.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogPostController : ControllerBase
    {
        private readonly IGenericRepository<BlogPost> _repo;

        public BlogPostController(IGenericRepository<BlogPost> repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BlogPost>>> GetBlogPosts()
        {
            return Ok(await _repo.GetAll().ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BlogPost>> GetBlogPost(Guid id)
        {
            var post = await _repo.GetById(id);

            if (post == null)
            {
                return NotFound();
            }

            return post;
        }

        // PUT: api/Post/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBlogPost(Guid id, BlogPost post)
        {
            if (id != post.Id)
            {
                return BadRequest();
            }

            try
            {
                await _repo.Update(id, post);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await BlogPostExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Post
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<ActionResult<BlogPost>> PostBlogPost(BlogPost post)
        {
            await _repo.Create(post);

            return CreatedAtAction("GetBlogPost", new { id = post.Id }, post);
        }

        // DELETE: api/Post/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<BlogPost>> DeleteBlogPost(Guid id)
        {
            try
            {
                await _repo.Delete(id);
            }
            catch (NullReferenceException notFoudException)
            {

                return NotFound();
            }
            
            return Ok();
        }

        private async Task<bool> BlogPostExists(Guid id)
        {
            return await _repo.GetById(id) != null;
        }
    }
}
