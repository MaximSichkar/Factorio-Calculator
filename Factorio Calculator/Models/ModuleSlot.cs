
namespace Factorio_Calculator
{
    public class ModuleSlot
    {
        public int Index
        {
            get; set;
        }
        public Module InstalledModule
        {
            get; set;
        }

        public bool IsEmpty
        {
            get
            {
                return InstalledModule == null;
            }
        }
    }
}
