using BepInEx;
using HarmonyLib;
using UnityEngine;

// Three issues:
// Where is the map stuff done
// Where and how are the recipes saved
// How can I change the user interface as if in a menu while still showing the map

namespace Lymm37.PotionCraft.RecipeMapPlayback
{
    [BepInPlugin("Lymm37.PotionCraft.RecipeMapPlayback", "RecipeMapPlayback", "1.0.0")]
    [BepInProcess("Potion Craft.exe")]
    public class RecipeMapPlayback : BaseUnityPlugin
    {
        private readonly Harmony harmony = new Harmony("Lymm37.PotionCraft.RecipeMapPlayback");

        void Awake()
        {
            Debug.Log("[Recipe Map Playback] Patching...");
            DisplayPathManager.Initialize();
            harmony.PatchAll();
            Debug.Log("[Recipe Map Playback] Patching complete.");
        }

        [HarmonyPatch(typeof(PotionManager))]
        [HarmonyPatch("Update")]
        class CheckUpdatePatch
        {
            static void Postfix(PotionManager __instance)
            {
                // Apparently updates like every frame
                //Debug.Log("[Recipe Map Playback] Potion manager updated");
                // Should instead only call when PotionManager.usedComponents is updated,
                DisplayPathManager.UpdateCurrentPath();
                //Debug.Log($"[Recipe Map Playback] Current price is {DisplayPathManager.currentPath.GetPrice()} and current stress is {DisplayPathManager.currentPath.GetStress()}");
            }
        }

        [HarmonyPatch(typeof(PotionManager))]
        [HarmonyPatch("ResetPotion")]
        class ResetPotionPatch
        {
            static void Prefix(PotionManager __instance)
            {
                DisplayPathManager.ResetCurrentPath();
                Debug.Log($"[Recipe Map Playback] Resetting current path");
            }
        }

        // Request update if the potion changed position on the map
        [HarmonyPatch(typeof(IndicatorMapItem))]
        [HarmonyPatch("CheckPositionChange")]
        class PositionChangePatch
        {
            static void Prefix(IndicatorMapItem __instance)
            {
                // Doing this check probably makes the distance checks in DisplayPath redundant
                if ((__instance.previousPosition - Managers.RecipeMap.recipeMapObject.indicatorContainer.localPosition).magnitude > DisplayPath.thresholdDistance)
                {
                    DisplayPathManager.SetIngredientAddedState("Move");
                    DisplayPathManager.RequestUpdate();
                }
            }
        }

        // Request update if ingredient is added
        [HarmonyPatch(typeof(ObjectBased.Stack.Stack))]
        [HarmonyPatch("AddIngredientPathToMapPath")]
        class AddIngredientPatch
        {
            static void Prefix(ObjectBased.Stack.Stack __instance)
            {
                if (__instance.Ingredient.name.Equals("FrostSapphire") && __instance.overallGrindStatus >= 0.9923f)
                {
                    // Special case for frost sapphire, where full-grind is not overallGrindStatus of 1...
                    if (System.Math.Abs(__instance.overallGrindStatus - 0.992302f) < 0.001)
                    {
                        // Full-grind
                        DisplayPathManager.SetIngredientAddedState("Full");
                    }
                    else
                    {
                        // Cheating
                        DisplayPathManager.SetIngredientAddedState("Cheated");
                    }
                }
                else
                {
                    if (__instance.overallGrindStatus == 1f)
                    {
                        // Full-grind
                        DisplayPathManager.SetIngredientAddedState("Full");
                    }
                    else if (__instance.overallGrindStatus == 0f)
                    {
                        // No-grind
                        DisplayPathManager.SetIngredientAddedState("None");
                    }
                    else
                    {
                        // Maybe cracked?
                        bool isGround = false;
                        foreach (StackItem stackItem in __instance.itemsFromThisStack)
                        {
                            if (stackItem is IngredientFromStack ingredientFromStack1)
                            {
                                if (ingredientFromStack1.currentGrindState != 1)
                                {
                                    isGround = true;
                                }
                            }
                            else if (stackItem is GrindedSubstanceInPlay)
                            {
                                isGround = true;
                            }
                        }
                        if (!isGround)
                        {
                            // Cracked
                            DisplayPathManager.SetIngredientAddedState("Cracked");
                        }
                        else
                        {
                            // Other grind
                            DisplayPathManager.SetIngredientAddedState("Other");
                        }
                    }
                }
                DisplayPathManager.RequestUpdate();
            }
        }

