using Microsoft.AspNetCore.Identity;

namespace RecipeList.Models
{
    public class RecipeListUser : IdentityUser
    {
        public ICollection<Recipes> Recipes { get; set; }
    }
}
