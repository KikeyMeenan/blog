using Blog.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Blog.Api.Tests.Unit
{
    public class BlogPostControllerTests
    {
        private readonly List<BlogPost> blogPosts;

        public BlogPostControllerTests()
        {
            blogPosts = new List<BlogPost>()
            {
                new BlogPost { Id = new Guid() },
                new BlogPost { Id = new Guid() }
            };
        }

        [Test]
        public async Task Given_DatabaseHasBlogPosts_When_Get_Then_ReturnAllBlogPosts()
        {
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: "GetAll")
                .Options;

            using (var context = new Context(options))
            {
                context.BlogPosts.AddRange(blogPosts);
                context.SaveChanges();
            }

            using (var context = new Context(options))
            {
                var service = new BlogPostController(new GenericRepository<BlogPost>(context));
                var result = await service.GetBlogPosts();
                Assert.AreEqual(blogPosts.Count, ((result.Result as OkObjectResult).Value as List<BlogPost>).Count);
            }
        }

        [Test]
        public async Task Given_DatabaseHasBlogPostWithId_When_Get_Then_ReturnBlogPost()
        {
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: "GetSuccess")
                .Options;

            using (var context = new Context(options))
            {
                context.BlogPosts.AddRange(blogPosts);
                context.SaveChanges();
            }

            using (var context = new Context(options))
            {
                var service = new BlogPostController(new GenericRepository<BlogPost>(context));
                var result = await service.GetBlogPost(blogPosts[0].Id);
                Assert.AreEqual(blogPosts[0].Id, (result.Value as BlogPost).Id);
            }
        }

        [Test]
        public async Task Given_DatabaseHasBlogPostWithoutId_When_Get_Then_Return404()
        {
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: "GetFail")
                .Options;

            using (var context = new Context(options))
            {
                context.BlogPosts.AddRange(blogPosts);
                context.SaveChanges();
            }

            using (var context = new Context(options))
            {
                var service = new BlogPostController(new GenericRepository<BlogPost>(context));
                var result = await service.GetBlogPost(new Guid());
                Assert.AreEqual(404, ((result as ActionResult<BlogPost>).Result as StatusCodeResult).StatusCode);
            }
        }

        [Test]
        public async Task Given_ValidBlogPostProvided_When_Post_Then_CreateBlogPost()
        {
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: "CreateBlogPost")
                .Options;

            var newBlogPost = new BlogPost()
            {
                Title = "new blogPost",
                Content = "some content"
            };

            using (var context = new Context(options))
            {
                var service = new BlogPostController(new GenericRepository<BlogPost>(context));
                var result = await service.PostBlogPost(newBlogPost);

                Assert.IsNotNull(context.BlogPosts.Find(newBlogPost.Id));

                var actionResult = (result as ActionResult<BlogPost>).Result as CreatedAtActionResult;

                Assert.AreEqual(201, actionResult.StatusCode);
                Assert.AreEqual("GetBlogPost", actionResult.ActionName);
                Assert.AreEqual(newBlogPost.Id, actionResult.RouteValues.GetValueOrDefault("id"));
                Assert.AreEqual(newBlogPost, actionResult.Value);
            }
        }

        //NEED TO TEST CONCURRENCY EXCEPTIONS!

        [Test]
        public async Task Given_IdDoesNotMatch_When_Put_Then_Return400()
        {
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: "PutFail")
                .Options;

            using (var context = new Context(options))
            {
                context.BlogPosts.AddRange(blogPosts);
                context.SaveChanges();
            }

            using (var context = new Context(options))
            {
                var service = new BlogPostController(new GenericRepository<BlogPost>(context));
                var result = await service.PutBlogPost(new Guid(), blogPosts[0]);
                Assert.AreEqual(400, (result as BadRequestResult).StatusCode);
            }
        }

        [Test]
        public async Task Given_ValidBlogPostProvided_When_Put_Then_UpdateBlogPost()
        {
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: "PutSuccess")
                .Options;

            using (var context = new Context(options))
            {
                context.BlogPosts.AddRange(blogPosts);
                context.SaveChanges();
            }

            using (var context = new Context(options))
            {
                var updatedBlogPost = new BlogPost()
                {
                    Id = blogPosts[0].Id,
                    Title = blogPosts[0].Title,
                    Content = "updated content",
                };

                var service = new BlogPostController(new GenericRepository<BlogPost>(context));
                var result = await service.PutBlogPost(updatedBlogPost.Id, updatedBlogPost);

                Assert.AreEqual(204, (result as StatusCodeResult).StatusCode);
                Assert.AreEqual("updated content", context.BlogPosts.Find(blogPosts[0].Id).Content);
            }
        }

        [Test]
        public async Task Given_IdDoesNotExist_When_Delete_Then_Return404()
        {
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: "DeleteFail")
                .Options;

            using (var context = new Context(options))
            {
                context.BlogPosts.AddRange(blogPosts);
                context.SaveChanges();
            }

            using (var context = new Context(options))
            {
                var service = new BlogPostController(new GenericRepository<BlogPost>(context));
                var result = await service.DeleteBlogPost(new Guid());
                Assert.AreEqual(404, ((result as ActionResult<BlogPost>).Result as StatusCodeResult).StatusCode);
            }
        }

        [Test]
        public async Task Given_IdDoesExist_When_Delete_Then_DeleteAndReturnBlogPost()
        {
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: "DeleteSuccess")
                .Options;

            using (var context = new Context(options))
            {
                context.BlogPosts.AddRange(blogPosts);
                context.SaveChanges();
            }

            using (var context = new Context(options))
            {
                var service = new BlogPostController(new GenericRepository<BlogPost>(context));
                var result = await service.DeleteBlogPost(blogPosts[0].Id);
                Assert.AreEqual(1, await context.BlogPosts.CountAsync());
            }
        }
    }
}