        // Request update if salt is added
        [HarmonyPatch(typeof(Salt))]
        [HarmonyPatch("OnCauldronDissolve")]
        class AddSaltPatch
        {
            static void Prefix(Salt __instance)
            {
                DisplayPathManager.SetIngredientAddedState("Salt");
                DisplayPathManager.RequestUpdate();
            }
        }

        [HarmonyPatch(typeof(IndicatorMapItem))]
        [HarmonyPatch("OnIndicatorRuined")]
        class FailedRecipePatch
        {
            static void Prefix()
            {
                DisplayPathManager.SaveFailedRecipe();
                Debug.Log($"[Recipe Map Playback] Saving failed recipe");
            }
        }

        [HarmonyPatch(typeof(IngredientFromStack))]
        [HarmonyPatch("Smash")]
        class SmashPatch
        {
            static void Postfix()
            {
                //DisplayPathManager.cracked = true;
                // Not sure how to do this. It's possible to crack an ingredient and then put it on the ground and throw in a different uncracked/ground ingredient,
                // so just seeing if it has been cracked is really not good enough.
                Debug.Log($"[Recipe Map Playback] Ingredient cracked");
            }
        }

        [HarmonyPatch(typeof(PotionCraftPanel.SaveRecipeButton))]
        [HarmonyPatch("OnButtonReleasedPointerInside")]
        class SaveRecipePatch
        {
            static void Prefix(PotionManager __instance)
            {
                // Apparently updates like every frame
                //Debug.Log("[Recipe Map Playback] Potion manager updated");
                // Should instead only call when PotionManager.usedComponents is updated,
                string name = Managers.Potion.potionCraftPanel.GetCurrentPotion().name; // Kind of a placeholder
                Debug.Log($"[Recipe Map Playback] Saving path...");
                DisplayPathManager.SavePath(name);
            }
        }

        // Quicksave and quickload patches

        [HarmonyPatch(typeof(SaveLoadManager))]
        [HarmonyPatch("SaveToSlot")]
        class QuicksavePatch
        {
            static void Postfix(ref SaveLoadSystem.SaveSlotIndex slot)
            {
                if (slot == SaveLoadSystem.SaveSlotIndex.Quicksave)
                {
                    Debug.Log($"[Recipe Map Playback] Quicksaving...");
                    DisplayPathManager.Quicksave();
                }
            }
        }

        [HarmonyPatch(typeof(SaveLoadManager))]
        [HarmonyPatch("LoadFromSlot")]
        class QuickloadPatch
        {
            static void Postfix(ref SaveLoadSystem.SaveSlotIndex slot)
            {
                if (slot == SaveLoadSystem.SaveSlotIndex.Quicksave)
                {
                    Debug.Log($"[Recipe Map Playback] Quickloading...");
                    DisplayPathManager.Quickload();
                }
            }
        }

        /*
        [HarmonyPatch(typeof(IndicatorMapItem))]
        [HarmonyPatch("UpdateHealth")]
        class UpdateHealthPatch
        {
            static void Postfix(IndicatorMapItem __instance)
            {
                ReflectionHelper.GetInternalField<float>(__instance, "health");
            }
        }
        */

