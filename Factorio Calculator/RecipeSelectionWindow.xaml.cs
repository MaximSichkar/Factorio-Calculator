
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Factorio_Calculator
{
    public partial class RecipeSelectionWindow : Window
    {
        private readonly List<Recipe> recipes;

        public Recipe SelectedRecipe
        {
            get; private set;
        }

        public RecipeSelectionWindow()
        {
            InitializeComponent();

            RecipeRepository repository = new RecipeRepository();
            recipes = repository.GetAll();

            LoadCategories();
            FilterRecipes();
        }

        private void LoadCategories()
        {
            List<RecipeGroup> groups = new List<RecipeGroup>();

            groups.Add(new RecipeGroup
            {
                Id = "",
                Name = "Все категории"
            });

            foreach (Recipe recipe in recipes)
            {
                bool exists = false;

                foreach (RecipeGroup group in groups)
                {
                    if (group.Id == recipe.Group)
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    groups.Add(new RecipeGroup
                    {
                        Id = recipe.Group,
                        Name = GetGroupName(recipe.Group)
                    });
                }
            }

            CategoryComboBox.ItemsSource = groups;
            CategoryComboBox.SelectedIndex = 0;
        }

        private string GetGroupName(string groupId)
        {
            switch (groupId)
            {
                case "logistics":
                    return "Логистика";

                case "production":
                    return "Производство";

                case "intermediate-products":
                    return "Компоненты";

                case "combat":
                    return "Военное";

                case "fluids":
                    return "Жидкости";

                default:
                    return groupId;
            }
        }


        private void FilterRecipes()
        {
            if (recipes == null)
            {
                return;
            }

            string search = SearchTextBox.Text.Trim();

            RecipeGroup selectedGroup = CategoryComboBox.SelectedItem as RecipeGroup;

            bool hideRecycling = HideRecyclingCheckBox != null &&
                                 HideRecyclingCheckBox.IsChecked == true;

            List<Recipe> filteredRecipes = new List<Recipe>();

            foreach (Recipe recipe in recipes)
            {
                bool matchesName = recipe.Name.IndexOf(
                    search,
                    StringComparison.CurrentCultureIgnoreCase
                ) >= 0;

                bool matchesGroup = selectedGroup == null ||
                                    string.IsNullOrEmpty(selectedGroup.Id) ||
                                    recipe.Group == selectedGroup.Id;

                bool isItemRecycling = IsItemRecyclingRecipe(recipe);

                if (matchesName && matchesGroup)
                {
                    if (hideRecycling && isItemRecycling)
                    {
                        continue;
                    }

                    filteredRecipes.Add(recipe);
                }
            }


            filteredRecipes.Sort(CompareRecipes);

            List<RecipeSubgroup> subgroups = BuildSubgroups(filteredRecipes);

            RecipeGroupsItemsControl.ItemsSource = subgroups;

        }

        private int CompareRecipes(Recipe first, Recipe second)
        {
            int result = StringComparer.Ordinal.Compare(
                first.SubgroupOrder,
                second.SubgroupOrder
            );

            if (result != 0)
            {
                return result;
            }

            result = StringComparer.Ordinal.Compare(
                first.Subgroup,
                second.Subgroup
            );

            if (result != 0)
            {
                return result;
            }

            result = StringComparer.Ordinal.Compare(
                first.SortOrder,
                second.SortOrder
            );

            if (result != 0)
            {
                return result;
            }

            return StringComparer.Ordinal.Compare(first.Id, second.Id);
        }


        private List<RecipeSubgroup> BuildSubgroups(List<Recipe> filteredRecipes)
        {
            List<RecipeSubgroup> groups = new List<RecipeSubgroup>();

            foreach (Recipe recipe in filteredRecipes)
            {
                string subgroupId = recipe.Subgroup;

                if (string.IsNullOrWhiteSpace(subgroupId))
                {
                    subgroupId = "other";
                }

                RecipeSubgroup selectedGroup = null;

                foreach (RecipeSubgroup group in groups)
                {
                    if (group.Id == subgroupId)
                    {
                        selectedGroup = group;
                        break;
                    }
                }

                if (selectedGroup == null)
                {
                    selectedGroup = new RecipeSubgroup
                    {
                        Id = subgroupId,
                        Name = subgroupId,
                        Order = recipe.SubgroupOrder
                    };

                    groups.Add(selectedGroup);
                }

                selectedGroup.Recipes.Add(recipe);
            }

            return groups;
        }

        private void RecipeIcon_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (button == null)
            {
                return;
            }

            Recipe recipe = button.Tag as Recipe;

            if (recipe == null)
            {
                return;
            }

            SelectedRecipe = recipe;
            DialogResult = true;
        }

        private bool IsItemRecyclingRecipe(Recipe recipe)
        {
            if (recipe == null || string.IsNullOrWhiteSpace(recipe.Id))
            {
                return false;
            }

            if (recipe.Id == "scrap-recycling")
            {
                return false;
            }

            return recipe.Id.EndsWith(
                "-recycling",
                StringComparison.OrdinalIgnoreCase
            );
        }


        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterRecipes();
        }

        private void CategoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterRecipes();
        }

        private void RecyclingFilter_Changed(object sender, RoutedEventArgs e)
        {
            FilterRecipes();
        }        

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
