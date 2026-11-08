
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Factorio_Calculator
{
    public class RecipeRepository
    {
        private readonly List<Recipe> recipes;

        public RecipeRepository()
        {
            recipes = new List<Recipe>();

            LoadRecipes();
        }

        private void LoadRecipes()
        {
            string filePath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Data",
                "recipes.json"
            );

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(
                    "Файл рецептов не найден.",
                    filePath
                );
            }

            string json = File.ReadAllText(filePath);

            List<Recipe> loadedRecipes = JsonConvert.DeserializeObject<List<Recipe>>(json);

            if (loadedRecipes == null)
            {
                throw new InvalidDataException("Не удалось загрузить рецепты.");
            }

            recipes.AddRange(loadedRecipes);
        }

        public List<Recipe> GetAll()
        {
            return new List<Recipe>(recipes);
        }

        public Recipe GetById(string id)
        {
            foreach (Recipe recipe in recipes)
            {
                if (recipe.Id == id)
                {
                    return recipe;
                }
            }

            return null;
        }

        public List<Recipe> GetByGroup(string group)
        {
            List<Recipe> result = new List<Recipe>();

            foreach (Recipe recipe in recipes)
            {
                if (recipe.Group == group)
                {
                    result.Add(recipe);
                }
            }

            return result;
        }
    }
}
