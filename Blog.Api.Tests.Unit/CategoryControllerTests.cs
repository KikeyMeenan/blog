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
                var service = new CategoryController(context);
                var result = await service.GetCategories();
                Assert.AreEqual(categories.Count, (result.Value as List<Category>).Count);
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
                var service = new CategoryController(context);
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
                var service = new CategoryController(context);
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
                var service = new CategoryController(context);
                var result = await service.PostCategory(newCategory);

                Assert.IsNotNull(context.Categories.Find(newCategory.Id));

                var actionResult = (result as ActionResult<Category>).Result as CreatedAtActionResult;

                Assert.AreEqual(201, actionResult.StatusCode);
                Assert.AreEqual("GetCategory", actionResult.ActionName);
                Assert.AreEqual(newCategory.Id, actionResult.RouteValues.GetValueOrDefault("id"));
                Assert.AreEqual(newCategory, actionResult.Value);
            }
        }
    }
}