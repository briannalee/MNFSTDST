using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.World
{
    public class Road
    {
        public List<Vector3> RoadPoints { get; set; }
        public List<List<Vector3[]>> AdjustedRoadPoints { get; set; }

        public Road(int pointsCount)
        {
            RoadPoints = new List<Vector3>(pointsCount);
        }
    }
}