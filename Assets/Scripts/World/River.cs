using UnityEngine;
using System.Collections.Generic;

namespace Assets.Scripts.World
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
        public List<TileData> Tiles;
        public int ID;

        public int Intersections;
        public float TurnCount;
        public Direction CurrentDirection;

        public River(int id)
        {
            ID = id;
            Tiles = new List<TileData>();
        }

        public void AddTile(TileData tile)
        {
            tile.SetRiverPath(this);
            Tiles.Add(tile);
        }
    }

    public class RiverGroup
    {
        public List<River> Rivers = new List<River>();
    }
}