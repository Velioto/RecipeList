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

        // GET: Recipes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var recipes = await _context.Recipes.Include(r => r.User)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (recipes == null)
            {
                return NotFound();
            }

            return View(recipes);
        }

        // GET: Recipes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Recipes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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
                using(var Stream = new FileStream(FilePath, FileMode.Create)) { await picture.CopyToAsync(Stream); }
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

            var recipe = await _context.Recipes.FindAsync(id);
            if (recipe == null)
            {
                return NotFound();
            }

            // Check if the recipe has been copied to the public list
            var isCopiedToPublic = await _context.PublicRecipes
                                                   .AnyAsync(p => p.OriginalRecipeId == recipe.ID);

            if (isCopiedToPublic)
            {
                // Optionally, you can add a message to show that the recipe cannot be edited
                TempData["ErrorMessage"] = "This recipe has been published and cannot be edited.";
                return RedirectToAction(nameof(Index)); // Redirect to the list of recipes
            }

            return View(recipe);
        }


        // POST: Recipes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name,UserID,Calories,Fats,Carbs,Proteins,Description")] Recipes recipes)
        {
            if (id != recipes.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(recipes);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RecipesExists(recipes.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(recipes);
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
