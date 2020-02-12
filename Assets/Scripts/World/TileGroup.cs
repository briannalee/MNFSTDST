using UnityEngine;
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

        public TileGroupType Type;
        public List<TileData> Tiles;

        public TileGroup()
        {
            Tiles = new List<TileData>();
        }
    }
}
