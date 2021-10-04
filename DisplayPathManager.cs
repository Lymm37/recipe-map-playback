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
        public static DisplayPath displayedPath;
        public static bool inDisplayMode = false;
        public static TextMeshPro tmpObj;
        public static GameObject textHolder;
        
        public static void Initialize() 
        {
            currentPath = new DisplayPath();
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
        }

        public static void ResetCurrentPath()
        {
            currentPath = new DisplayPath();
        }

        public static void UpdateCurrentPath()
        {
            currentPath.usedComponents = Managers.Potion.usedComponents;
            currentPath.CalculatePrice();
            currentPath.CalculateStress();

            
            textHolder.SetActive(true);
            
            
            //priceText.rectTransform.rect = new Rect(new Vector2(0f, 0f), new Vector2(100f, 100f));
            tmpObj.text = currentPath.GetPrice().ToString()+" / "+ currentPath.GetStress().ToString(); ;
        }

        public static void TryToAddPoint(Vector2 pt, string state)
        {
            ControlPoint point = new ControlPoint(pt, state);
            currentPath.TryToAdd(point);
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
        public static void SavePath()
        {
            currentPath.Save();
            displayedPath = currentPath;
        }

        public static bool LoadPath(string name)
        {
            // Don't delete current path, just display
            displayedPath = DisplayPath.Load(name);
            return (displayedPath.GetLength() > 0);
        }

        public static bool Rename(string oldname, string newname)
        {
            try
            {
                if (File.Exists(oldname + ".dat")) {
                    if (File.Exists(newname + ".dat"))
                    {
                        File.Delete(newname + ".dat");
                    }
                    File.Move(oldname + ".dat", newname + ".dat");
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
