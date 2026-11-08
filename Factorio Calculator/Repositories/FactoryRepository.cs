
using System.Collections.Generic;

namespace Factorio_Calculator
{
    public class FactoryRepository
    {
        private readonly List<Factory> factories;

        public FactoryRepository()
        {
            factories = new List<Factory>();

            LoadDefaultFactories();
        }

        private void LoadDefaultFactories()
        {
            factories.Add(new Factory
            {
                Name = "Сборочный автомат 1",
                CraftingSpeed = 0.5,
                ModuleSlots = 0,
                IconPath = "/Assets/Factories/assembling-machine-1.png",
                SupportedCategories = new List<string>
                {
                    RecipeCategory.Crafting,
                    RecipeCategory.AdvancedCrafting
                }
            });

            factories.Add(new Factory
            {
                Name = "Сборочный автомат 2",
                CraftingSpeed = 0.75,
                ModuleSlots = 2,
                IconPath = "/Assets/Factories/assembling-machine-2.png",
                SupportedCategories = new List<string>
                {
                    RecipeCategory.Crafting,
                    RecipeCategory.AdvancedCrafting,
                    RecipeCategory.CraftingWithFluid
                }
            });

            factories.Add(new Factory
            {
                Name = "Сборочный автомат 3",
                CraftingSpeed = 1.25,
                ModuleSlots = 4,
                IconPath = "/Assets/Factories/assembling-machine-3.png",
                SupportedCategories = new List<string>
                {
                    RecipeCategory.Crafting,
                    RecipeCategory.AdvancedCrafting,
                    RecipeCategory.CraftingWithFluid
                }
            });
        }

        public List<Factory> GetAll()
        {
            return new List<Factory>(factories);
        }

        public List<Factory> GetCompatibleFactories(Recipe recipe)
        {
            List<Factory> compatibleFactories = new List<Factory>();

            foreach (Factory factory in factories)
            {
                if (factory.CanCraft(recipe))
                {
                    compatibleFactories.Add(factory);
                }
            }

            return compatibleFactories;
        }
    }
}
