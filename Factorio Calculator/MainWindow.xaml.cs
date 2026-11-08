
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Factorio_Calculator
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<ModuleSlot> moduleSlots;
        private ObservableCollection<Ingredient> ingredients;
        private FactoryRepository factoryRepository;
        private Recipe selectedRecipe;

        public MainWindow()
        {
            InitializeComponent();

            moduleSlots = new ObservableCollection<ModuleSlot>();
            ModuleSlotsItemsControl.ItemsSource = moduleSlots;
            ingredients = new ObservableCollection<Ingredient>();
            IngredientsDataGrid.ItemsSource = ingredients;
            factoryRepository = new FactoryRepository();

            FactoryListBox.ItemsSource = factoryRepository.GetAll();
            FactoryListBox.SelectedIndex = 0;


            RecipeRepository recipeRepository = new RecipeRepository();

            List<Recipe> loadedRecipes = recipeRepository.GetAll();

            MessageBox.Show(
                "Загружено рецептов: " + loadedRecipes.Count,
                "Проверка каталога"
            );


            string scriptOutputPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Factorio",
                "script-output"
            );


            Factory testFactory = new Factory
            {
                Name = "Тестовый электромагнитный завод",
                CraftingSpeed = 2,
                ModuleSlots = 4,
                SupportedCategories = new List<string>
    {
        "electromagnetics"
    }
            };

            RecipeRepository repository = new RecipeRepository();
            Recipe electronicCircuit = repository.GetById("electronic-circuit");

            bool canCraft = testFactory.CanCraft(electronicCircuit);

            MessageBox.Show("Совместимость: " + canCraft);


            /*
            string projectRootPath = @"C:\Users\clash\OneDrive\Desktop\FactorioCalculatorSolution\Factorio Calculator";

            FactorioDataImporter importer = new FactorioDataImporter();

            int importedCount = importer.ImportRecipes(scriptOutputPath, projectRootPath);

            MessageBox.Show("Импортировано рецептов: " + importedCount);
            */
        }


        private void UpdateModuleSlots(Factory factory)
        {
            moduleSlots.Clear();

            if (factory == null)
            {
                return;
            }

            for (int i = 0; i < factory.ModuleSlots; i++)
            {
                ModuleSlot slot = new ModuleSlot();

                slot.Index = i;

                moduleSlots.Add(slot);
            }
        }


        private void AddIngredient_Click(object sender, RoutedEventArgs e)
        {
            Ingredient ingredient = new Ingredient();

            ingredients.Add(ingredient);
            IngredientsDataGrid.SelectedItem = ingredient;
        }

        private void RemoveIngredient_Click(object sender, RoutedEventArgs e)
        {
            Ingredient selectedIngredient = IngredientsDataGrid.SelectedItem as Ingredient;

            if (selectedIngredient != null)
            {
                ingredients.Remove(selectedIngredient);
            }
        }

        private void OpenRecipeCatalog_Click(object sender, RoutedEventArgs e)
        {
            RecipeSelectionWindow window = new RecipeSelectionWindow();
            window.Owner = this;

            if (window.ShowDialog() != true)
            {
                return;
            }

            Recipe recipe = window.SelectedRecipe;

            selectedRecipe = recipe;
            ValidateInstalledModules(recipe);

            ClearProductionResult();

            RecipeNameTextBox.Text = recipe.Name;
            CraftTimeTextBox.Text = recipe.CraftTime.ToString();
            OutputAmountTextBox.Text = recipe.OutputAmount.ToString();

            ingredients.Clear();

            foreach (Ingredient ingredient in recipe.Ingredients)
            {
                ingredients.Add(new Ingredient
                {
                    Name = ingredient.Name,
                    Amount = ingredient.Amount
                });
            }
        }


        private void FactoryListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Factory selectedFactory = FactoryListBox.SelectedItem as Factory;

            UpdateModuleSlots(selectedFactory);

            if (selectedFactory == null)
            {
                FactoryNameTextBlock.Text = string.Empty;
                FactorySpeedTextBlock.Text = string.Empty;
                FactoryModulesTextBlock.Text = string.Empty;
                return;
            }

            FactoryNameTextBlock.Text = selectedFactory.Name;
            FactorySpeedTextBlock.Text = "Скорость производства: " + selectedFactory.CraftingSpeed;
            FactoryModulesTextBlock.Text = "Слотов для модулей: " + selectedFactory.ModuleSlots;
        }
        private void ModuleSlot_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (button == null)
            {
                return;
            }

            ModuleSlot slot = button.Tag as ModuleSlot;

            if (slot == null)
            {
                return;
            }

            bool allowProductivity = selectedRecipe != null &&
                                     selectedRecipe.AllowProductivity;

            ModuleSelectionWindow window = new ModuleSelectionWindow(allowProductivity);
            window.Owner = this;

            if (window.ShowDialog() != true)
            {
                return;
            }

            slot.InstalledModule = window.SelectedModule;

            ModuleSlotsItemsControl.Items.Refresh();
            ClearProductionResult();
        }

        private void ValidateInstalledModules(Recipe recipe)
        {
            if (recipe == null || recipe.AllowProductivity)
            {
                return;
            }

            foreach (ModuleSlot slot in moduleSlots)
            {
                if (slot.InstalledModule == null)
                {
                    continue;
                }

                if (slot.InstalledModule.ProductivityBonus > 0)
                {
                    slot.InstalledModule = null;
                }
            }

            ModuleSlotsItemsControl.Items.Refresh();
        }

        private Recipe GetRecipeFromInput()
        {
            double craftTime;
            double outputAmount;

            if (string.IsNullOrWhiteSpace(RecipeNameTextBox.Text))
            {
                throw new ArgumentException("Введите название рецепта.");
            }

            if (!double.TryParse(CraftTimeTextBox.Text, out craftTime) ||
                double.IsNaN(craftTime) ||
                double.IsInfinity(craftTime) ||
                craftTime <= 0)
            {
                throw new ArgumentException("Укажите корректное время производства.");
            }

            if (!double.TryParse(OutputAmountTextBox.Text, out outputAmount) ||
                double.IsNaN(outputAmount) ||
                double.IsInfinity(outputAmount) ||
                outputAmount <= 0)
            {
                throw new ArgumentException("Укажите корректное количество продукта.");
            }

            IngredientsDataGrid.CommitEdit(DataGridEditingUnit.Cell, true);
            IngredientsDataGrid.CommitEdit(DataGridEditingUnit.Row, true);

            Recipe recipe = new Recipe
            {
                Id = selectedRecipe != null ? selectedRecipe.Id : RecipeNameTextBox.Text.Trim(),
                Name = RecipeNameTextBox.Text.Trim(),
                Category = selectedRecipe != null ? selectedRecipe.Category : RecipeCategory.Crafting,
                CraftTime = craftTime,
                OutputAmount = outputAmount,
                AllowProductivity = selectedRecipe != null && selectedRecipe.AllowProductivity,
            };


            if (selectedRecipe != null &&
                selectedRecipe.Categories != null &&
                selectedRecipe.Categories.Count > 0)
            {
                foreach (string category in selectedRecipe.Categories)
                {
                    recipe.Categories.Add(category);
                }
            }
            else
            {
                recipe.Categories.Add(recipe.Category);
            }


            foreach (Ingredient ingredient in ingredients)
            {
                if (string.IsNullOrWhiteSpace(ingredient.Name) ||
                    double.IsNaN(ingredient.Amount) ||
                    double.IsInfinity(ingredient.Amount) ||
                    ingredient.Amount <= 0)
                {
                    throw new ArgumentException("Проверьте ингредиенты рецепта.");
                }

                recipe.Ingredients.Add(ingredient);
            }

            return recipe;
        }

        private void CalculateProduction_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                double requiredProduction;

                if (!double.TryParse(RequiredProductionTextBox.Text, out requiredProduction) ||
                    double.IsNaN(requiredProduction) ||
                    double.IsInfinity(requiredProduction) ||
                    requiredProduction <= 0)
                {
                    throw new ArgumentException("Введите корректное количество продуктов в секунду.");
                }

                Factory selectedFactory = FactoryListBox.SelectedItem as Factory;

                Recipe recipe = GetRecipeFromInput();

                ProductionCalculator calculator = new ProductionCalculator();


                ProductionResult result = calculator.Calculate(
                    recipe,
                    selectedFactory,
                    requiredProduction,
                    moduleSlots
                );


                ProductionResultTextBlock.Text =
                    "Бонус скорости: " + result.SpeedBonus.ToString("P0") +
                    "\nБонус продуктивности: " + result.ProductivityBonus.ToString("P0") +
                    "\nБонус энергопотребления: " + result.EnergyBonus.ToString("P0") +
                    "\nИтоговая скорость завода: " + result.EffectiveCraftingSpeed.ToString("N3") +
                    "\nПроизводительность одного завода: " + result.ProductionPerFactory.ToString("N2") + " шт/с" +
                    "\nТеоретическое количество заводов: " + result.ExactFactoryCount.ToString("N2") +
                    "\nНеобходимо построить заводов: " + result.FactoryCount +
                    "\nМаксимальный выпуск: " + result.MaximumProduction.ToString("N2") + " шт/с";

                ConsumptionDataGrid.ItemsSource = result.Ingredients;
            }
            catch (Exception exception)
            {
                ProductionResultTextBlock.Text = string.Empty;
                ConsumptionDataGrid.ItemsSource = null;

                MessageBox.Show(exception.Message, "Ошибка расчёта");
            }
        }


        private void CheckRecipe_Click(object sender, RoutedEventArgs e)
        {
            IngredientsDataGrid.CommitEdit(DataGridEditingUnit.Cell, true);
            IngredientsDataGrid.CommitEdit(DataGridEditingUnit.Row, true);

            double craftTime;
            double outputAmount;

            if (string.IsNullOrWhiteSpace(RecipeNameTextBox.Text))
            {
                MessageBox.Show("Введите название рецепта.");
                return;
            }

            if (!double.TryParse(CraftTimeTextBox.Text, out craftTime) || craftTime <= 0)
            {
                MessageBox.Show("Укажите корректное время производства.");
                return;
            }

            if (!double.TryParse(OutputAmountTextBox.Text, out outputAmount) || outputAmount <= 0)
            {
                MessageBox.Show("Укажите корректное количество продукта.");
                return;
            }

            foreach (Ingredient ingredient in ingredients)
            {
                if (string.IsNullOrWhiteSpace(ingredient.Name) || ingredient.Amount <= 0)
                {
                    MessageBox.Show("Проверьте названия и количество ингредиентов.");
                    return;
                }
            }

            Recipe recipe = new Recipe();

            recipe.Name = RecipeNameTextBox.Text.Trim();
            recipe.CraftTime = craftTime;
            recipe.OutputAmount = outputAmount;

            foreach (Ingredient ingredient in ingredients)
            {
                recipe.Ingredients.Add(ingredient);
            }


            Factory selectedFactory = FactoryListBox.SelectedItem as Factory;

            if (selectedFactory == null)
            {
                MessageBox.Show("Выберите производственную машину.");
                return;
            }

            if (!selectedFactory.CanCraft(recipe))
            {
                MessageBox.Show("Выбранная машина не поддерживает этот рецепт.");
                return;
            }


            MessageBox.Show(
                "Рецепт: " + recipe.Name +
                "\nВремя производства: " + recipe.CraftTime +
                "\nПродуктов за цикл: " + recipe.OutputAmount +
                "\nИнгредиентов: " + recipe.Ingredients.Count,
                "Проверка рецепта");
        }

        private void ClearProductionResult()
        {
            ProductionResultTextBlock.Text = string.Empty;
            ConsumptionDataGrid.ItemsSource = null;
        }

    }
}
