using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.EntityFrameworkCore;
using RecipeList.Data;
using RecipeList.Models;
using System.IO;
using Microsoft.AspNetCore.Http;
using RecipeList.ViewModel;


namespace RecipeList.Controllers
{
    public class RecipesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<RecipeListUser> _userManager;
        private readonly IWebHostEnvironment _ENV;

        public RecipesController(ApplicationDbContext context, UserManager<RecipeListUser> userManager, IWebHostEnvironment ENV)
        {
            _context = context;
            _userManager = userManager;
            _ENV = ENV;
        }

        // GET: Recipes
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var recipes = await _context.Recipes
                .Where(r => r.UserId == user.Id)
                .Include(r => r.Picture)
                .Include(r => r.User)
                .ToListAsync();

            return View(recipes);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);

            var recipe = await _context.Recipes
                .Include(r => r.Picture)
                .FirstOrDefaultAsync(m => m.ID == id && m.UserId == userId);

            if (recipe == null)
                return NotFound();

            // Check if recipe is copied from public (based on matching userId and content)
            bool isCopied = await _context.PublicRecipes.AnyAsync(pr =>
                pr.Name == recipe.Name &&
                pr.Description == recipe.Description &&
                pr.PictureID == recipe.PictureID &&
                pr.Calories == recipe.Calories &&
                pr.Proteins == recipe.Proteins &&
                pr.Fats == recipe.Fats &&
                pr.Carbs == recipe.Carbs &&
                pr.UserId != userId);

            ViewBag.IsCopiedFromPublic = isCopied;

            // Check if recipe is already published
            bool alreadyPublished = await _context.PublicRecipes.AnyAsync(pr =>
                pr.Name == recipe.Name &&
                pr.Description == recipe.Description &&
                pr.PictureID == recipe.PictureID &&
                pr.Calories == recipe.Calories &&
                pr.Proteins == recipe.Proteins &&
                pr.Fats == recipe.Fats &&
                pr.Carbs == recipe.Carbs &&
                pr.UserId == userId);

            // Show publish button only if recipe is not copied and not already published
            ViewBag.CanPublish = !isCopied && !alreadyPublished;

            return View(recipe);
        }



        // GET: Recipes/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RecipeViewModel recipes)
        {
            if (!ModelState.IsValid)
            {
                // Log or print validation errors
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine(error.ErrorMessage); // Or log it to a file or debug console
                }
                return View(recipes); // Return the view with errors
            }
            
            if (ModelState.IsValid)
            {
                var picture = recipes.Picture;
                var UploadPath = Path.Combine(_ENV.WebRootPath, "Pictures");
                var FileNAme = Path.GetFileName(recipes.Picture.FileName);
                var FilePath = Path.Combine(UploadPath, FileNAme);
                using (var Stream = new FileStream(FilePath, FileMode.Create)) { await picture.CopyToAsync(Stream); }
                var picture1 = new Pictures
                {
                    FileName = FileNAme,
                    PicturePath = $"/Pictures/{FileNAme}"
                };

                _context.Pictures.Add(picture1);
                await _context.SaveChangesAsync();
                var recipe = new Recipes
                {
                    Name = recipes.Name,
                    Calories = recipes.Calories,
                    Fats = recipes.Fats,
                    Carbs = recipes.Carbs,
                    Proteins = recipes.Proteins,
                    Description = recipes.Description,
                    PictureID = picture1.ID,
                };
                var user = await _userManager.GetUserAsync(User);
                recipe.UserId = user.Id;
                _context.Recipes.Add(recipe);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));

            }
            return View(recipes);
        }

        // GET: Recipes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var recipe = await _context.Recipes
                .Include(r => r.Picture)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (recipe == null)
            {
                return NotFound();
            }

            // Prevent editing if it's a public recipe
            var isCopiedToPublic = await _context.PublicRecipes
                .AnyAsync(p => p.OriginalRecipeId == recipe.ID);

            if (isCopiedToPublic)
            {
                TempData["ErrorMessage"] = "This recipe has been published and cannot be edited.";
                return RedirectToAction(nameof(Index));
            }

            return View(recipe);
        }


        // POST: Recipes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name,UserId,Calories,Fats,Carbs,Proteins,Description")] Recipes recipe)
        {
            if (id != recipe.ID)
            {
                return NotFound();
            }

            ModelState.Remove("Picture");

            if (ModelState.IsValid)
            {
                try
                {
                    var existingRecipe = await _context.Recipes.FindAsync(id);
                    if (existingRecipe == null)
                        return NotFound();

                    // Only update basic fields
                    existingRecipe.Name = recipe.Name;
                    existingRecipe.Calories = recipe.Calories;
                    existingRecipe.Fats = recipe.Fats;
                    existingRecipe.Carbs = recipe.Carbs;
                    existingRecipe.Proteins = recipe.Proteins;
                    existingRecipe.Description = recipe.Description;

                    _context.Update(existingRecipe);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RecipesExists(recipe.ID))
                        return NotFound();
                    else
                        throw;
                }
            }

            return View(recipe);
        }
    



        // GET: Recipes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var recipes = await _context.Recipes
                .FirstOrDefaultAsync(m => m.ID == id);
            if (recipes == null)
            {
                return NotFound();
            }

            return View(recipes);
        }

        // POST: Recipes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var recipe = await _context.Recipes.FindAsync(id);
            if (recipe == null)
            {
                return NotFound();
            }

            // Remove the corresponding public recipe if it exists
            var publicRecipe = await _context.PublicRecipes
                .FirstOrDefaultAsync(p => p.OriginalRecipeId == recipe.ID);

            if (publicRecipe != null)
            {
                _context.PublicRecipes.Remove(publicRecipe);
            }

            // Remove the original private recipe
            _context.Recipes.Remove(recipe);

            // Save all changes in one go
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        private bool RecipesExists(int id)
        {
            return _context.Recipes.Any(e => e.ID == id);
        }
    }
}
