using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace Factorio_Calculator
{
    public class FactorioDataImporter
    {
        public int ImportRecipes(string scriptOutputPath, string projectRootPath)
        {
            string dataDumpPath = Path.Combine(scriptOutputPath, "data-raw-dump.json");
            string recipeLocalePath = Path.Combine(scriptOutputPath, "recipe-locale.json");
            string itemLocalePath = Path.Combine(scriptOutputPath, "item-locale.json");
            string recipeIconsPath = Path.Combine(scriptOutputPath, "recipe");
            
            string dataFolderPath = Path.Combine(projectRootPath, "Data");
            string assetsRecipesPath = Path.Combine(projectRootPath, "Assets", "Recipes");
            string outputRecipesPath = Path.Combine(dataFolderPath, "recipes.json");

            if (!File.Exists(dataDumpPath))
            {
                throw new FileNotFoundException("Не найден data-raw-dump.json", dataDumpPath);
            }

            if (!File.Exists(recipeLocalePath))
            {
                throw new FileNotFoundException("Не найден recipe-locale.json", recipeLocalePath);
            }

            if (!Directory.Exists(recipeIconsPath))
            {
                throw new DirectoryNotFoundException("Не найдена папка recipe с иконками.");
            }

            Directory.CreateDirectory(dataFolderPath);
            Directory.CreateDirectory(assetsRecipesPath);

            JObject dataRoot = LoadJsonObject(dataDumpPath);
            JObject recipeLocaleRoot = LoadJsonObject(recipeLocalePath);
            JObject itemLocaleRoot = LoadJsonObjectIfExists(itemLocalePath);

            JObject recipesObject = dataRoot["recipe"] as JObject;
            JObject itemsObject = dataRoot["item"] as JObject;
            JObject itemGroupsObject = dataRoot["item-group"] as JObject;
            JObject itemSubgroupsObject = dataRoot["item-subgroup"] as JObject;


            if (recipesObject == null)
            {
                throw new InvalidDataException("В data-raw-dump.json отсутствует раздел recipe.");
            }

            List<Recipe> importedRecipes = new List<Recipe>();

            foreach (JProperty recipeProperty in recipesObject.Properties())
            {
                Recipe recipe = CreateRecipe(
                    recipeProperty,
                    recipeLocaleRoot,
                    itemLocaleRoot,
                    itemsObject,
                    itemGroupsObject,
                    itemSubgroupsObject,
                    dataRoot
                );


                if (recipe == null)
                {
                    continue;
                }

                importedRecipes.Add(recipe);

                CopyRecipeIconIfExists(recipe.Id, recipeIconsPath, assetsRecipesPath);
            }

            string json = JsonConvert.SerializeObject(importedRecipes, Formatting.Indented);
            File.WriteAllText(outputRecipesPath, json);

            return importedRecipes.Count;
        }

        private JObject LoadJsonObject(string filePath)
        {
            string json = File.ReadAllText(filePath);
            JObject result = JsonConvert.DeserializeObject<JObject>(json);

            if (result == null)
            {
                throw new InvalidDataException("Не удалось прочитать JSON: " + filePath);
            }

            return result;
        }

        private JObject LoadJsonObjectIfExists(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return new JObject();
            }

            return LoadJsonObject(filePath);
        }

        private Recipe CreateRecipe(
            JProperty recipeProperty,
            JObject recipeLocaleRoot,
            JObject itemLocaleRoot,
            JObject itemsObject,
            JObject itemGroupsObject,
            JObject itemSubgroupsObject,
            JObject dataRoot)
        {
            JObject recipeObject = recipeProperty.Value as JObject;

            if (recipeObject == null)
            {
                return null;
            }

            JArray ingredientsArray = recipeObject["ingredients"] as JArray;
            JArray resultsArray = recipeObject["results"] as JArray;

            if (ingredientsArray == null || resultsArray == null || resultsArray.Count == 0)
            {
                return null;
            }

            JObject firstResult = resultsArray[0] as JObject;

            if (firstResult == null)
            {
                return null;
            }

            string recipeId = GetStringValue(recipeObject, "name");

            if (string.IsNullOrWhiteSpace(recipeId))
            {
                recipeId = recipeProperty.Name;
            }

            string localizedName = GetLocalizedName(recipeLocaleRoot, recipeId);

            if (string.IsNullOrWhiteSpace(localizedName))
            {
                string resultItemName = GetStringValue(firstResult, "name");

                if (!string.IsNullOrWhiteSpace(resultItemName))
                {
                    localizedName = GetLocalizedName(itemLocaleRoot, resultItemName);
                }
            }

            if (string.IsNullOrWhiteSpace(localizedName))
            {
                localizedName = recipeId;
            }

            List<string> categories = GetRecipeCategories(recipeObject);


            string mainProductId = GetMainProductId(recipeObject);
            JObject productPrototype = FindProductPrototype(mainProductId, dataRoot);

            string subgroup = GetStringValue(recipeObject, "subgroup");
            string sortOrder = GetStringValue(recipeObject, "order");

            if (productPrototype != null)
            {
                if (string.IsNullOrWhiteSpace(subgroup))
                {
                    subgroup = GetStringValue(productPrototype, "subgroup");
                }

                if (string.IsNullOrWhiteSpace(sortOrder))
                {
                    sortOrder = GetStringValue(productPrototype, "order");
                }
            }

            if (string.IsNullOrWhiteSpace(subgroup))
            {
                subgroup = "other";
            }

            if (string.IsNullOrWhiteSpace(sortOrder))
            {
                sortOrder = recipeId;
            }

            string subgroupOrder = string.Empty;

            JObject subgroupsObject = dataRoot["item-subgroup"] as JObject;

            if (subgroupsObject != null)
            {
                JObject subgroupObject = subgroupsObject[subgroup] as JObject;

                if (subgroupObject != null)
                {
                    subgroupOrder = GetStringValue(subgroupObject, "order");
                }
            }


            double craftTime = GetDoubleValue(recipeObject, "energy_required", 0.5);
            double outputAmount = GetDoubleValue(firstResult, "amount", 1);

            string group = "other";

            if (subgroupsObject != null)
            {
                JObject subgroupObject = subgroupsObject[subgroup] as JObject;

                if (subgroupObject != null)
                {
                    string detectedGroup = GetStringValue(subgroupObject, "group");

                    if (!string.IsNullOrWhiteSpace(detectedGroup))
                    {
                        group = detectedGroup;
                    }
                }
            }

            Recipe recipe = new Recipe();

            recipe.Id = recipeId;
            recipe.Name = localizedName;
            recipe.Group = group;
            recipe.Categories = categories;
            recipe.Category = categories[0];
            recipe.IconPath = "/Assets/Recipes/" + recipeId + ".png";
            recipe.CraftTime = craftTime;
            recipe.OutputAmount = outputAmount;
            recipe.AllowProductivity = recipeObject.Value<bool?>("allow_productivity") == true;
            recipe.Subgroup = subgroup;
            recipe.SubgroupOrder = subgroupOrder;
            recipe.SortOrder = sortOrder;           


            foreach (JToken ingredientToken in ingredientsArray)
            {
                JObject ingredientObject = ingredientToken as JObject;

                if (ingredientObject == null)
                {
                    continue;
                }

                string ingredientId = GetStringValue(ingredientObject, "name");
                string ingredientName = GetLocalizedName(itemLocaleRoot, ingredientId);

                if (string.IsNullOrWhiteSpace(ingredientName))
                {
                    ingredientName = ingredientId;
                }

                double ingredientAmount = GetDoubleValue(ingredientObject, "amount", 0);

                if (ingredientAmount <= 0)
                {
                    continue;
                }

                Ingredient ingredient = new Ingredient();
                ingredient.Name = ingredientName;
                ingredient.Amount = ingredientAmount;

                recipe.Ingredients.Add(ingredient);
            }

            if (recipe.Ingredients.Count == 0)
            {
                return null;
            }

            return recipe;
        }

        private JObject FindProductPrototype(string productId, JObject dataRoot)
        {
            if (string.IsNullOrWhiteSpace(productId))
            {
                return null;
            }

            string[] prototypeTypes =
            {
        "item",
        "module",
        "repair-tool",
        "tool",
        "ammo",
        "gun",
        "armor",
        "capsule",
        "item-with-entity-data",
        "rail-planner",
        "fluid"
    };

            foreach (string prototypeType in prototypeTypes)
            {
                JObject prototypes = dataRoot[prototypeType] as JObject;

                if (prototypes == null)
                {
                    continue;
                }

                JObject product = prototypes[productId] as JObject;

                if (product != null)
                {
                    return product;
                }
            }

            return null;
        }

        private string GetMainProductId(JObject recipeObject)
        {
            string mainProduct = GetStringValue(recipeObject, "main_product");

            if (!string.IsNullOrWhiteSpace(mainProduct))
            {
                return mainProduct;
            }

            JArray results = recipeObject["results"] as JArray;

            if (results != null && results.Count > 0)
            {
                JObject firstResult = results[0] as JObject;

                if (firstResult != null)
                {
                    return GetStringValue(firstResult, "name");
                }
            }

            return string.Empty;
        }


        private List<string> GetRecipeCategories(JObject recipeObject)
        {
            List<string> categories = new List<string>();

            JArray categoriesArray = recipeObject["categories"] as JArray;

            if (categoriesArray != null)
            {
                foreach (JToken categoryToken in categoriesArray)
                {
                    string category = categoryToken.ToString();

                    if (!string.IsNullOrWhiteSpace(category) &&
                        !categories.Contains(category))
                    {
                        categories.Add(category);
                    }
                }
            }

            if (categories.Count == 0)
            {
                string category = GetStringValue(recipeObject, "category");

                if (string.IsNullOrWhiteSpace(category))
                {
                    category = RecipeCategory.Crafting;
                }

                categories.Add(category);
            }

            return categories;
        }

        private string GetLocalizedName(JObject localeRoot, string id)
        {
            if (localeRoot == null || string.IsNullOrWhiteSpace(id))
            {
                return string.Empty;
            }

            JToken token = localeRoot[id];

            if (token == null)
            {
                return string.Empty;
            }

            return token.ToString().Trim();
        }

        private string GetFirstCategory(JObject recipeObject)
        {
            JArray categoriesArray = recipeObject["categories"] as JArray;

            if (categoriesArray != null && categoriesArray.Count > 0)
            {
                JToken firstToken = categoriesArray[0];

                if (firstToken != null)
                {
                    return firstToken.ToString();
                }
            }

            string category = GetStringValue(recipeObject, "category");

            if (!string.IsNullOrWhiteSpace(category))
            {
                return category;
            }

            return RecipeCategory.Crafting;
        }


        private string DetectGroup(JObject recipeObject, JObject dataRoot)
        {
            JObject subgroups = dataRoot["item-subgroup"] as JObject;

            if (subgroups == null)
            {
                return "other";
            }

            string subgroupId = GetStringValue(recipeObject, "subgroup");

            if (string.IsNullOrWhiteSpace(subgroupId))
            {
                string mainProduct = GetStringValue(recipeObject, "main_product");

                if (!string.IsNullOrWhiteSpace(mainProduct))
                {
                    subgroupId = FindProductSubgroup(mainProduct, dataRoot);
                }
            }

            if (string.IsNullOrWhiteSpace(subgroupId))
            {
                JArray results = recipeObject["results"] as JArray;

                if (results != null && results.Count > 0)
                {
                    JObject firstResult = results[0] as JObject;

                    if (firstResult != null)
                    {
                        string productId = GetStringValue(firstResult, "name");
                        subgroupId = FindProductSubgroup(productId, dataRoot);
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(subgroupId))
            {
                return "other";
            }

            JObject subgroup = subgroups[subgroupId] as JObject;

            if (subgroup == null)
            {
                return "other";
            }

            string groupId = GetStringValue(subgroup, "group");

            if (string.IsNullOrWhiteSpace(groupId))
            {
                return "other";
            }

            return groupId;
        }


        private string GetStringValue(JObject obj, string propertyName)
        {
            if (obj == null)
            {
                return string.Empty;
            }

            JToken token = obj[propertyName];

            if (token == null)
            {
                return string.Empty;
            }

            return token.ToString();
        }

        private double GetDoubleValue(JObject obj, string propertyName, double defaultValue)
        {
            if (obj == null)
            {
                return defaultValue;
            }

            JToken token = obj[propertyName];

            if (token == null)
            {
                return defaultValue;
            }

            double value;

            if (double.TryParse(token.ToString(), out value))
            {
                return value;
            }

            return defaultValue;
        }

        private string FindProductSubgroup(string productId, JObject dataRoot)
        {
            if (string.IsNullOrWhiteSpace(productId))
            {
                return string.Empty;
            }

            foreach (JProperty prototypeType in dataRoot.Properties())
            {
                if (prototypeType.Name == "recipe")
                {
                    continue;
                }

                JObject prototypes = prototypeType.Value as JObject;

                if (prototypes == null)
                {
                    continue;
                }

                JObject product = prototypes[productId] as JObject;

                if (product == null)
                {
                    continue;
                }

                string subgroup = GetStringValue(product, "subgroup");

                if (!string.IsNullOrWhiteSpace(subgroup))
                {
                    return subgroup;
                }
            }

            return string.Empty;
        }

        private void CopyRecipeIconIfExists(string recipeId, string sourceFolder, string destinationFolder)
        {
            string sourcePath = Path.Combine(sourceFolder, recipeId + ".png");
            string destinationPath = Path.Combine(destinationFolder, recipeId + ".png");

            if (!File.Exists(sourcePath))
            {
                return;
            }

            File.Copy(sourcePath, destinationPath, true);
        }
    }
}