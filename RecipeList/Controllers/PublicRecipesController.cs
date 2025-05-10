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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Publish(int id)
        {
            var userId = _userManager.GetUserId(User);

            // Only select the recipe if it belongs to the current user
            var recipe = await _context.Recipes
                .Include(r => r.Picture)
                .FirstOrDefaultAsync(r => r.ID == id && r.UserId == userId);

            if (recipe == null)
                return NotFound();

            // Prevent duplicate publishings of the same recipe by the same user
            bool alreadyPublished = await _context.PublicRecipes
                .AnyAsync(r =>
                    r.Name == recipe.Name &&
                    r.Description == recipe.Description &&
                    r.PictureID == recipe.PictureID &&
                    r.Calories == recipe.Calories &&
                    r.Proteins == recipe.Proteins &&
                    r.Fats == recipe.Fats &&
                    r.Carbs == recipe.Carbs &&
                    r.UserId == userId);

            if (alreadyPublished)
            {
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
                PublishedAt = DateTime.UtcNow
            };

            _context.PublicRecipes.Add(publicRecipe);
            await _context.SaveChangesAsync();

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


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Copy(int id)
        {
            var userId = _userManager.GetUserId(User);

            var publicRecipe = await _context.PublicRecipes
                .Include(r => r.Picture)
                .FirstOrDefaultAsync(r => r.ID == id);

            if (publicRecipe == null)
                return NotFound();

            // Check if the user already copied this recipe (optional, but good to avoid dupes)
            bool alreadyCopied = await _context.Recipes.AnyAsync(r =>
                r.Name == publicRecipe.Name &&
                r.Description == publicRecipe.Description &&
                r.UserId == userId &&
                r.PictureID == publicRecipe.PictureID);

            if (alreadyCopied)
            {
                TempData["ErrorMessage"] = "You already have this recipe in your list.";
                return RedirectToAction("Index");
            }

            var copiedRecipe = new Recipes
            {
                Name = publicRecipe.Name,
                Description = publicRecipe.Description,
                Calories = publicRecipe.Calories,
                Proteins = publicRecipe.Proteins,
                Fats = publicRecipe.Fats,
                Carbs = publicRecipe.Carbs,
                PictureID = publicRecipe.PictureID, // Reuse picture
                UserId = userId
            };

            _context.Recipes.Add(copiedRecipe);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Recipe copied to your list!";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Details(int id)
        {
            var recipe = await _context.PublicRecipes
                .Include(r => r.Picture)
                .Include(r => r.User) // <-- Ensure the User is loaded
                .FirstOrDefaultAsync(r => r.ID == id);

            if (recipe == null)
            {
                return NotFound();
            }

            // Optional: check if recipe has been copied by this user to disable "Copy to My List"
            var userId = _userManager.GetUserId(User);
            bool isAlreadyCopied = await _context.Recipes.AnyAsync(r =>
                r.Name == recipe.Name &&
                r.Description == recipe.Description &&
                r.Calories == recipe.Calories &&
                r.Fats == recipe.Fats &&
                r.Carbs == recipe.Carbs &&
                r.Proteins == recipe.Proteins &&
                r.PictureID == recipe.PictureID &&
                r.UserId == userId
            );

            ViewBag.CanCopy = !isAlreadyCopied;

            return View(recipe);
        }


    }
}