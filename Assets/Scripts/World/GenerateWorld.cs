using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts._3rdparty;
using Assets.Scripts.Roads;
using Assets.Scripts.Sprites;
using Assets.Scripts.Villages;
using UnityEngine;
using UnityEngine.Tilemaps;
using Pathfinding;
using UnityEngine.UI;
using Assets.Scripts.Market;
using Random = UnityEngine.Random;
using Vector3Int = UnityEngine.Vector3Int;

namespace Assets.Scripts.World
{
    public class GenerateWorld : MonoBehaviour
    {
        public int Seed;

        public Tilemap Tilemap;
        public static Tilemap TilemapAccessor;

        public GameObject Overlay;

        public AstarPath Pathfinder;

        public RoadHandler RoadHandler;

        public static WorldData World;

        public Seeker SeekerTool;

        public static bool MapGenerated;

        public SpriteHelper SpriteHelper;

        public GameObject Villager;

        public GameObject VillageAIPrefab;

        public readonly Dictionary<Vector3Int, VillageTile> VillagesDictionary = new Dictionary<Vector3Int, VillageTile>();

        public static Color DryestColor = new Color(248f / 255f, 164f / 255f, 6f / 255f);
        public static Color DryerColor = new Color(225f / 255f, 182f / 255f, 102f / 255f);
        public static Color DryColor = new Color(248f / 255f, 215f / 255f, 152f / 255f);
        public static Color WettestColor = new Color(190f / 255f, 196f / 255f, 255f / 255f);

        public static ArrayByEnum<decimal, ResourceType> ResourceMarketPrices = new ArrayByEnum<decimal, ResourceType>();

        public Text WaterPriceDisplay;

        // Start is called before the first frame update
        private void Start()
        {
            TilemapAccessor = Tilemap;
            Market.Market.Init();
            ResourceMarketPrices[ResourceType.Water] = 3m;
            ResourceMarketPrices[ResourceType.Food] = 8m;
            ResourceMarketPrices[ResourceType.Wood] = 5m;
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

        private void Update()
        {
            foreach (ResourceType resource in Enum.GetValues(typeof(ResourceType)))
            {
                int marketSellOrders = Market.Market.SellOrders[resource].Count;
                int marketBuyOrders = Market.Market.BuyOrders[resource].Count;
                decimal newPrice = ResourceMarketPrices[resource];
                if (marketBuyOrders > marketSellOrders) newPrice += 0.0001m;
                if (marketSellOrders > marketBuyOrders) newPrice -= 0.0001m;
                if (newPrice.CompareTo(0.01m) < 0) newPrice = 0.1m;
                ResourceMarketPrices[resource] = newPrice;
            }
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

                if ((World.TerrainTileMap[x, y].HeightType == HeightType.Forest || World.TerrainTileMap[x, y].HeightType == HeightType.Grass || World.TerrainTileMap[x, y].HeightType == HeightType.Dirt) && !VillagesDictionary.ContainsKey(position))
                {
                    
                    VillageTile villageTile = ScriptableObject.CreateInstance<VillageTile>();
                    villageTile.Create(Tilemap.CellToWorld(position) + new Vector3(0.5f, 0.5f, 0), position,RelationshipType.Neutral);
                    Instantiate(VillageAIPrefab).GetComponent<VillageAI>().VillageTile = villageTile;
                    VillagesDictionary.Add(position,villageTile);
                    generatedVillages++;
                }
            }
            yield return StartCoroutine(OptimizeRoads());

            foreach (KeyValuePair<Vector3Int, VillageTile> village in VillagesDictionary)
            {
                for (int i = 0; i < 10; i++)
                {
                    Villager villager = Instantiate(Villager).GetComponent<Villager>();
                    villager.GenerateVillager(this,village.Value);
                }
            }
            Debug.Log("Finished Villages...");
        }

    }
}