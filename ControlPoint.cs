using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Lymm37.PotionCraft.RecipeMapPlayback
{
    // Combines the functionality of a point and a SerializedRecipeMark
    [Serializable]
    class ControlPoint
    {
        // Vector2 isn't serializable for some stupid reason!
        //public Vector2 p;
        public float x;
        public float y;
        public SerializedRecipeMark.Type type;
        public int typeCode; // Bitwise - stir, ladle, heat (for effect), whirlpool, teleport, philosopher's salt, 
        public string name;
        public float value;
        public float health;
        public float rotation;
        public float teleportStatus;
        public float whirlpoolStatus;
        public float philSaltStatus;
        //public string type;

        public ControlPoint(Vector2 p, SerializedRecipeMark mark, float health, float rotation, float teleportStatus, float whirlpoolStatus, float philSaltStatus)
        {
            this.x = p.x;
            this.y = p.y;
            this.type = mark.type;
            this.value = mark.floatValue;
            this.health = health;
            this.rotation = rotation;
            this.teleportStatus = teleportStatus;
            this.whirlpoolStatus = whirlpoolStatus;
            this.philSaltStatus = philSaltStatus;
            switch (type)
            {
                // Start
                case SerializedRecipeMark.Type.PotionBase:
                    this.name = mark.stringValue;
                    this.typeCode = 0;
                    break;
                // Adding an ingredient
                case SerializedRecipeMark.Type.Ingredient:
                    this.name = mark.stringValue;
                    this.typeCode = 128;
                    break;
                // Stirring
                case SerializedRecipeMark.Type.Spoon:
                    this.name = "Stir";
                    this.typeCode = 1;
                    if (whirlpoolStatus > 0)
                    {
                        this.name = "Whirlpool + Stir";
                        this.typeCode = 9;
                    }
                    // First frame of stirring when starting a teleport will show the teleport status as 1 even though stir is the mark. Should that show up as teleport + stir?
                    break;
                // Watering
                case SerializedRecipeMark.Type.Ladle:
                    this.name = "Ladle";
                    this.typeCode = 2;
                    if (teleportStatus != 0)
                    {
                        this.name = "Teleport + Ladle";
                        this.typeCode = 18;
                    }
                    else if (whirlpoolStatus > 0)
                    {
                        this.name = "Whirlpool + Ladle";
                        this.typeCode = 10;
                    }
                    break;
                // Adding salt
                case SerializedRecipeMark.Type.Salt:
                    this.name = mark.stringValue;
                    this.typeCode = 64;
                    if (philSaltStatus != 0)
                    {
                        // These include adding the philosopher's salt AND moving from it
                        this.typeCode = 96;
                        if (teleportStatus != 0)
                        {
                            this.name = "Teleport + Philosopher's Salt";
                            this.typeCode = 112;
                        }
                        else if (whirlpoolStatus > 0)
                        {
                            this.name = "Whirlpool + Philosopher's Salt";
                            this.typeCode = 104;
                        }
                    }
                    break;
                // Default
                case SerializedRecipeMark.Type.Bellows:
                    this.name = "Heat";
                    this.typeCode = 4;
                    if (teleportStatus != 0)
                    {
                        this.name = "Teleport";
                        this.typeCode = 16;
                        if (philSaltStatus != 0)
                        {
                            this.name = "Teleport + Philosopher's Salt";
                            this.typeCode = 48;
                        }
                    }
                    else if (whirlpoolStatus > 0)
                    {
                        this.name = "Whirlpool";
                        this.typeCode = 8;
                        if (philSaltStatus != 0)
                        {
                            this.name = "Whirlpool + Philosopher's Salt";
                            this.typeCode = 40;
                        }
                    }
                    else if (philSaltStatus != 0)
                    {
                        this.name = "Philosopher's Salt";
                        this.typeCode = 32;
                    }
                    // The philosopher's salt ones here are from moving from the salt but not from adding it. Surprisingly, it will keep moving you a bit after you add it.
                    break;
                
            }

        }

        public float Distance(ControlPoint other)
        {
            return (float)Math.Sqrt(Math.Pow(x - other.x, 2) + Math.Pow(y - other.y, 2));
        }

        public Vector2 GetPoint()
        {
            return new Vector2(x, y);
        }

        public string GetName()
        {
            return name;
        }

        public ControlPoint Clone()
        {
            Vector2 p = new Vector2(x, y);
            SerializedRecipeMark mark = new SerializedRecipeMark();
            mark.type = type;
            mark.floatValue = value;
            mark.stringValue = name;
            return new ControlPoint(p, mark, health, rotation, teleportStatus, whirlpoolStatus, philSaltStatus);
        }
    }
}

