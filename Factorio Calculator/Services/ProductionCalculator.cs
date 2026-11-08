
using System;
using System.Collections.Generic;

namespace Factorio_Calculator
{
    public class ProductionCalculator
    {
        public ProductionResult Calculate(
            Recipe recipe,
            Factory factory,
            double requiredProduction,
            IEnumerable<ModuleSlot> moduleSlots = null)
        {
            if (recipe == null)
            {
                throw new ArgumentNullException(nameof(recipe));
            }

            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            if (recipe.CraftTime <= 0 || recipe.OutputAmount <= 0)
            {
                throw new ArgumentException("Некорректные параметры рецепта.");
            }

            if (factory.CraftingSpeed <= 0 ||
                requiredProduction <= 0 ||
                double.IsNaN(requiredProduction) ||
                double.IsInfinity(requiredProduction))
            {
                throw new ArgumentException("Некорректные параметры производства.");
            }

            if (!factory.CanCraft(recipe))
            {
                throw new InvalidOperationException("Завод не поддерживает выбранный рецепт.");
            }

            ModuleEffectCalculator moduleCalculator = new ModuleEffectCalculator();
            ModuleEffectResult effects = moduleCalculator.Calculate(factory, moduleSlots);

            if (!recipe.AllowProductivity && effects.ProductivityBonus > 0)
            {
                throw new InvalidOperationException(
                    "Выбранный рецепт не допускает бонус продуктивности."
                );
            }

            double effectiveSpeed = factory.CraftingSpeed * effects.SpeedMultiplier;
            double craftsPerSecond = effectiveSpeed / recipe.CraftTime;
            double productionPerFactory = craftsPerSecond * recipe.OutputAmount * effects.ProductivityMultiplier;

            double exactFactoryCount = requiredProduction / productionPerFactory;
            double roundedFactoryCount = Math.Ceiling(exactFactoryCount);

            if (double.IsNaN(roundedFactoryCount) ||
                double.IsInfinity(roundedFactoryCount) ||
                roundedFactoryCount > int.MaxValue)
            {
                throw new ArgumentException("Количество заводов выходит за допустимые пределы.");
            }

            int factoryCount = (int)roundedFactoryCount;
            double maximumProduction = factoryCount * productionPerFactory;

            ProductionResult result = new ProductionResult
            {
                RequiredProduction = requiredProduction,
                ProductionPerFactory = productionPerFactory,
                ExactFactoryCount = exactFactoryCount,
                FactoryCount = factoryCount,
                MaximumProduction = maximumProduction,
                EffectiveCraftingSpeed = effectiveSpeed,
                SpeedBonus = effects.SpeedBonus,
                ProductivityBonus = effects.ProductivityBonus,
                EnergyBonus = effects.EnergyBonus
            };

            foreach (Ingredient ingredient in recipe.Ingredients)
            {
                if (string.IsNullOrWhiteSpace(ingredient.Name) ||
                    ingredient.Amount <= 0 ||
                    double.IsNaN(ingredient.Amount) ||
                    double.IsInfinity(ingredient.Amount))
                {
                    throw new ArgumentException("Некорректный ингредиент рецепта.");
                }

                double ingredientPerProduct =
                    ingredient.Amount / (recipe.OutputAmount * effects.ProductivityMultiplier);

                IngredientConsumption consumption = new IngredientConsumption
                {
                    Name = ingredient.Name,
                    RequiredPerSecond = requiredProduction * ingredientPerProduct,
                    MaximumPerSecond = maximumProduction * ingredientPerProduct
                };

                result.Ingredients.Add(consumption);
            }

            return result;
        }
    }
}
