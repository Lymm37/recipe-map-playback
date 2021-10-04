using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Lymm37.PotionCraft.RecipeMapPlayback
{
    [Serializable]
    class ControlPoint
    {
        public Vector2 p;
        public string type;
        // Types: "start", "ingredient_end", "crystal_start", "crystal_end", "heating", "water", "water_end", "stirring", "end"

        public ControlPoint(Vector2 p, string type)
        {
            this.p = p;
            this.type = type;
        }

        public float Distance(ControlPoint other)
        {
            return Vector2.Distance(p, other.p);
        }
    }
}
