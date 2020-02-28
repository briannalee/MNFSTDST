using System.Collections.Generic;
using Assets.Scripts.World;
using Pathfinding;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Scripts.Villages
{
    public class Village
    {
        public Vector3 Position { get; set; }
        public Vector3Int CellPosition { get; set; }

        public Village(Vector3Int position, Tilemap tilemap)
        {
            Position = tilemap.CellToWorld(position) + new Vector3(0.5f,0.5f,0);
            CellPosition = position;
        }
    }
}
