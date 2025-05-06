using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeList.Data;
using RecipeList.Models;

namespace RecipeList.Controllers
{
    [Authorize]
    public class PublicRecipeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<RecipeListUser> _userManager;

        public PublicRecipeController(ApplicationDbContext context, UserManager<RecipeListUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // POST: PublicRecipe/Publish
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Publish(int id)
        {
            var userId = _userManager.GetUserId(User);

            var recipe = await _context.Recipes
                .Include(r => r.Picture)
                .FirstOrDefaultAsync(r => r.ID == id && r.UserId == userId);

            if (recipe == null)
                return NotFound();

            bool alreadyPublished = await _context.PublicRecipes
                .AnyAsync(r => r.OriginalRecipeId == recipe.ID);

            if (alreadyPublished)
            {
                TempData["ErrorMessage"] = "This recipe has already been published.";
                return RedirectToAction("Index", "Recipes");
            }

            var publicRecipe = new PublicRecipe
            {
                Name = recipe.Name,
                Description = recipe.Description,
                Calories = recipe.Calories,
                Proteins = recipe.Proteins,
                Fats = recipe.Fats,
                Carbs = recipe.Carbs,
                PictureID = recipe.PictureID,
                UserId = recipe.UserId,
                OriginalRecipeId = recipe.ID,
                PublishedAt = DateTime.UtcNow
            };

            _context.PublicRecipes.Add(publicRecipe);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Recipe published successfully!";
            return RedirectToAction("Index", "PublicRecipe");
        }

        // GET: PublicRecipe
        public async Task<IActionResult> Index()
        {
            var publicRecipes = await _context.PublicRecipes
                .Include(r => r.User)
                .Include(r => r.Picture)
                .ToListAsync();

            return View(publicRecipes);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int id)
        {
            var userId = _userManager.GetUserId(User);

            var publicRecipe = await _context.PublicRecipes
                .FirstOrDefaultAsync(r => r.ID == id && r.UserId == userId);

            if (publicRecipe == null)
            {
                return NotFound(); // Either it doesn't exist or doesn't belong to the current user
            }

            _context.PublicRecipes.Remove(publicRecipe);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
    