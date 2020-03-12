using System.Collections.Generic;

namespace Assets.Scripts.World
{
    public enum TileGroupType
    {
        Water,
        Land
    }

    public class TileGroup
    {
        public List<TerrainTile> Tiles;

        public TileGroupType Type;

        public TileGroup()
        {
            Tiles = new List<TerrainTile>();
        }
    }
}