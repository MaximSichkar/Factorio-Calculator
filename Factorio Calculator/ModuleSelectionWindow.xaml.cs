
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Factorio_Calculator
{
    public partial class ModuleSelectionWindow : Window
    {
        public Module SelectedModule
        {
            get; private set;
        }


        public ModuleSelectionWindow(bool allowProductivity)
        {
            InitializeComponent();

            ModuleRepository repository = new ModuleRepository();
            List<Module> availableModules = new List<Module>();

            foreach (Module module in repository.GetAll())
            {
                if (!allowProductivity && module.ProductivityBonus > 0)
                {
                    continue;
                }

                availableModules.Add(module);
            }

            ModulesItemsControl.ItemsSource = availableModules;
        }


        private void ModuleButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (button == null)
            {
                return;
            }

            Module selectedModule = button.Tag as Module;

            if (selectedModule == null)
            {
                return;
            }

            SelectedModule = selectedModule;
            DialogResult = true;
        }

        private void RemoveModule_Click(object sender, RoutedEventArgs e)
        {
            SelectedModule = null;
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
