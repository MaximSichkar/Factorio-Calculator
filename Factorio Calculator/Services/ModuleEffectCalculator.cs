
using System;
using System.Collections.Generic;

namespace Factorio_Calculator
{
    public class ModuleEffectCalculator
    {
        public bool AllowProductivity
        {
            get; set;
        }

        public ModuleEffectResult Calculate(Factory factory, IEnumerable<ModuleSlot> slots)
        {

            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            ModuleEffectResult result = new ModuleEffectResult();

            int slotCount = 0;

            if (slots != null)
            {
                foreach (ModuleSlot slot in slots)
                {
                    if (slot == null)
                    {
                        throw new ArgumentException("Обнаружен некорректный слот модуля.");
                    }

                    slotCount++;

                    if (slotCount > factory.ModuleSlots)
                    {
                        throw new ArgumentException("Превышено количество слотов завода.");
                    }

                    Module module = slot.InstalledModule;

                    if (module == null)
                    {
                        continue;
                    }

                    result.SpeedBonus += module.SpeedBonus;
                    result.ProductivityBonus += module.ProductivityBonus;
                    result.EnergyBonus += module.EnergyBonus;
                }
            }

            result.ProductivityBonus += factory.BaseProductivity;

            result.SpeedMultiplier = Math.Max(0.2, 1 + result.SpeedBonus);
            result.ProductivityBonus = Math.Min(3.0, result.ProductivityBonus);
            result.ProductivityMultiplier = 1 + result.ProductivityBonus;
            result.EnergyMultiplier = Math.Max(0.2, 1 + result.EnergyBonus);

            return result;
        }
    }
}
