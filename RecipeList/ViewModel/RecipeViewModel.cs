using RecipeList.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecipeList.ViewModel
{
    public class RecipeViewModel
    {
        public string Name { get; set; }
        public int Calories { get; set; }
        public int Fats { get; set; }
        public int Carbs { get; set; }
        public int Proteins { get; set; }
        public string Description { get; set; }
        public IFormFile Picture { get; set; }

    }
}
