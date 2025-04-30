using System.ComponentModel.DataAnnotations.Schema;

namespace RecipeList.Models
{
    public class PublicRecipe
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int Calories { get; set; }
        public int Fats { get; set; }
        public int Carbs { get; set; }
        public int Proteins { get; set; }
        public string Description { get; set; }
        public int OriginalRecipeId { get; set; }
        public DateTime PublishedAt { get; set; } = DateTime.UtcNow;
        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual RecipeListUser? User { get; set; }
    }
}
