using System.Collections.Generic;
using UnityEngine;

namespace ReGoap.Unity.FSMExample.OtherScripts
{
    public static class Utilities
    {
        public static T GetNearest<T>(Vector3 thisPosition, Dictionary<T, Vector3> otherPositions, float stopOnDistance = 1f) where T : class
        {
            T best = null;
            var bestDistance = float.MaxValue;
            foreach (var pair in otherPositions)
            {
                var otherPosition = pair.Value;
                var delta = (thisPosition - otherPosition).sqrMagnitude;
                if (delta < bestDistance)
                {
                    bestDistance = delta;
                    best = pair.Key;
                }
                if (delta <= stopOnDistance)
                    break;
            }
            return best;
        }
    }
}