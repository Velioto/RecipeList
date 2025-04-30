using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeList.Data;
using RecipeList.Models;

namespace RecipeList.Controllers
{
    public class PublicRecipeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<RecipeListUser> _userManager;

        public PublicRecipeController(ApplicationDbContext context, UserManager<RecipeListUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize]
        public async Task<IActionResult> Publish(int? id)
        {
            if (id == null) return NotFound();

            var recipe = await _context.Recipes.FirstOrDefaultAsync(r => r.ID == id && r.UserId == _userManager.GetUserId(User));

            if (recipe == null) return NotFound();

            var publicRecipe = new PublicRecipe
            {
                Name = recipe.Name,
                Description = recipe.Description,
                Calories = recipe.Calories,
                Proteins = recipe.Proteins,
                Fats = recipe.Fats,
                Carbs = recipe.Carbs,
                PublishedAt = default,
                OriginalRecipeId = recipe.ID,
                UserId = recipe.UserId
            };

            return View(publicRecipe); // Preview before publishing
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> PublishConfirmed(PublicRecipe publicRecipe)
        {
            bool alreadyPublished = await _context.PublicRecipes
            .AnyAsync(r => r.OriginalRecipeId == publicRecipe.OriginalRecipeId);

            if (alreadyPublished)
            {
                ModelState.AddModelError("", "This recipe has already been published.");
                return View(publicRecipe); // Show the error
            }
            if (ModelState.IsValid)
            {
                _context.PublicRecipes.Add(publicRecipe);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "PublicRecipe");
            }

            return View(publicRecipe);
        }
        public async Task<IActionResult> Index()
        {
            var publicRecipes = await _context.PublicRecipes
            .Include(r => r.User) // optional, if you show author name
            .ToListAsync();
            return View(publicRecipes);
        }
    }
}
