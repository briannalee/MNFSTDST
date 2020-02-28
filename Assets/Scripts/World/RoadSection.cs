using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.World
{
    public class RoadSection
    {
        public List<RoadPoint> RoadPoints { get; set; }
        public bool IsAdjustedSection { get; set; }
        public Road RoadAccessor { get; set; }
        public bool Skip { get; set; }
                
        public RoadSection(Road road, bool isAdjustedSection = false)
        {
            RoadAccessor = road;
            IsAdjustedSection = isAdjustedSection;
            RoadPoints = new List<RoadPoint>();
        }

        public void Add(RoadPoint roadPoint)
        {
            RoadPoints.Add(roadPoint);
        }

        public void Add(Vector3 position, RoadPoint nearestNeighbour = null)
        {
            RoadPoints.Add(RoadAccessor.CreateRoadPoint(position, nearestNeighbour));
        }
    }
}
