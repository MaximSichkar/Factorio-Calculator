
using System.Collections.Generic;

namespace Factorio_Calculator
{
    public class ProductionResult
    {   
        public double EffectiveCraftingSpeed
        {
            get; set;
        }
        public double SpeedBonus
        {
            get; set;
        }
        public double ProductivityBonus
        {
            get; set;
        }
        public double EnergyBonus
        {
            get; set;
        }

        public double RequiredProduction
        {
            get; set;
        }
        public double ProductionPerFactory
        {
            get; set;
        }
        public double ExactFactoryCount
        {
            get; set;
        }
        public int FactoryCount
        {
            get; set;
        }
        public double MaximumProduction
        {
            get; set;
        }

        public List<IngredientConsumption> Ingredients { get; set; } = new List<IngredientConsumption>();
    }

    public class IngredientConsumption
    {
        public string Name { get; set; } = string.Empty;
        public double RequiredPerSecond
        {
            get; set;
        }
        public double MaximumPerSecond
        {
            get; set;
        }
    }
}
