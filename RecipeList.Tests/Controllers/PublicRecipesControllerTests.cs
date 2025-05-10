using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using RecipeList.Controllers;
using RecipeList.Data;
using RecipeList.Models;
using System;
using System.Linq;
using Microsoft.AspNetCore.Http;

public class PublicRecipeControllerTests
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<UserManager<RecipeListUser>> _userManagerMock;

    public PublicRecipeControllerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        _context = new ApplicationDbContext(options);

        var store = new Mock<IUserStore<RecipeListUser>>();
        _userManagerMock = new Mock<UserManager<RecipeListUser>>(store.Object, null, null, null, null, null, null, null, null);
    }

    private ClaimsPrincipal CreateTestUser(string userId)
    {
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        return new ClaimsPrincipal(identity);
    }

    [Fact]
    public async Task Publish_RecipeExists_AddsToPublicRecipes()
    {
        // Arrange
        var userId = "user123";
        var recipe = new Recipes { ID = 1, Name = "Test", UserId = userId };
        _context.Recipes.Add(recipe);
        _context.SaveChanges();

        var controller = new PublicRecipeController(_context, _userManagerMock.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = CreateTestUser(userId) }
        };

        _userManagerMock.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);

        // Act
        var result = await controller.Publish(1);

        // Assert
        var publicRecipe = _context.PublicRecipes.FirstOrDefault();
        Assert.NotNull(publicRecipe);
        Assert.Equal("Test", publicRecipe.Name);
        Assert.IsType<RedirectToActionResult>(result);
    }

    [Fact]
    public async Task Remove_RecipeExists_RemovesFromPublicRecipes()
    {
        var userId = "user456";
        var recipe = new PublicRecipe { ID = 2, Name = "ToRemove", UserId = userId };
        _context.PublicRecipes.Add(recipe);
        _context.SaveChanges();

        var controller = new PublicRecipeController(_context, _userManagerMock.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = CreateTestUser(userId) }
        };

        _userManagerMock.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);

        var result = await controller.Remove(2);

        Assert.False(_context.PublicRecipes.Any(r => r.ID == 2));
        Assert.IsType<RedirectToActionResult>(result);
    }

    [Fact]
    public async Task Copy_PublicRecipeExists_CreatesNewPersonalRecipe()
    {
        var userId = "user789";
        var picture = new Pictures { ID = 5, PicturePath = "/img.png" };
        _context.Pictures.Add(picture);
        var publicRecipe = new PublicRecipe
        {
            ID = 3,
            Name = "CopyMe",
            Description = "Delicious",
            UserId = "otherUser",
            PictureID = 5,
            Picture = picture
        };
        _context.PublicRecipes.Add(publicRecipe);
        _context.SaveChanges();

        var controller = new PublicRecipeController(_context, _userManagerMock.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = CreateTestUser(userId) }
        };

        _userManagerMock.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);

        var result = await controller.Copy(3);

        Assert.True(_context.Recipes.Any(r => r.Name == "CopyMe" && r.UserId == userId));
        Assert.IsType<RedirectToActionResult>(result);
    }

    [Fact]
    public async Task Details_RecipeExists_ReturnsView()
    {
        var userId = "userDetail";
        var recipe = new PublicRecipe { ID = 4, Name = "DetailsTest", UserId = userId };
        _context.PublicRecipes.Add(recipe);
        _context.SaveChanges();

        var controller = new PublicRecipeController(_context, _userManagerMock.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = CreateTestUser(userId) }
        };

        _userManagerMock.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);

        var result = await controller.Details(4);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<PublicRecipe>(viewResult.Model);
        Assert.Equal("DetailsTest", model.Name);
    }

    [Fact]
    public async Task Index_ReturnsPublicRecipesView()
    {
        _context.PublicRecipes.Add(new PublicRecipe { ID = 5, Name = "Recipe1" });
        _context.SaveChanges();

        var controller = new PublicRecipeController(_context, _userManagerMock.Object);

        var result = await controller.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<PublicRecipe>>(viewResult.Model);
        Assert.Single(model);
    }
}
