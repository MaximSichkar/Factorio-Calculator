
namespace Factorio_Calculator
{
    public class ModuleEffectResult
    {
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

        public double SpeedMultiplier { get; set; } = 1;
        public double ProductivityMultiplier { get; set; } = 1;
        public double EnergyMultiplier { get; set; } = 1;
    }
}
