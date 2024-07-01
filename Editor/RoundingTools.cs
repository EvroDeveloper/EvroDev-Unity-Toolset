using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EvroDev.LevelEditorTool.Tools
{
    public static class RoundingTools
    {
        public static Vector3 RoundToNearest(this Vector3 point, float rounding)
        {
            point /= rounding;
            point = Vector3Int.RoundToInt(point);
            point *= rounding;

            return point;
        }

        public static Quaternion Round(this Quaternion q, float incrementDegrees)
        {
            Vector3 euler = q.eulerAngles; // Step 1: Convert to Euler angles

            // Step 2: Round each component
            euler.x = RoundToIncrement(euler.x, incrementDegrees);
            euler.y = RoundToIncrement(euler.y, incrementDegrees);
            euler.z = RoundToIncrement(euler.z, incrementDegrees);

            // Step 3: Convert back to Quaternion
            return Quaternion.Euler(euler);
        }

        // Helper method to round the angle to the nearest increment
        public static float RoundToIncrement(this float angle, float increment)
        {
            return Mathf.Round(angle / increment) * increment;
        }
    }
}
