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
        public float lastSaltMovement;
        
        private List<ControlPoint> points;
        public static float thresholdDistance = 0.01f; // Could be adjusted...
        public List<Potion.UsedComponent> usedComponents;
        public float stress;
        public float totalCost;
        public float noSaltCost;
        public int ingredientCount;
        public List<string> effects;

        public DisplayPath()
        {
            points = new List<ControlPoint>();
            usedComponents = new List<Potion.UsedComponent>();
            stress = 0f;
            totalCost = 0f;
            noSaltCost = 0f;
            ingredientCount = 0;
            effects = new List<string>();
            lastNonHeatingMark = null;
            lastControlPoint = null;
            lastNumMarks = 0;
            lastSaltMovement = 0;
        }

        public void CalculatePrice()
        {
            totalCost = 0f;
            noSaltCost = 0f;
            ingredientCount = 0;
            foreach (Potion.UsedComponent c in usedComponents)
            {
                if (c.componentType == Potion.UsedComponent.ComponentType.InventoryItem)
                {
                    totalCost += 4*c.amount*((InventoryItem)c.componentObject).GetPrice();
                    if (!((InventoryItem)c.componentObject).name.Contains("Salt"))
                    {
                        noSaltCost += 4 * c.amount * ((InventoryItem)c.componentObject).GetPrice();
                        ingredientCount++;
                    }
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
            return totalCost;
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

        public void TryToAdd(Vector2 point, SerializedRecipeMark mark, int numMarks, float health, float rotation, float teleportStatus, float whirlpoolStatus, float saltMovement)
        {
            float philSaltStatus = 0;
            if (saltMovement - lastSaltMovement != 0)
            {
                philSaltStatus = 1;
            }
            if (teleportStatus != 0)
            {
                teleportStatus = 1; // Covers the very start and end of teleport
            }

            if (lastControlPoint is null)
            {
                // It's the first point. Go ahead and add it.
                ControlPoint newPoint = new ControlPoint(point, mark, health, rotation, teleportStatus, whirlpoolStatus, philSaltStatus);
                AddPoint(newPoint);
                lastNonHeatingMark = mark.Clone();
                lastControlPoint = GetPoint(points.Count - 1);
                lastNumMarks = numMarks;
                lastSaltMovement = saltMovement;
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
                        ControlPoint newPoint = new ControlPoint(point, mark, health, rotation, teleportStatus, whirlpoolStatus, philSaltStatus);
                        AddPoint(newPoint);
                        lastNonHeatingMark = mark.Clone();
                        lastControlPoint = GetPoint(points.Count - 1);
                        lastNumMarks = numMarks;
                        lastSaltMovement = saltMovement;
                        Debug.Log("[Recipe Map Playback] Adding point of new type "+newPoint.GetName()+" with value " + mark.floatValue);
                    }
                    else
                    {
                        // Update if the floatValue OR health has changed (this may cause a bug if stirring in a whirlpool while losing health...)
                        if (mark.floatValue != lastNonHeatingMark.floatValue || health != lastControlPoint.health)
                        {
                            // Add the point if it's not too close
                            if ((point - lastControlPoint.GetPoint()).magnitude > thresholdDistance)
                            {
                                ControlPoint newPoint = new ControlPoint(point, mark, health, rotation, teleportStatus, whirlpoolStatus, philSaltStatus);
                                AddPoint(newPoint);
                                lastNonHeatingMark = mark.Clone();
                                lastControlPoint = GetPoint(points.Count - 1);
                                lastNumMarks = numMarks;
                                lastSaltMovement = saltMovement;
                                Debug.Log("[Recipe Map Playback] Adding point of same type with value " + mark.floatValue);
                            }
                            else
                            {
                                // Update value if it didn't move
                                GetPoint(points.Count - 1).value = mark.floatValue;
                                GetPoint(points.Count - 1).health = health;
                                GetPoint(points.Count - 1).rotation = rotation;
                                GetPoint(points.Count - 1).teleportStatus = teleportStatus;
                                GetPoint(points.Count - 1).whirlpoolStatus = whirlpoolStatus;
                                GetPoint(points.Count - 1).philSaltStatus = philSaltStatus;
                                lastNonHeatingMark = mark.Clone();
                                lastControlPoint = GetPoint(points.Count - 1);
                                lastNumMarks = numMarks;
                                lastSaltMovement = saltMovement;
                                // This updates a lot, don't bother displaying it
                                //Debug.Log("[Recipe Map Playback] Updating float value to "+mark.floatValue);
                            }
                        }
                        else
                        {
                            // No change in floatValue. However, heating is not recorded. So if the position is different, then it must have been heated or something else happened like philosopher's salt
                            if ((point - lastControlPoint.GetPoint()).magnitude > thresholdDistance)
                            {
                                SerializedRecipeMark newMark = new SerializedRecipeMark();
                                newMark.floatValue = 0.0f;
                                newMark.type = SerializedRecipeMark.Type.Bellows; // Non-nullable, so this is basically just a placeholder...
                                newMark.stringValue = "Other";
                                // Just going to leave note null for now
                                // Could display as whirlpool, teleport, etc
                                ControlPoint newPoint = new ControlPoint(point, newMark, health, rotation, teleportStatus, whirlpoolStatus, philSaltStatus);
                                AddPoint(newPoint);
                                // Don't update lastNonHeatingMark
                                lastControlPoint = GetPoint(points.Count - 1);
                                lastNumMarks = numMarks;
                                lastSaltMovement = saltMovement;
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

        public void Save(string name, int failed, bool asCSV = true)
        {
            if (name != "") {
                if (points.Count > 1)
                {
                    // Calculate metadata
                    CalculatePrice();
                    CalculateStress();
                    string ingredientsString = "";
                    string saltsString = "";
                    for (int i = 0; i < usedComponents.Count; i++)
                    {
                        Potion.UsedComponent c = usedComponents[i];
                        if (c.componentType == Potion.UsedComponent.ComponentType.InventoryItem)
                        {
                            if (((InventoryItem)c.componentObject).name.Contains("Salt"))
                            {
                                saltsString += c.amount + " " + ((InventoryItem)c.componentObject).name;
                                if (i < usedComponents.Count - 1)
                                {
                                    saltsString += ",";
                                }
                            }
                            else
                            {
                                ingredientsString += c.amount+" "+((InventoryItem)c.componentObject).name;
                                if (i < usedComponents.Count-1)
                                {
                                    ingredientsString += ",";
                                }
                            }
                        }
                    }
                    string effectsString = "";
                    for (int i = 0; i < effects.Count; i++)
                    {
                        effectsString += effects[i];
                        if (i < effects.Count - 1)
                        {
                            effectsString += ",";
                        }
                    }
                    // Challenge tags (TODO)
                    string challengeTags = "";



                    if (asCSV)
                    {
                        // As CSV
                        try
                        {  
                            StreamWriter writer = new StreamWriter("recipe_path_" + name + ".csv");
                            // Going to have to include one additional header line for metadata... So it's not *really* a CSV, but still.
                            writer.WriteLine("Metadata_start");
                            writer.WriteLine(totalCost + " " + noSaltCost + " " + ingredientCount + " " + stress + " " + failed);
                            writer.WriteLine(effectsString);
                            writer.WriteLine(ingredientsString);
                            writer.WriteLine(saltsString);
                            writer.WriteLine(challengeTags);
                            writer.WriteLine("Metadata_end");
                            writer.WriteLine("step,name,x,y,typeCode,value,health,rotation,teleport,whirlpool,philSalt");
                            for (int i = 0; i < points.Count; i++)
                            {
                                ControlPoint p = points[i];
                                writer.WriteLine(i + "," + p.name + "," + p.x + "," + p.y + "," + p.typeCode + "," + p.value + "," + p.health + "," + p.rotation + "," + p.teleportStatus + "," + p.whirlpoolStatus + "," + p.philSaltStatus);
                            }
                            writer.Close();
                        }
                        catch (Exception ex)
                        {
                            Debug.Log("[Recipe Map Playback] Error saving file");
                            Debug.Log(ex.StackTrace);
                        }
                    }
                    else
                    {
                        // As JSON
                        string pathJson = JsonConvert.SerializeObject(points);
                        try
                        {
                            File.WriteAllText("recipe_path_" + name + ".json", pathJson);
                            Debug.Log($"[Recipe Map Playback] Display path with {points.Count} points written to {name}.json");
                        }
                        catch (Exception ex)
                        {
                            Debug.Log("[Recipe Map Playback] Error saving file");
                            Debug.Log(ex.StackTrace);
                        }
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
            DisplayPath newPath = new DisplayPath();
            newPath.lastNonHeatingMark = lastNonHeatingMark.Clone();
            newPath.lastControlPoint = lastControlPoint.Clone();
            newPath.lastNumMarks = lastNumMarks;
            newPath.lastSaltMovement = lastSaltMovement;
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
            newPath.totalCost = totalCost;
            newPath.stress = stress;
            newPath.noSaltCost = noSaltCost;
            newPath.ingredientCount = ingredientCount;
            newPath.effects = new List<string>();
            foreach (string e in effects)
            {
                newPath.effects.Add(e);
            }
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
