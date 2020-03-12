using Assets.Scripts.Sprites;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Scripts.Roads
{
    public class RoadTile : Tile
    {
        public Vector3 Position { get; set; }
        public Road ThisRoad { get; set; }
        public Vector3Int CellPosition { get; set; }
        public RoadTile NearestNeighbour { get; set;  }

        public void Create(Vector3 position, Vector3Int cellPosition, Road road, RoadTile nearestNeighbour = null)
        {
            Position = position + new Vector3(0,0,2);
            ThisRoad = road;
            CellPosition = cellPosition + new Vector3Int(0, 0, 2);
            NearestNeighbour = nearestNeighbour;
        }

        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            tileData.sprite = SpriteHelper.Sprites["villages_1225"];
        }
    }
}