        /*
        // Resetting: Happens when current potion is finished
        [HarmonyPatch(typeof())]
        [HarmonyPatch("")]
        class ResetPatch
        {
            static void Postfix()
            {
                DisplayPathManager.resetCurrentPath();
            }
        }

        // Update Name: Happens when current potion is renamed
        [HarmonyPatch(typeof())]
        [HarmonyPatch("")]
        class UpdateNamePatch
        {
            // TODO: Obviously rename n
            static void Postfix(ref string ___n)
            {
                DisplayPathManager.currentPath.SetName(___n);
            }
        }

        // Update Name: Happens when saved potion is renamed
        [HarmonyPatch(typeof())]
        [HarmonyPatch("")]
        class UpdateNameAltPatch
        {
            // TODO: Obviously rename n and m
            static void Prefix(ref string ___n, ref string ___m)
            {
                DisplayPathManager.Rename(___n, ___m);
            }
        }

        // Extend the path: Happens *a lot* ... hmm
        [HarmonyPatch(typeof())]
        [HarmonyPatch("")]
        class ExtensionPatch
        {
            // TODO: Obviously rename v
            static void Postfix(ref Vector2 ___v)
            {
                string controlType = "start";
                DisplayPathManager.TryToAddPoint(___v, controlType);
            }
        }

        // Draw the path, also suppress other events? That seems like it may require many separate patches though.
        // Happens around drawing time? Idk
        [HarmonyPatch(typeof())]
        [HarmonyPatch("")]
        class DisplayPatch
        {
            static void Postfix()
            {
                if (DisplayPathManager.IsInDisplayMode()) {
                    Debug.Log("[Recipe Map Playback] Drawing...");
                    DisplayPath displayPath = DisplayPathManager.displayedPath;
                    for (int i = 0; i < displayPath.GetLength(); i++)
                    {
                        ControlPoint point = displayPath.GetPoint(i);
                        // TODO: Something
                    }
                }
            }
        }

        //PotionManager.ResetPotion
        //PotionManager.RecipeMarksSubManager

        // TODO: Probably needs debounce or different keys

        // Allow to enter display mode: Happens while in recipe book on a non-blank recipe for which there is saved data and user presses tab

        //private method: recipeBook.IsEmptyPage(recipeBook.currentPotionRecipeIndex)
        //recipeBook.savedRecipes[recipeBook.currentPotionRecipeIndex]
        //potion.GetSerializedPotion() // then usedcomponentstypes for example
        //SerializedPotionFromPanel has the path and the list of recipe marks though...
        //potion.potionFromPanel;
        //recipeBook.savedRecipes[recipeBook.currentPotionRecipeIndex].potionFromPanel
        // Check for null?
        [HarmonyPatch(typeof())]
        [HarmonyPatch("")]
        class EnterDisplayModePatch
        {
            static void Postfix(ref string ___n)
            {
                if (Input.GetKey(KeyCode.Tab))
                {
                    // Attempt to load the recipe
                    if (DisplayPathManager.LoadPath(___n))
                    {
                        DisplayPathManager.SetInDisplayMode(true);
                    }
                }
            }
        }

        // Allow to exit display mode: Happens ... sometime that is checked often
        [HarmonyPatch(typeof())]
        [HarmonyPatch("")]
        class ExitDisplayModePatch
        {
            static void Postfix()
            {
                if (Input.GetKey(KeyCode.Tab))
                {
                    DisplayPathManager.SetInDisplayMode(false);
                }
            }
        }

        // Save the path data: Happens when the current potion is saved to the recipe book
        [HarmonyPatch(typeof())]
        [HarmonyPatch("")]
        class SavePatch
        {
            static void Postfix()
            {
                DisplayPathManager.SavePath();
            }
        }

        // Add ingredient: Happens when an ingredient is added to the cauldron (NOT when it is just taken out of the inventory)
        [HarmonyPatch(typeof())]
        [HarmonyPatch("")]
        class AddIngredientPatch
        {
            static void Postfix(ref Ingredient ___ing)
            {
                DisplayPathManager.currentPath.AddIngredient(___ing);
            }
        }
        */
    }
}
