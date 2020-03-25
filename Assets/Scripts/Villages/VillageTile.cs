using System.Collections.Generic;
using Assets.Scripts._3rdparty;
using Assets.Scripts.Sprites;
using Assets.Scripts.World;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Scripts.Villages
{
    public enum RelationshipType
    {
        Friendly = 0,
        Neutral,
        Wary,
        Aggressive
    }

    public enum ResourceType
    {
        Water = 0,
        Food,
        Wood
    }

    public class VillageTile : Tile
    {
        public Vector3 WorldPosition { get; set; }
        public Vector3Int CellPosition { get; set; }

        public RelationshipType Relationship { get; set; }

        public VillageAI VillageAI;
        public ITilemap Tilemap;

        public ArrayByEnum<float, ResourceType> ContainsResources = new ArrayByEnum<float, ResourceType>();

        public void Create(Vector3 position, Vector3Int cellPosition, RelationshipType relationshipType)
        {
            WorldPosition = position;
            CellPosition = cellPosition;
            Relationship = relationshipType;
            int xPos = cellPosition.x - 5;
            int yPos = cellPosition.y - 5;
            int width = cellPosition.x + 5;
            int height = cellPosition.y + 5;
            for (int x = xPos; x < width; x++)
            for (int y = yPos; y < height; y++)
            {
                TerrainTile tile = GenerateWorld.World.TerrainTileMap[x, y];
                if (tile.BiomeType == BiomeType.Woodland ||
                    tile.BiomeType == BiomeType.TropicalRainforest ||
                    tile.BiomeType == BiomeType.TemperateRainforest ||
                    tile.BiomeType == BiomeType.BorealForest) ContainsResources[ResourceType.Wood] += 0.2f;
                if (tile.HeightType == HeightType.River ||
                    tile.HeightType == HeightType.Mountain ||
                    tile.HeightType == HeightType.Snow) ContainsResources[ResourceType.Water] += 0.2f;
                if (tile.BiomeType == BiomeType.Grassland ||
                    tile.BiomeType == BiomeType.TropicalRainforest ||
                    tile.BiomeType == BiomeType.TemperateRainforest ||
                    tile.BiomeType == BiomeType.BorealForest)
                    ContainsResources[ResourceType.Food] += 0.2f;
            }
        }

        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            tileData.sprite = SpriteHelper.Sprites[Relationship.ToString().ToLower()];
        }
    }
}
