using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;

namespace Lymm37.PotionCraft.RecipeMapPlayback
{
    class DisplayPath
    {
        // Keeps track of a list of precise positions along the path
        // Has to account for ingredient paths, water, and whirlpools/heating
        // And also void salt...
        // Easy but expensive way to keep track of this would be a list of points with some given spacing, plus control points
        // Control points being places where ingredients start/end, where water is added/stops being added, and where heating happens

        public string name;
        private List<ControlPoint> points;
        public static float thresholdDistance = 0.05f; // Could be adjusted...
        // Apparently points like this are already stored internally!
        public List<Potion.UsedComponent> usedComponents;
        public float price;
        public float stress;

        public DisplayPath()
        {
            name = ""; // Needs to be set later
            points = new List<ControlPoint>();
            usedComponents = new List<Potion.UsedComponent>();
            price = 0;
            stress = 0f;
        }

        public void CalculatePrice()
        {
            price = 0f;
            foreach (Potion.UsedComponent c in usedComponents)
            {
                if (c.componentType == Potion.UsedComponent.ComponentType.InventoryItem)
                {
                    price += 4*c.amount*((InventoryItem)c.componentObject).GetPrice();
                }
            }
        }

        public void CalculateStress()
        {
            stress = 0f;
            foreach (Potion.UsedComponent c in usedComponents)
            {
                stress += (float)Math.Pow(c.amount, 2);
            }
            stress = (float)Math.Sqrt(stress);
        }

        public float GetPrice()
        {
            return price;
        }

        public float GetStress()
        {
            return stress;
        }

        public void AddPoint(ControlPoint p)
        {
            points.Add(p);
        }

        public ControlPoint GetPoint(int i)
        {
            return points[i];
        }

        public int GetLength()
        {
            return points.Count;
        }

        public void TryToAdd(ControlPoint p)
        {
            ControlPoint q = GetPoint(points.Count - 1);
            float dist = p.Distance(q);
            if (dist > thresholdDistance || !p.GetType().Equals(q.GetType()))
            {
                AddPoint(p);
            }
        }

        public string GetName()
        {
            return name;
        }

        public void SetName(string n)
        {
            name = n;
        }

        public void Save()
        {
            if (name != "") {
                if (points.Count > 0)
                {
                    IFormatter formatter = new BinaryFormatter();
                    Stream stream = new FileStream(name + ".dat", FileMode.Create, FileAccess.Write);
                    formatter.Serialize(stream, points);
                    stream.Close();
                    Debug.Log($"[Recipe Map Playback] Display path with {points.Count} points written to {name}.dat");
                }
                else
                {
                    Debug.Log("[Recipe Map Playback] Path has no points; failed to save");
                }
            }
            else
            {
                Debug.Log("[Recipe Map Playback] No name for display path; failed to save");
            }
        }

        public static DisplayPath Load(string fileName)
        {
            DisplayPath dp = new DisplayPath();
            if (fileName is not null) {
                dp.name = fileName;
                if (!dp.GetName().Equals(""))
                {
                    if (File.Exists(dp.GetName() + ".dat"))
                    {
                        IFormatter formatter = new BinaryFormatter();
                        Stream stream = new FileStream(dp.GetName() + ".dat", FileMode.Open, FileAccess.Read);
                        dp.points = (List<ControlPoint>)formatter.Deserialize(stream);
                        stream.Close(); // ?
                        Debug.Log($"[Recipe Map Playback] Display path with {dp.GetLength()} points loaded from {dp.GetName()}.dat");
                    }
                    else
                    {
                        Debug.Log($"[Recipe Map Playback] Path file for recipe {dp.GetName()} does not exist; failed to load");
                    }
                }
                else
                {
                    Debug.Log("[Recipe Map Playback] No name for display path; failed to load");
                }
            }
            else
            {
                Debug.Log("[Recipe Map Playback] Bruh you can't just pass a null filename");
            }
            return dp;
        }

    }

    
}
