using System.Collections.Generic;

namespace Factorio_Calculator
{
    public class FactorioImportRecipe
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Group { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string IconPath { get; set; } = string.Empty;
        public double CraftTime
        {
            get; set;
        }
        public double OutputAmount
        {
            get; set;
        }
        public List<Ingredient> Ingredients { get; set; } = new List<Ingredient>();
    }
}   