
using System.Collections.Generic;

namespace Factorio_Calculator
{
    public class ModuleRepository
    {
        private readonly List<Module> modules;

        public ModuleRepository()
        {
            modules = new List<Module>();

            LoadDefaultModules();
        }

        private void LoadDefaultModules()
        {
            AddModule("speed-module", "Модуль скорости 1", 1, 0.20, 0, 0.50);
            AddModule("speed-module-2", "Модуль скорости 2", 2, 0.30, 0, 0.60);
            AddModule("speed-module-3", "Модуль скорости 3", 3, 0.50, 0, 0.70);

            AddModule("productivity-module", "Модуль продуктивности 1", 1, -0.05, 0.04, 0.40);
            AddModule("productivity-module-2", "Модуль продуктивности 2", 2, -0.10, 0.06, 0.60);
            AddModule("productivity-module-3", "Модуль продуктивности 3", 3, -0.15, 0.10, 0.80);

            AddModule("efficiency-module", "Модуль эффективности 1", 1, 0, 0, -0.30);
            AddModule("efficiency-module-2", "Модуль эффективности 2", 2, 0, 0, -0.40);
            AddModule("efficiency-module-3", "Модуль эффективности 3", 3, 0, 0, -0.50);
        }

        private void AddModule(
            string id,
            string name,
            int tier,
            double speedBonus,
            double productivityBonus,
            double energyBonus)
        {
            Module module = new Module
            {
                Id = id,
                Name = name,
                Tier = tier,
                SpeedBonus = speedBonus,
                ProductivityBonus = productivityBonus,
                EnergyBonus = energyBonus,
                IconPath = "/Assets/Recipes/" + id + ".png"
            };

            modules.Add(module);
        }

        public List<Module> GetAll()
        {
            return new List<Module>(modules);
        }
    }
}
