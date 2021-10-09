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
        public string name;
        public float value;
        public float health;
        public float rotation;
        public float teleportStatus;
        //public string type;

        public ControlPoint(Vector2 p, SerializedRecipeMark mark, float health, float rotation, float teleportStatus)
        {
            this.x = p.x;
            this.y = p.y;
            this.type = mark.type;
            this.name = mark.stringValue;
            this.value = mark.floatValue;
            this.health = health;
            this.rotation = rotation;
            this.teleportStatus = teleportStatus;
            if (name is null)
            {
                if (type == SerializedRecipeMark.Type.Spoon)
                {
                    this.name = "Stir";
                }
                if (type == SerializedRecipeMark.Type.Bellows)
                {
                    this.name = "Heat";
                }
                if (type == SerializedRecipeMark.Type.Ladle)
                {
                    this.name = "Ladle";
                }
                // Others won't be null.
            }
            if (teleportStatus != 0 && name != "Ladle")
            {
                this.name = "Teleport";
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
            switch (type)
            {
                case SerializedRecipeMark.Type.PotionBase:
                    return "Start";
                case SerializedRecipeMark.Type.Ingredient:
                    return name;
                case SerializedRecipeMark.Type.Spoon:
                    return "Stir";
                case SerializedRecipeMark.Type.Ladle:
                    return "Water";
                case SerializedRecipeMark.Type.Bellows:
                    return "Heat";
                case SerializedRecipeMark.Type.Salt:
                    return name;
            }
            return "Bug";
        }

        public ControlPoint Clone()
        {
            Vector2 p = new Vector2(x, y);
            SerializedRecipeMark mark = new SerializedRecipeMark();
            mark.type = type;
            mark.floatValue = value;
            mark.stringValue = name;
            return new ControlPoint(p, mark, health, rotation, teleportStatus);
        }
    }
}

