using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Roads
{
    public class RoadSection
    {
        public List<RoadTile> RoadPoints { get; set; }
        public bool IsAdjustedSection { get; set; }
        public Road RoadAccessor { get; set; }
        public bool Skip { get; set; }
                
        public RoadSection(Road road, bool isAdjustedSection = false)
        {
            RoadAccessor = road;
            IsAdjustedSection = isAdjustedSection;
            RoadPoints = new List<RoadTile>();
        }

        public void Add(RoadTile roadTile)
        {
            RoadPoints.Add(roadTile);
        }

        public void Add(Vector3 position, RoadTile nearestNeighbour = null)
        {
            RoadPoints.Add(RoadAccessor.CreateRoadPoint(position, nearestNeighbour));
        }
    }
}
