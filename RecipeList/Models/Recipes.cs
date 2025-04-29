using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecipeList.Models
{
    public class Recipes
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int Calories { get; set; }
        public int Fats { get; set; }
        public int Carbs { get; set; }
        public int Proteins { get; set; }
        public string Description { get; set; }
        public string? UserId { get; set; }
        
        [ForeignKey("UserId")]
        public virtual RecipeListUser? User { get; set; }


    }
}
    