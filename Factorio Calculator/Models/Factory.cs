
using System.Collections.Generic;

namespace Factorio_Calculator
{
    public class Factory
    {
        public string Name { get; set; } = string.Empty;
        public double CraftingSpeed { get; set; } = 1;
        public string IconPath { get; set; } = string.Empty;
        public int ModuleSlots
        {
            get; set;
        }
        public double BaseProductivity
        {
            get; set;
        }

        public List<string> SupportedCategories { get; set; } = new List<string>();
        public List<string> AllowedRecipes { get; set; } = new List<string>();
        public List<string> BlockedRecipes { get; set; } = new List<string>();


        public bool CanCraft(Recipe recipe)
        {
            if (recipe == null)
            {
                return false;
            }

            if (BlockedRecipes.Contains(recipe.Id))
            {
                return false;
            }

            if (AllowedRecipes.Contains(recipe.Id))
            {
                return true;
            }

            if (recipe.Categories != null && recipe.Categories.Count > 0)
            {
                foreach (string category in recipe.Categories)
                {
                    if (SupportedCategories.Contains(category))
                    {
                        return true;
                    }
                }

                return false;
            }

            return SupportedCategories.Contains(recipe.Category);
        }

    }
}
