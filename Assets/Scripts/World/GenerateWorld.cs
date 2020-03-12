using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Roads;
using Assets.Scripts.Sprites;
using Assets.Scripts.Villages;
using UnityEngine;
using UnityEngine.Tilemaps;
using Pathfinding;
using Random = UnityEngine.Random;
using Vector3Int = UnityEngine.Vector3Int;

namespace Assets.Scripts.World
{
    public class GenerateWorld : MonoBehaviour
    {
        public int Seed;

        public Tilemap Tilemap;

        public GameObject Overlay;

        public AstarPath Pathfinder;

        public RoadHandler RoadHandler;

        public WorldData World;

        public Seeker SeekerTool;

        public bool MapGenerated;

        public SpriteHelper SpriteHelper;

        public readonly Dictionary<Vector3Int, VillageTile> VillagesDictionary = new Dictionary<Vector3Int, VillageTile>();

        public static Color DryestColor = new Color(248f / 255f, 164f / 255f, 6f / 255f);
        public static Color DryerColor = new Color(225f / 255f, 182f / 255f, 102f / 255f);
        public static Color DryColor = new Color(248f / 255f, 215f / 255f, 152f / 255f);
        public static Color WettestColor = new Color(190f / 255f, 196f / 255f, 255f / 255f);

        // Start is called before the first frame update
        private void Start()
        {
            SpriteHelper = gameObject.GetComponent<SpriteHelper>();
            SpriteHelper.LoadSprites();
            RoadHandler = gameObject.AddComponent<RoadHandler>();
            RoadHandler.RoadTilemap = Tilemap;
            SeekerTool = GetComponent<Seeker>();
            


            World = new WorldData(256,256);
            Overlay.transform.localScale = new Vector3(World.Width, World.Height, 1);
            Overlay.transform.position = new Vector3(World.Width / 2f, World.Height / 2f, -1);

            StartCoroutine(Load());
        }

        private IEnumerator<Coroutine> Load()
        {
            yield return StartCoroutine(GenerateVillages());

            while (SpriteHelper.ActiveThreads > 0)
            {
                yield return null;
            }
            GenerateTilemap();
            MapGenerated = true;
        }

        private void GenerateTilemap()
        {
            for (int x = 0; x < World.Width; x++)
            {
                TerrainTile[] data = TileArray<TerrainTile>.GetRow(World.TerrainTileMap, x);
                // ReSharper disable once CoVariantArrayConversion
                Tilemap.SetTilesBlock(new BoundsInt(x, 0, 0, 1, World.Height, 1), data);
            }
            // ReSharper disable once CoVariantArrayConversion
            Tilemap.SetTiles(RoadHandler.RoadDictionary.Keys.ToArray(), RoadHandler.RoadDictionary.Values.ToArray());
            // ReSharper disable once CoVariantArrayConversion
            Tilemap.SetTiles(VillagesDictionary.Keys.ToArray(),VillagesDictionary.Values.ToArray());
        }


        private IEnumerator OptimizeRoads()
        {
            KeyValuePair<Vector3Int, VillageTile>[] villages = VillagesDictionary.ToArray();

            for (int i = 0; i < villages.Length; i++)
            {
                int nextIndex = (i < VillagesDictionary.Count - 1) ? i + 1 : 0;
                yield return StartCoroutine(RoadHandler.CreateRoad(villages[i].Value.WorldPosition, villages[nextIndex].Value.WorldPosition));
            }
        }

        public IEnumerator<Coroutine> GenerateVillages()
        {
            int generatedVillages = 0;
            while (generatedVillages < 10)
            {
                int x = Random.Range(5, World.Width - 5);
                int y = Random.Range(5, World.Height - 5);
                Vector3Int position = new Vector3Int(x, y, 5);

                if ((World.TerrainTileMap[x, y].HeightType == HeightType.Forest || World.TerrainTileMap[x, y].HeightType == HeightType.Grass) && !VillagesDictionary.ContainsKey(position))
                {
                    
                    VillageTile villageTile = ScriptableObject.CreateInstance<VillageTile>();
                    villageTile.Create(Tilemap.CellToWorld(position) + new Vector3(0.5f, 0.5f, 0), position,RelationshipType.Neutral);
                    VillagesDictionary.Add(position,villageTile);
                    generatedVillages++;
                }
            }
            yield return StartCoroutine(OptimizeRoads());
            Debug.Log("Finished Villages...");
        }

    }
}