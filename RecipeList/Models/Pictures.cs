using Microsoft.EntityFrameworkCore;

namespace RecipeList.Models
{
    public class Pictures
    {
        public int ID{ get; set; }
        public string FileName { get; set; }
        public string PicturePath { get; set; }
        public Recipes Recipe { get; set; }
    }
}
