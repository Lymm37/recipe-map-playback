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
        // Lazy by doing more work: I couldn't find an easy way to get the localization strings for English specifically, because I'd have to figure out how to change the locale and back... Downside is I'll have to add to this when new ingredients are added.
        // These have defaults in case people make custom ingredients or effects, though the categories for ingredients would get messed up
        static Dictionary<string, string> ingredientTranslation = new Dictionary<string, string> { { "Leaf", "Terraria" }, { "Waterbloom", "Waterbloom" }, { "GoblinMushroom", "Goblin Shroom" }, { "Firebell", "Firebell" }, {"DryadMushroom", "Dryad's Saddle" }, { "Windbloom", "Windbloom"}, { "CliffFungus", "Brown Mushroom"}, { "Marshrooms", "Marshroom"}, {"Thornstick", "Thornstick" }, { "Wierdshroom", "Weirdshroom"}, {"Tangleweeds","Tangleweed" }, { "Thistle", "Thunder Thistle"}, { "LumpyBeet", "Lumpy Beet"}, {"IceDragonfruit", "Ice Fruit" }, {"GreenMushroom", "Green Mushroom" }, {"RedMushroom", "Red Mushroom" }, { "GreyChanterelle", "Shadow Chanterelle"}, { "GraveTruffle", "Grave Truffle"}, {"SulphurShelf", "Sulphur Shelf" }, {"LavaRoot", "Lava Root" }, {"Refruit", "Hairy Banana" }, {"Goldthorn", "Goldthorn" }, {"WitchMushroom", "Witch Mushroom" }, { "BloodyRoot", "Bloodthorn"}, {"EarthCrystal", "Earth Pyrite" }, {"FrostSapphire", "Frost Sapphire" }, { "FireCitrine", "Fire Citrine"}, {"Crystal", "Cloud Crystal" }, {"BloodRuby", "Blood Ruby" }, {"Void Salt", "Void Salt"}, {"Moon Salt", "Moon Salt" }, { "Sun Salt", "Sun Salt" }, { "Life Salt","Life Salt" }, {"Philosopher's Salt", "Philosopher's Salt" } };
        static Dictionary<string, string> effectTranslation = new Dictionary<string, string> { {"Crop","Rich Harvest"},{ "Invisibility", "Invisibility" },{ "StoneSkin", "Stone Skin" },{ "Growth", "Fast Growth" }, { "SlowDown", "Slow Down" }, { "Sleep", "Sleep" }, { "SharpVision", "Magical Vision" }, { "Mana", "Mana" }, { "Lightning", "Lightning" }, { "Hallucinations", "Hallucinations" }, { "Fly", "Levitation" }, { "Explosion", "Explosion" }, { "Charm", "Charm" }, { "Berserker", "Berserker" }, { "Light", "Light" }, { "Libido", "Libido" }, { "Bounce", "Bounce" }, { "Acid", "Acid" }, { "Fire", "Fire" }, { "Necromancy", "Necromancy" },{ "Frost", "Frost" }, { "Poison", "Poisoning" }, { "Healing", "Healing" } };
        // Alchemy machine recipes
        static List<string> nigredoPotions = new List<string> { "Healing 3", "Hallucinations 2,Mana 2", "Acid 3,Poisoning 2", "Fast Growth 1,Stone Skin 1, Sleep 1", "Fire 1,Frost 1,Light 1,Lightning 1" };
        static List<string> voidSaltPotions = new List<string> { "Necromancy 3", "Mana 3", "Acid 2,Poisoning 3", "Frost 3,Sleep 2", "Explosion 2,Fire 1,Lightning 2" };
        static List<string> albedoPotions = new List<string> { "Necromancy 3", "Light 3", "Fast Growth 1,Slow Down 1,Stone Skin 1", "Slow Down 2,Stone Skin 2", "Explosion 1,Fire 1,Light 1", "Bounce 2,Invisibility 2", "Bounce 1,Invisibility 1,Mana 1", "Charm 2,Levitation 2,Lightning 1", "Hallucinations 2,Magical Vision 2,Mana 1" };
        static List<string> moonSaltPotions = new List<string> { };
        static List<string> citrinitasPotions = new List<string> { "Berserker 2,Bounce 3", "Fire 1,Mana 3,Poisoning 1", "Hallucinations 3,Libido 2", "Charm 2,Levitation 3", "Frost 2,Invisibility 3", "Acid 1,Levitation 1,Lightning 3", "Magical Vision 3,Necromancy 2", "Necromancy 3", "Light 3,Rich Harvest 2" };
        static List<string> sunSaltPotions = new List<string> { };
        static List<string> rubedoPotions = new List<string> { "Frost 1,Mana 1,Rich Harvest 3", "Acid 2,Fire 1,Poisoning 1", "Healing 1,Necromancy 3,Slow Down 1", "Hallucinations 1,Magical Vision 2,Mana 1", "Libido 3", "Berserker 3,Explosion 1,Fire 1", "Bounce 1,Hallucinations 2,Invisibility 2", "Healing 3,Libido 2", "Berserker 1,Charm 2,Libido 2", "Healing 1,Light 1,Lightning 1,Mana 1,Poisoning 1", "Charm 1,Explosion 1,Fire 1,Light 1", "Frost 1,Slow Down 2,Stone Skin 1" };
        static List<string> lifeSaltPotions = new List<string> { "Fire 1,Healing 1,Libido 2,Poisoning 1", "Hallucinations 2,Healing 2,Magical Vision 1", "Berserker 2,Healing 2", "Acid 1,Healing 1,Light 1,Mana 1,Stone Skin 1", "Healing 3", "Healing 1,Necromancy 2", "Fast Growth 2,Healing 1,Rich Harvest 2", "Charm 2,Healing 2,Levitation 1", "Healing 1,Necromancy 2,Sleep 2", "Berserker 1,Bounce 1,Charm 1,Healing 1,Levitation 1", "Charm 1,Frost 1,Healing 1,Levitation 1,Lightning 1", "Hallucinations 1,Healing 1,Invisibility 1,Mana 2" };
        static List<string> philoStonePotions = new List<string> { "Frost 1,Magical Vision 1,Mana 1,Rich Harvest 1,Sleep 1", "Acid 2,Necromancy 2,Stone Skin 1", "Fast Growth 1,Healing 1,Poisoning 1,Slow Down 1,Stone Skin 1", "Bounce 1,Invisibility 1,Levitation 1,Lightning 1,Mana 1", "Hallucinations 1,Necromancy 3,Rich Harvest 1", "Fast Growth 1,Light 2,Rich Harvest 1,Stone Skin 1", "Fast Growth 1,Healing 1,Rich Harvest 3", "Acid 1,Explosion 1,Necromancy 3", "Charm 2,Levitation 1,Libido 2", "Berserker 1,Explosion 1,Fire 1,Libido 1,Light 1", "Explosion 1,Fire 1,Frost 1,Lightning 1,Poisoning 1", "Berserker 1,Charm 1,Hallucinations 1,Invisibility 1,Levitation 1" };
        static List<string> philoSaltPotions = new List<string> { };
        // Would be ideal to just fetch these lists from Delivery Category but not sure how to specify which merchant
        // Downside is I'll have to add to these if new ingredients are added
        static List<string> herbs = new List<string> { "Leaf", "Waterbloom", "Firebell", "Windbloom", "Thornstick", "Tangleweeds", "Thistle", "LumpyBeet", "IceDragonfruit", "LavaRoot", "Refruit", "Goldthorn", "BloodyRoot" };
        static List<string> fungi = new List<string> { "GoblinMushroom", "DryadMushroom", "CliffFungus", "Marshrooms", "Wierdshroom", "GreenMushroom", "RedMushroom", "GreyChanterelle", "GraveTruffle", "SulphurShelf", "WitchMushroom" };
        static List<string> crystals = new List<string> { "EarthCrystal", "FrostSapphire", "FireCitrine", "Crystal", "BloodRuby" };
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
        public static float thresholdDistance = 0.05f; // Could be adjusted... Balance between file size and precision.
        public static int decimalPrecision = 2; // Save on some more space
        // Reduced space usage for 4-ingredient hallu 3 from ~140 kB to 21 kB. Can't really get it much smaller without using Bezier curve stuff. Max size of whole database is maybe 3 GB...
        // I calculated 97,728 unique potion effect combinations (including empty), and that would make this database on the scale of terabytes with all challenges. But not all potions are useful, so I'm sure not all would be uploaded anyway.
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
                        ingredientCount += c.amount;
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

        public void TryToAdd(Vector2 point, SerializedRecipeMark mark, int numMarks, float health, float rotation, float teleportStatus, float whirlpoolStatus, float saltMovement, string state)
        {
            float philSaltStatus = 0;
            if (saltMovement - lastSaltMovement != 0 || (mark.type == SerializedRecipeMark.Type.Salt && mark.stringValue == "Philosopher's Salt"))
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
                ControlPoint newPoint = new ControlPoint(point, mark, health, rotation, teleportStatus, whirlpoolStatus, philSaltStatus, 0f, state);
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
                        float cumulativeDistance = GetPoint(points.Count - 1).cumulativeDistance + (float)Math.Sqrt(Math.Pow(point.x - GetPoint(points.Count - 1).x, 2) + Math.Pow(point.y - GetPoint(points.Count - 1).y, 2));
                        ControlPoint newPoint = new ControlPoint(point, mark, health, rotation, teleportStatus, whirlpoolStatus, philSaltStatus, cumulativeDistance, state);
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
                                float cumulativeDistance = GetPoint(points.Count - 1).cumulativeDistance + (float)Math.Sqrt(Math.Pow(point.x - GetPoint(points.Count - 1).x, 2) + Math.Pow(point.y - GetPoint(points.Count - 1).y, 2));
                                ControlPoint newPoint = new ControlPoint(point, mark, health, rotation, teleportStatus, whirlpoolStatus, philSaltStatus, cumulativeDistance, state);
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
                                // State?
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
                                float cumulativeDistance = GetPoint(points.Count - 1).cumulativeDistance + (float)Math.Sqrt(Math.Pow(point.x - GetPoint(points.Count - 1).x, 2) + Math.Pow(point.y - GetPoint(points.Count - 1).y, 2));
                                ControlPoint newPoint = new ControlPoint(point, newMark, health, rotation, teleportStatus, whirlpoolStatus, philSaltStatus, cumulativeDistance, state);
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
                                string defaultName = ((InventoryItem)c.componentObject).name;
                                if (ingredientTranslation.ContainsKey(defaultName)) { 
                                    ingredientsString += c.amount + " " + ingredientTranslation[defaultName];
                                }
                                else
                                {
                                    // Fallback for custom ingredients
                                    ingredientsString += c.amount + " " + defaultName;
                                }
                                if (i < usedComponents.Count-1)
                                {
                                    ingredientsString += ",";
                                }
                            }
                        }
                    }

                    // Effects (translated)
                    List<string> translatedEffects = new List<string>();
                    for (int i = 0; i < effects.Count; i++)
                    {
                        if (effectTranslation.ContainsKey(effects[i]))
                        {
                            translatedEffects.Add(effectTranslation[effects[i]]);
                        }
                        else
                        {
                            // Fallback for custom effects
                            translatedEffects.Add(effects[i]);
                        }
                    }
                    var groups = translatedEffects.GroupBy(s => s).Select(s => new { Effect = s.Key, Count = s.Count() });
                    Dictionary<string, int> effectCounts = groups.ToDictionary(g => g.Effect, g => g.Count);
                    string effectsString = "";
                    // Alphabetical for consistency?
                    List<string> sortedKeys = effectCounts.Keys.OrderBy(q => q).ToList();
                    for (int i = 0; i < sortedKeys.Count; i++)
                    {
                        effectsString += sortedKeys[i] + " " + effectCounts[sortedKeys[i]];
                        if (i < sortedKeys.Count-1)
                        {
                            effectsString += ",";
                        }
                    }
                    /*
                    for (int i = 0; i < effects.Count; i++)
                    {
                        effectsString += effects[i];
                        if (i < effects.Count - 1)
                        {
                            effectsString += ",";
                        }
                    }
                    */

                    // Challenge tags
                    string challengeTags = "";

                    List<string> usedIngredients = new List<string>();

                    // Ingredient categories; Start out true but then narrowed down; Default Mixed
                    bool isOrganic = true;
                    bool isHerbal = true;
                    bool isFungal = true;
                    bool isCrystalline = true;
                    // Ingredient counts
                    bool isHighlander = true;
                    bool isLowlander = true;

                    // Ladle use; Default Dry
                    bool isWet = false;
                    // Whirlpool use; Default Cold
                    bool isHot = false;
                    // Salt use; Default Dull
                    bool isRich = false;
                    // Grinding; No default, since they need to be combined in a particular way...
                    bool usesAnyGrind = false;
                    bool usesFullGrind = false;
                    bool usesCracking = false;
                    bool usesNoGrind = false;
                    // Cheated (with instant grind on frost sapphire :/)
                    bool cheated = false;
                    
                    for (int i = 0; i < points.Count; i++)
                    {
                        int typeCode = points[i].typeCode;
                        string pName = points[i].name;
                        // Check ingredient restrictions
                        if (typeCode == 128)
                        {
                            if (usedIngredients.Contains(pName))
                            {
                                isHighlander = false;
                            }
                            else
                            {
                                usedIngredients.Add(pName);
                                if (herbs.Contains(pName))
                                {
                                    isFungal = false;
                                    isCrystalline = false;
                                }
                                else if (fungi.Contains(pName))
                                {
                                    isHerbal = false;
                                    isCrystalline = false;
                                }
                                else if (crystals.Contains(pName))
                                {
                                    isHerbal = false;
                                    isFungal = false;
                                    isOrganic = false;
                                }
                                if (usedIngredients.Count > 1)
                                {
                                    isLowlander = false;
                                }
                            }
                        }
                        // Check other restrictions
                        string pState = points[i].grindState;
                        if (pState.Equals("Move"))
                        {
                            if (pName.Contains("Ladle"))
                            {
                                isWet = true;
                            }
                            else if (pName.Contains("Whirlpool"))
                            {
                                isHot = true;
                            }
                        }
                        else if (pState.Equals("Salt"))
                        {
                            isRich = true;
                        }
                        else if (pState.Equals("Cheated"))
                        {
                            cheated = true;
                        }
                        else if (!cheated)
                        {
                            if(pState.Equals("Other"))
                            {
                                usesAnyGrind = true;
                            }
                            else if (!usesAnyGrind)
                            {
                                if (pState.Equals("Full"))
                                {
                                    usesFullGrind = true;
                                }
                                else if (pState.Equals("None"))
                                {
                                    usesNoGrind = true;
                                }
                                else if (pState.Equals("Cracked"))
                                {
                                    usesCracking = true;
                                }
                            }
                        }
                    }
                    // Form the challenge tags
                    challengeTags += "mixed,";
                    if (isOrganic)
                    {
                        challengeTags += "organic,";
                    }
                    if (isHerbal)
                    {
                        challengeTags += "herbal,";
                    }
                    if (isFungal)
                    {
                        challengeTags += "fungal,";
                    }
                    if (isCrystalline)
                    {
                        challengeTags += "crystalline,";
                    }
                    challengeTags += "any number,";
                    if (isHighlander)
                    {
                        challengeTags += "highlander,";
                    }
                    if (isLowlander)
                    {
                        challengeTags += "lowlander,";
                    }
                    if (isWet)
                    {
                        challengeTags += "wet,";
                    }
                    else
                    {
                        challengeTags += "dry,";
                        challengeTags += "wet,"; // water *could* be used, just isn't necessary
                    }
                    if (isHot)
                    {
                        challengeTags += "hot,";
                    }
                    else
                    {
                        challengeTags += "cold,";
                        challengeTags += "hot,"; // whirlpool *could* be used, just isn't necessary
                    }
                    if (isRich)
                    {
                        challengeTags += "rich,";
                    }
                    else
                    {
                        challengeTags += "dull,";
                        challengeTags += "rich,"; // salt *could* be used, just isn't necessary
                    }
                    if (usesAnyGrind)
                    {
                        challengeTags += "any grind,";
                    }
                    else
                    {
                        if (usesFullGrind)
                        {
                            if (usesNoGrind)
                            {
                                if (usesCracking)
                                {
                                    challengeTags += "extreme cracked,";
                                }
                                else
                                {
                                    challengeTags += "extreme,";
                                    challengeTags += "extreme cracked,";  // cracking *could* be used, just isn't necessary
                                }
                            }
                            else
                            {
                                if (usesCracking)
                                {
                                    challengeTags += "extreme cracked,";
                                }
                                else
                                {
                                    challengeTags += "full-grind,";
                                    challengeTags += "extreme,";  // no-grind *could* be used, just isn't necessary
                                    challengeTags += "extreme cracked,";  // cracking *could* be used, just isn't necessary
                                }
                            }
                        }
                        else
                        {
                            if (usesNoGrind)
                            {
                                if (usesCracking)
                                {
                                    challengeTags += "mortarless,";
                                }
                                else
                                {
                                    challengeTags += "no-grind,";
                                    challengeTags += "mortarless,"; // cracking *could* be used, just isn't necessary
                                }
                            }
                            else
                            {
                                if (usesCracking)
                                {
                                    challengeTags += "mortarless,";
                                }
                                else
                                {
                                    challengeTags += "any grind,"; // ???
                                }
                            }
                        }
                    }
                    if (cheated)
                    {
                        challengeTags += "cheated,";
                    }
                    else
                    {
                        challengeTags += "legit,";
                    }
                    // Effect level / number of effects
                    if (effectsString.Length > 0)
                    {
                        int effectLevel = effectCounts.Values.Max();
                        challengeTags += "level " + effectLevel + ",";
                        int numEffects = effectCounts.Count;
                        challengeTags += numEffects + " effect,";

                        // Other challenge tags for alchemy machine recipes
                        if (nigredoPotions.Contains(effectsString))
                        {
                            challengeTags += "nigredo,";
                        }
                        if (albedoPotions.Contains(effectsString))
                        {
                            challengeTags += "albedo,";
                        }
                        if (citrinitasPotions.Contains(effectsString))
                        {
                            challengeTags += "citrinitas,";
                        }
                        if (rubedoPotions.Contains(effectsString))
                        {
                            challengeTags += "rubedo,";
                        }
                        if (philoStonePotions.Contains(effectsString))
                        {
                            challengeTags += "philosopher's stone,";
                        }
                        if (voidSaltPotions.Contains(effectsString))
                        {
                            challengeTags += "void salt,";
                        }
                        if (lifeSaltPotions.Contains(effectsString))
                        {
                            challengeTags += "life salt,";
                        }
                    }
                    
                    // Whether the potion failed or not
                    if (failed == 1)
                    {
                        challengeTags += "failed";
                    }
                    else
                    {
                        challengeTags += "successful";
                    }

                    float totalPathDistance = points[points.Count - 1].cumulativeDistance;
                    float lastRotation = points[points.Count - 1].rotation;


                    if (asCSV)
                    {
                        // As CSV
                        try
                        {  
                            StreamWriter writer = new StreamWriter("recipe_path_" + name + ".csv");
                            // Going to have to include one additional header line for metadata... So it's not *really* a CSV, but still.
                            writer.WriteLine("Metadata_start");
                            writer.WriteLine(totalCost + "," + noSaltCost + "," + ingredientCount + "," + stress + "," + totalPathDistance + "," + lastRotation);
                            writer.WriteLine(effectsString);
                            writer.WriteLine(ingredientsString);
                            writer.WriteLine(saltsString);
                            writer.WriteLine(challengeTags);
                            writer.WriteLine("Metadata_end");
                            // Full version
                            //writer.WriteLine("step,name,x,y,typeCode,grindState,value,health,rotation,teleport,whirlpool,philSalt,distance");
                            // Reduced (step is easy to recover from line number, typeCode covers teleport, whirlpool>0, philSalt, throwing out distance and rotation and just using the total)
                            writer.WriteLine("typeCode,name,x,y,health");
                            for (int i = 0; i < points.Count; i++)
                            {
                                ControlPoint p = points[i];
                                string pointName = p.name;
                                if (p.typeCode == 128) // Not necessary for the salts, since they are already named correctly
                                {
                                    if (ingredientTranslation.ContainsKey(p.name))
                                    {
                                        pointName = ingredientTranslation[p.name] + "_" + p.grindState; // Merge ingredient name and grind state to save space...
                                    }
                                    else
                                    {
                                        // Fallback for custom ingredients
                                        pointName = p.name + "_" + p.grindState; // Merge ingredient name and grind state to save space...
                                    }
                                }
                                else if (p.typeCode == 64 || p.typeCode == 0) // Salt and base
                                {
                                    pointName = p.name;
                                }
                                else 
                                {
                                    pointName = ""; // Save space, already covered in the typeCode, no need to write out things like "Stir" over and over
                                }
                                // Full version
                                //writer.WriteLine(i + "," + pointName + "," + Math.Round(p.x, decimalPrecision) + "," + Math.Round(p.y, decimalPrecision) + "," + p.typeCode + "," + p.grindState + "," + Math.Round(p.value, decimalPrecision) + "," + Math.Round(p.health, decimalPrecision) + "," + Math.Round(p.rotation, decimalPrecision) + "," + p.teleportStatus + "," + p.whirlpoolStatus + "," + p.philSaltStatus + "," + Math.Round(p.cumulativeDistance, decimalPrecision));
                                writer.WriteLine(p.typeCode + "," + pointName + "," + Math.Round(p.x, decimalPrecision) + "," + Math.Round(p.y, decimalPrecision) + "," + Math.Round(p.health, decimalPrecision));
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
