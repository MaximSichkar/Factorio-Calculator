
using System.Collections.Generic;

namespace Factorio_Calculator
{
    public class RecipeSubgroup
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Order { get; set; } = string.Empty;

        public List<Recipe> Recipes { get; set; } = new List<Recipe>();
    }
}
