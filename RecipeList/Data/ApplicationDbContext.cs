using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RecipeList.Models;

namespace RecipeList.Data
{
    public class ApplicationDbContext : IdentityDbContext<RecipeListUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Recipes> Recipes { get; set; }
        public DbSet<PublicRecipe> PublicRecipes { get; set; }
        public DbSet<Pictures> Pictures { get; set; }
    }
}
