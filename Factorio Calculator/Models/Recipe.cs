
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Factorio_Calculator
{
    public class Recipe
    {
        public string Id { get; set; } = string.Empty;
        public bool AllowProductivity { get; set; } = false;
        public string Category { get; set; } = RecipeCategory.Crafting;
        public List<string> Categories { get; set; } = new List<string>();
        public string Name { get; set; } = string.Empty;
        public double CraftTime { get; set; } = 1;
        public double OutputAmount { get; set; } = 1;
        public string Group { get; set; } = string.Empty;
        public string IconPath { get; set; } = string.Empty;
        public string Subgroup { get; set; } = string.Empty;
        public string SubgroupOrder { get; set; } = string.Empty;
        public string SortOrder { get; set; } = string.Empty;



        public ObservableCollection<Ingredient> Ingredients { get; set; } = new ObservableCollection<Ingredient>();
    }
}
