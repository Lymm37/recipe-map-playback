using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.Runtime.Serialization;
//using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

namespace Lymm37.PotionCraft.RecipeMapPlayback
{
    class DisplayPath
    {
        // Keeps track of a list of precise positions along the path
        // Has to account for ingredient paths, water, and whirlpools/heating
        // And also void salt...
        // Easy but expensive way to keep track of this would be a list of points with some given spacing, plus control points
        // Control points being places where ingredients start/end, where water is added/stops being added, and where heating happens

        public SerializedRecipeMark lastNonHeatingMark;
        public ControlPoint lastControlPoint;
        public int lastNumMarks;
        
        private List<ControlPoint> points;
        public static float thresholdDistance = 0.05f; // Could be adjusted...
        // Apparently points like this are already stored internally! Except not.
        public List<Potion.UsedComponent> usedComponents;
        public float price;
        public float stress;

        public DisplayPath()
        {
            points = new List<ControlPoint>();
            usedComponents = new List<Potion.UsedComponent>();
            price = 0;
            stress = 0f;
            lastNonHeatingMark = null;
            lastControlPoint = null;
            lastNumMarks = 0;
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

        public void TryToAdd(Vector2 point, SerializedRecipeMark mark, int numMarks, float health, float rotation, float teleportStatus)
        {
            if (lastControlPoint is null)
            {
                // It's the first point. Go ahead and add it.
                ControlPoint newPoint = new ControlPoint(point, mark, health, rotation, teleportStatus);
                AddPoint(newPoint);
                lastNonHeatingMark = mark.Clone();
                lastControlPoint = GetPoint(points.Count - 1);
                lastNumMarks = numMarks;
                Debug.Log("[Recipe Map Playback] Adding point of new type " + newPoint.GetName() + " with value " + mark.floatValue);
            }
            else
            {
                if (mark is not null)
                {
                    if (numMarks != lastNumMarks || mark.type != lastNonHeatingMark.type || mark.stringValue != lastNonHeatingMark.stringValue)
                    {
                        // Mark type/name is different, so add the point even if it's close to the last one
                        // Update
                        //AddPoint()
                        ControlPoint newPoint = new ControlPoint(point, mark, health, rotation, teleportStatus);
                        AddPoint(newPoint);
                        lastNonHeatingMark = mark.Clone();
                        lastControlPoint = GetPoint(points.Count - 1);
                        lastNumMarks = numMarks;
                        Debug.Log("[Recipe Map Playback] Adding point of new type "+newPoint.GetName()+" with value " + mark.floatValue);
                    }
                    else
                    {
                        // Update if the floatValue OR health has changed
                        if (mark.floatValue != lastNonHeatingMark.floatValue || health != lastControlPoint.health)
                        {
                            // Add the point if it's not too close
                            if ((point - lastControlPoint.GetPoint()).magnitude > thresholdDistance)
                            {
                                ControlPoint newPoint = new ControlPoint(point, mark, health, rotation, teleportStatus);
                                AddPoint(newPoint);
                                lastNonHeatingMark = mark.Clone();
                                lastControlPoint = GetPoint(points.Count - 1);
                                lastNumMarks = numMarks;
                                Debug.Log("[Recipe Map Playback] Adding point of same type with value " + mark.floatValue);
                            }
                            else
                            {
                                // Update value if it didn't move
                                GetPoint(points.Count - 1).value = mark.floatValue;
                                GetPoint(points.Count - 1).health = health;
                                GetPoint(points.Count - 1).rotation = rotation;
                                GetPoint(points.Count - 1).teleportStatus = teleportStatus;
                                lastNonHeatingMark = mark.Clone();
                                lastControlPoint = GetPoint(points.Count - 1);
                                lastNumMarks = numMarks;
                                // This updates a lot, don't bother displaying it
                                //Debug.Log("[Recipe Map Playback] Updating float value to "+mark.floatValue);
                            }
                        }
                        else
                        {
                            // No change in floatValue. However, heating is not recorded. So if the position is different *at all*, then the potion must have been heated!
                            if ((point - lastControlPoint.GetPoint()).magnitude > thresholdDistance)
                            {
                                SerializedRecipeMark newMark = new SerializedRecipeMark();
                                newMark.floatValue = 0.0f;
                                newMark.type = SerializedRecipeMark.Type.Bellows;
                                newMark.stringValue = "Heat";
                                // Just going to leave note null for now
                                // Updates to display teleporting instead of heat, but mark is still bellows... eh
                                ControlPoint newPoint = new ControlPoint(point, newMark, health, rotation, teleportStatus);
                                AddPoint(newPoint);
                                // Don't update lastNonHeatingMark
                                lastControlPoint = GetPoint(points.Count - 1);
                                lastNumMarks = numMarks;
                                Debug.Log("[Recipe Map Playback] Adding secret heating/teleport point at position ("+point.x+", "+point.y+")");
                            }
                        }
                    }

                }
            }
            
        }

        /*
        public void TryToAdd(ControlPoint p)
        {
            ControlPoint q = GetPoint(points.Count - 1);
            float dist = p.Distance(q);
            if (dist > thresholdDistance || !p.GetType().Equals(q.GetType()))
            {
                AddPoint(p);
            }
        }
        */

        public void Save(string name)
        {
            if (name != "") {
                if (points.Count > 0)
                {
                    /*
                    IFormatter formatter = new BinaryFormatter();
                    Stream stream = new FileStream(name + ".dat", FileMode.Create, FileAccess.Write);
                    formatter.Serialize(stream, points);
                    stream.Close();
                    */
                    string pathJson = JsonConvert.SerializeObject(points);
                    try
                    {
                        File.WriteAllText("recipe_path_"+name + ".json", pathJson);
                        Debug.Log($"[Recipe Map Playback] Display path with {points.Count} points written to {name}.json");
                    }
                    catch (Exception ex)
                    {
                        Debug.Log("[Recipe Map Playback] Error saving file");
                        Debug.Log(ex.StackTrace);
                    }
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

        public DisplayPath Clone()
        {
            /*public SerializedRecipeMark lastNonHeatingMark;
        public ControlPoint lastControlPoint;
        public int lastNumMarks;
        
        private List<ControlPoint> points;
        public static float thresholdDistance = 0.05f; // Could be adjusted...
        // Apparently points like this are already stored internally! Except not.
        public List<Potion.UsedComponent> usedComponents;
        public float price;
        public float stress;
            */
            DisplayPath newPath = new DisplayPath();
            newPath.lastNonHeatingMark = lastNonHeatingMark.Clone();
            newPath.lastControlPoint = lastControlPoint.Clone();
            newPath.lastNumMarks = lastNumMarks;
            newPath.points = new List<ControlPoint>();
            foreach (ControlPoint p in points)
            {
                newPath.points.Add(p.Clone());
            }
            newPath.usedComponents = new List<Potion.UsedComponent>();
            foreach (Potion.UsedComponent c in usedComponents)
            {
                newPath.usedComponents.Add(c.Clone());
            }
            newPath.price = price;
            newPath.stress = stress;
            return newPath;
        }

        /*
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
        */

        }


    }
