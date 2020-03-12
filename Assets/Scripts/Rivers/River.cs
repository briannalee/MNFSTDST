using System.Collections.Generic;
using Assets.Scripts.World;

namespace Assets.Scripts.Rivers
{
    public enum Direction
    {
        Left,
        Right,
        Top,
        Bottom
    }

    public class River
    {

        public int Length;
        public List<TerrainTile> Tiles;
        public int ID;

        public int Intersections;
        public float TurnCount;
        public Direction CurrentDirection;

        public River(int id)
        {
            ID = id;
            Tiles = new List<TerrainTile>();
        }

        public void AddTile(TerrainTile terrainTile)
        {
            terrainTile.SetRiverPath(this);
            Tiles.Add(terrainTile);
        }
    }

    public class RiverGroup
    {
        public List<River> Rivers = new List<River>();
    }
}