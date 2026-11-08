
namespace Factorio_Calculator
{
    public class Module
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string IconPath { get; set; } = string.Empty;

        public int Tier
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
    }
}
