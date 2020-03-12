using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Scripts.Roads
{
    public class Road
    {
        public List<RoadTile> Points { get; set; }
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
            Points = new List<RoadTile>(pointsCount);
            MapCells = new List<Vector3Int>();
        }

        public RoadTile AddFinalizedRoadPoint(Vector3 position)
        {
            RoadTile tile = CreateRoadPoint(position);
            MapCells.Add(tile.CellPosition);
            Points.Add(tile);
            return tile;
        }
        public void AddFinalizedRoadPoint(RoadTile roadTile)
        {
            MapCells.Add(roadTile.CellPosition);
            Points.Add(roadTile);
        }

        public RoadTile CreateRoadPoint(Vector3 position, RoadTile nearestNeighbour = null)
        {
            Vector3Int cellPosition = TilemapAccessor.WorldToCell(position);
            RoadTile tile = ScriptableObject.CreateInstance<RoadTile>();
            tile.Create(position, cellPosition, this, nearestNeighbour);
            return tile;
        }

        public void AddPointToSection(Vector3 position, RoadTile nearestNeighbour = null)
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