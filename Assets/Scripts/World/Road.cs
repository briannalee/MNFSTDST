using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Scripts.World
{
    public class Road
    {
        public List<RoadPoint> Points { get; set; }
        public List<Vector3Int> MapCells { get; set; }
        public List<RoadSection> Sections { get; set; }
        public Tilemap TilemapAccessor;
        public RoadSection ActiveSection { get; set; }

        public Road(int pointsCount,Tilemap tilemap)
        {
            //By Default, there is always at least one road section
            Sections = new List<RoadSection>();
            RoadSection roadSection = new RoadSection(this,false);
            ActiveSection = roadSection;
            Sections.Add(roadSection);
            
            TilemapAccessor = tilemap;
            Points = new List<RoadPoint>(pointsCount);
            MapCells = new List<Vector3Int>();
        }

        public RoadPoint AddFinalizedRoadPoint(Vector3 position)
        {
            RoadPoint point = CreateRoadPoint(position);
            MapCells.Add(point.CellPosition);
            Points.Add(point);
            return point;
        }
        public void AddFinalizedRoadPoint(RoadPoint roadPoint)
        {
            MapCells.Add(roadPoint.CellPosition);
            Points.Add(roadPoint);
        }

        public RoadPoint CreateRoadPoint(Vector3 position, RoadPoint nearestNeighbour = null)
        {
            Vector3Int cellPosition = TilemapAccessor.WorldToCell(position);
            RoadPoint point = new RoadPoint(position, cellPosition, this, nearestNeighbour);
            return point;
        }

        public void AddPointToSection(Vector3 position, RoadPoint nearestNeighbour = null)
        {
            bool isAdjustedPoint = nearestNeighbour != null;
            if (isAdjustedPoint && !ActiveSection.IsAdjustedSection || !isAdjustedPoint && ActiveSection.IsAdjustedSection)
            {
                AddRoadSection(isAdjustedPoint);
            }
            ActiveSection.Add(position, nearestNeighbour);
        }

        public RoadSection AddRoadSection(bool isAdjustedSection = false)
        {
            RoadSection roadSection = new RoadSection(this,isAdjustedSection);
            ActiveSection = roadSection;
            Sections.Add(roadSection);
            return roadSection;
        }
    }
}