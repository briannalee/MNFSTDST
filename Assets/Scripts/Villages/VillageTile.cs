using Assets.Scripts.Sprites;
using Assets.Scripts.World;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Scripts.Villages
{
    public enum RelationshipType
    {
        Friendly,
        Neutral,
        Wary,
        Aggressive
    }

    public class VillageTile : Tile
    {
        public Vector3 WorldPosition { get; set; }
        public Vector3Int CellPosition { get; set; }

        public RelationshipType Relationship { get; set; }



        public void Create(Vector3 position, Vector3Int cellPosition, RelationshipType relationshipType)
        {
            WorldPosition = position;
            CellPosition = cellPosition;
            Relationship = relationshipType;
        }

        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            if (!SpriteHelper.Sprites.ContainsKey(Relationship.ToString().ToLower()))
            {
                Debug.Log("SPRITE MISSING!!!!");
            }
            tileData.sprite = SpriteHelper.Sprites[Relationship.ToString().ToLower()];
        }
    }
}
