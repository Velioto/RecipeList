using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using RecipeList.Controllers;
using RecipeList.Models;
using RecipeList.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

public class RecipesControllerTests
{
    private readonly Mock<ApplicationDbContext> _mockDbContext;
    private readonly Mock<DbSet<Recipes>> _mockDbSet;
    private readonly RecipesController _controller;

    public RecipesControllerTests()
    {
        // Initialize Mock DbSet
        _mockDbContext = new Mock<ApplicationDbContext>();
        _mockDbSet = new Mock<DbSet<Recipes>>();

        // Setting up the mock to return a list of recipes
        var recipes = new List<Recipes>
        {
            new Recipes { ID = 1, Name = "Recipe 1", Calories = 100 },
            new Recipes { ID = 2, Name = "Recipe 2", Calories = 200 }
        }.AsQueryable();

        _mockDbSet.As<IQueryable<Recipes>>().Setup(m => m.Provider).Returns(recipes.Provider);
        _mockDbSet.As<IQueryable<Recipes>>().Setup(m => m.Expression).Returns(recipes.Expression);
        _mockDbSet.As<IQueryable<Recipes>>().Setup(m => m.ElementType).Returns(recipes.ElementType);
        _mockDbSet.As<IQueryable<Recipes>>().Setup(m => m.GetEnumerator()).Returns(recipes.GetEnumerator());

        _mockDbContext.Setup(c => c.Recipes).Returns(_mockDbSet.Object);

        // Initialize the controller
        _controller = new RecipesController(_mockDbContext.Object, null, null);
    }

    [Fact]
    public async Task Index_Returns_View_With_List_Of_Recipes()
    {
        // Act
        var result = await _controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<List<Recipes>>(viewResult.Model);
        Assert.Equal(2, model.Count); // Check if the number of recipes matches
    }

    // Add more tests as needed
}
