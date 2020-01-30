using Blog.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Blog.Api.Tests.Unit
{
    public class CategoryControllerTests
    {
        private readonly List<Category> categories;

        public CategoryControllerTests()
        {
            categories = new List<Category>()
            {
                new Category { Id = new Guid() },
                new Category { Id = new Guid() }
            };
        }

        [Test]
        public async Task Given_DatabaseHasCategories_When_Get_Then_ReturnAllCategories()
        {
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: "GetAll")
                .Options;

            using (var context = new Context(options))
            {
                context.Categories.AddRange(categories);
                context.SaveChanges();
            }

            using (var context = new Context(options))
            {
                var service = new CategoryController(new GenericRepository<Category>(context));
                var result = await service.GetCategories();
                Assert.AreEqual(categories.Count, ((result.Result as OkObjectResult).Value as List<Category>).Count);
            }
        }

        [Test]
        public async Task Given_DatabaseHasCategoryWithId_When_Get_Then_ReturnCategory()
        {
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: "GetSuccess")
                .Options;

            using (var context = new Context(options))
            {
                context.Categories.AddRange(categories);
                context.SaveChanges();
            }

            using (var context = new Context(options))
            {
                var service = new CategoryController(new GenericRepository<Category>(context));
                var result = await service.GetCategory(categories[0].Id);
                Assert.AreEqual(categories[0].Id, (result.Value as Category).Id);
            }
        }

        [Test]
        public async Task Given_DatabaseHasCategoryWithoutId_When_Get_Then_Return404()
        {
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: "GetFail")
                .Options;

            using (var context = new Context(options))
            {
                context.Categories.AddRange(categories);
                context.SaveChanges();
            }

            using (var context = new Context(options))
            {
                var service = new CategoryController(new GenericRepository<Category>(context));
                var result = await service.GetCategory(new Guid());
                Assert.AreEqual(404, ((result as ActionResult<Category>).Result as StatusCodeResult).StatusCode);
            }
        }

        [Test]
        public async Task Given_ValidCategoryProvided_When_Post_Then_CreateCategory()
        {
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: "CreateCategory")
                .Options;

            var newCategory = new Category()
            {
                Name = "new category",
                Description = "some description"
            };

            using (var context = new Context(options))
            {
                var service = new CategoryController(new GenericRepository<Category>(context));
                var result = await service.PostCategory(newCategory);

                Assert.IsNotNull(context.Categories.Find(newCategory.Id));

                var actionResult = (result as ActionResult<Category>).Result as CreatedAtActionResult;

                Assert.AreEqual(201, actionResult.StatusCode);
                Assert.AreEqual("GetCategory", actionResult.ActionName);
                Assert.AreEqual(newCategory.Id, actionResult.RouteValues.GetValueOrDefault("id"));
                Assert.AreEqual(newCategory, actionResult.Value);
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
                context.Categories.AddRange(categories);
                context.SaveChanges();
            }

            using (var context = new Context(options))
            {
                var service = new CategoryController(new GenericRepository<Category>(context));
                var result = await service.PutCategory(new Guid(), categories[0]);
                Assert.AreEqual(400, (result as BadRequestResult).StatusCode);
            }
        }

        [Test]
        public async Task Given_ValidCategoryProvided_When_Put_Then_UpdateCategory()
        {
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: "PutSuccess")
                .Options;

            using (var context = new Context(options))
            {
                context.Categories.AddRange(categories);
                context.SaveChanges();
            }

            using (var context = new Context(options))
            {
                var updatedCategory = new Category()
                {
                    Id = categories[0].Id,
                    Name = categories[0].Name,
                    Description = "updated description",
                };

                var service = new CategoryController(new GenericRepository<Category>(context));
                var result = await service.PutCategory(updatedCategory.Id, updatedCategory);

                Assert.AreEqual(204, (result as StatusCodeResult).StatusCode);
                Assert.AreEqual("updated description", context.Categories.Find(categories[0].Id).Description);
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
                context.Categories.AddRange(categories);
                context.SaveChanges();
            }

            using (var context = new Context(options))
            {
                var service = new CategoryController(new GenericRepository<Category>(context));
                var result = await service.DeleteCategory(new Guid());
                Assert.AreEqual(404, ((result as ActionResult<Category>).Result as StatusCodeResult).StatusCode);
            }
        }

        [Test]
        public async Task Given_IdDoesExist_When_Delete_Then_DeleteAndReturnCategory()
        {
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: "DeleteSuccess")
                .Options;

            using (var context = new Context(options))
            {
                context.Categories.AddRange(categories);
                context.SaveChanges();
            }

            using (var context = new Context(options))
            {
                var service = new CategoryController(new GenericRepository<Category>(context));
                var result = await service.DeleteCategory(categories[0].Id);
                Assert.AreEqual(1, await context.Categories.CountAsync());
            }
        }
    }
}