using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using UnityEngine;
using TMPro;

namespace Lymm37.PotionCraft.RecipeMapPlayback
{
    static class DisplayPathManager
    {
        public static DisplayPath currentPath;
        public static DisplayPath displayedPath; // Not used yet, but was planned for if the recipes would be displayed in-game
        public static bool inDisplayMode = false;
        public static TextMeshPro tmpObj;
        public static GameObject textHolder;
        public static bool isInitialized = false;
        public static DisplayPath quicksavePath;

        public static void Initialize()
        {
            currentPath = new DisplayPath();
            quicksavePath = null;
            displayedPath = null;
            inDisplayMode = false;
            //priceText = new TextMeshPro();
            //stressText = new TextMeshPro();
            textHolder = new GameObject();
            textHolder.name = "RecipeCostTextHolder";
            textHolder.transform.Translate(-9.14f, 3.3f, 0.0f);
            textHolder.layer = 5;
            tmpObj = textHolder.AddComponent<TextMeshPro>();
            tmpObj.alignment = TextAlignmentOptions.Center;
            tmpObj.enableAutoSizing = true;
            tmpObj.sortingLayerID = -1650695527;
            tmpObj.sortingOrder = 100;
            tmpObj.fontSize = 4;
            tmpObj.fontSizeMin = 4;
            tmpObj.fontSizeMax = 4;

            tmpObj.color = Color.black;
            GameObject panel = GameObject.Find("PotionCraftPanel");
            if (panel is not null)
            {
                textHolder.transform.SetParent(panel.transform);
            }
            else
            {
                Debug.Log("[Recipe Map Playback] You really messed up with the positioning here...");
            }
            isInitialized = true;
        }

        public static void ResetCurrentPath()
        {
            currentPath = new DisplayPath();
        }

        public static void SaveFailedRecipe()
        {
            SavePath("failed_potion", 1);

        }

        public static void UpdateCurrentPath()
        {
            if (isInitialized && currentPath is not null)
            {
                Vector3 pos = Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition;
                Vector2 point = new Vector2(pos.x, pos.y);
                int numMarks = Managers.Potion.recipeMarks.GetMarksList().Count;
                SerializedRecipeMark mark = Managers.Potion.recipeMarks.GetMarksList()[numMarks - 1];

                float health = 1.0f;
                float rotation = 0.0f;
                float teleportStatus = 0.0f;
                float whirlpoolStatus = 0.0f;

                IndicatorMapItem imi = Managers.RecipeMap.indicator;
                if (imi is not null) {
                    health = ReflectionHelper.GetPrivateField<float>(imi, "health");
                    teleportStatus = imi.previousTeleportationStatus;
                }
                RecipeMapManager.IndicatorRotationSubManager irsm = Managers.RecipeMap.indicatorRotation;
                if (irsm is not null) {
                    rotation = irsm.Value;
                }
                ObjectBased.RecipeMap.RecipeMapItem.VortexMapItem.VortexMapItem vortex = Managers.RecipeMap.currentVortexMapItem;
                Coals.BellowsCoals coals = Managers.Ingredient.coals;
                if (vortex is not null && coals is not null)
                {
                    whirlpoolStatus = coals.Heat;
                }

                float saltMovement = Managers.RecipeMap.moveToNearestEffectBySalt;

                currentPath.TryToAdd(point, mark, numMarks, health, rotation, teleportStatus, whirlpoolStatus, saltMovement);
                PotionEffect[] potionEffects = Managers.Potion.collectedPotionEffects;
                if (potionEffects is not null)
                {
                    currentPath.effects = new List<string>();
                    foreach (PotionEffect pe in potionEffects)
                    {
                        if (pe is not null)
                        {
                            currentPath.effects.Add(pe.name);
                        }
                    }
                }

                currentPath.usedComponents = Managers.Potion.usedComponents;
                currentPath.CalculatePrice();
                currentPath.CalculateStress();
                if (textHolder is not null && textHolder.transform is not null)
                {
                    GameObject panel = Managers.Potion.potionCraftPanel.gameObject;
                    //GameObject panel = GameObject.Find("PotionCraftPanel");
                    if (panel is not null && panel.transform is not null)
                    {
                        textHolder.transform.SetParent(panel.transform);
                        textHolder.SetActive(true);
                    }
                }
                //priceText.rectTransform.rect = new Rect(new Vector2(0f, 0f), new Vector2(100f, 100f));
                tmpObj.text = currentPath.GetPrice().ToString() + " / " + currentPath.GetStress().ToString() + " / " + health;

            }
        }

        public static void SetInDisplayMode(bool b)
        {
            inDisplayMode = b;
            if (!inDisplayMode)
            {
                displayedPath = null;
            }
        }

        public static bool IsInDisplayMode()
        {
            return inDisplayMode;
        }

        // Called when saving a recipe
        public static void SavePath(string name, int failed=0)
        {
            currentPath.Save(name, failed);
            displayedPath = currentPath;
        }

        /*
        public static bool LoadPath(string name)
        {
            // Don't delete current path, just display
            displayedPath = DisplayPath.Load(name);
            return (displayedPath.GetLength() > 0);
        }
        */

        public static void Quicksave()
        {
            quicksavePath = currentPath.Clone();
            Debug.Log("[Recipe Map Playback] " + currentPath.GetLength() + " points saved.");
        }

        public static void Quickload()
        {
            if (quicksavePath != null)
            {
                currentPath = quicksavePath;
                Debug.Log("[Recipe Map Playback] " + currentPath.GetLength() + " points loaded.");
            }
        }

        public static bool Rename(string oldname, string newname)
        {
            try
            {
                if (File.Exists(oldname + ".json")) {
                    if (File.Exists(newname + ".json"))
                    {
                        File.Delete(newname + ".json");
                    }
                    File.Move(oldname + ".json", newname + ".json");
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.Log($"[Recipe Map Playback] Exception occurred when renaming: \n{ex.StackTrace}");
            }
            return false;
        }
    }
}
