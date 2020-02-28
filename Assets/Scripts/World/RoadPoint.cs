using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Scripts.World
{
    public class RoadPoint
    {
        public Vector3 Position { get; }
        public Road ThisRoad { get; }
        public Vector3Int CellPosition { get; }
        public RoadPoint NearestNeighbour { get; set;  }

        public RoadPoint(Vector3 position, Vector3Int cellPosition, Road road, RoadPoint nearestNeighbour = null)
        {
            Position = position;
            ThisRoad = road;
            CellPosition = cellPosition;
            NearestNeighbour = nearestNeighbour;
        }
    }
}
