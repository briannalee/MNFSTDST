using System.Collections.Generic;
using Assets.Scripts._3rdparty;
using Assets.Scripts._3rdparty.AccidentalNoise.Enums;
using Assets.Scripts._3rdparty.AccidentalNoise.Implicit;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Scripts.World
{
    public class GenerateWorld : MonoBehaviour
    {
        //Border Tiles
        private readonly List<Vector3Int> borderTiles = new List<Vector3Int>();


        // private variables
        private ImplicitFractal heightMap;
        private readonly List<Vector3Int> moistureAdjustedTiles = new List<Vector3Int>();

        [SerializeField] public float ColderValue = 0.18f;

        [SerializeField] public float ColdestValue = 0.05f;

        [SerializeField] public float ColdValue = 0.4f;

        [SerializeField] public float DeepWater = 0.2f;

        public double DeepWaterHeight = 0.20;

        public TileBase DeepWaterTile;
        public double DirtHeight = 0.88;
        public TileBase DirtTile;

        [SerializeField] public float DryerValue = 0.27f;

        [SerializeField] public float DryValue = 0.4f;

        [SerializeField] public float Forest = 0.8f;

        public double ForestHeight = 0.62;
        public TileBase ForestTile;

        [SerializeField] public float Grass = 0.7f;

        public double GrassHeight = 42;
        public TileBase GrassTile;
        private MapData heatData;

        [SerializeField] public double HeatFrequency = 3.0;

        private ImplicitCombiner heatMap;

        [Header("Heat Map")] [SerializeField] public int HeatOctaves = 4;

        [SerializeField] public int Height = 512;

        private MapData heightData;
        private readonly List<TileGroup> lands = new List<TileGroup>();

        [SerializeField] public int MaxRiverAttempts = 1000;

        [SerializeField] public int MaxRiverIntersections = 2;

        [SerializeField] public float MinRiverHeight = 0.6f;

        [SerializeField] public int MinRiverLength = 20;

        [SerializeField] public int MinRiverTurns = 18;

        private MapData moistureData;

        [SerializeField] public double MoistureFrequency = 3.0;

        private ImplicitFractal moistureMap;

        [Header("Moisture Map")] [SerializeField]
        public int MoistureOctaves = 4;

        public double MountainHeight = 0.9;
        public TileBase MountainTile;

        [Header("Rivers")] [SerializeField] public int RiverCount = 40;

        private List<RiverGroup> riverGroups = new List<RiverGroup>();

        private List<River> rivers = new List<River>();

        [SerializeField] public float Rock = 0.9f;

        [SerializeField] public float Sand = 0.5f;

        public double SandHeight = 0.28;
        public TileBase SandTile;

        public int Seed;

        [SerializeField] public float ShallowWater = 0.4f;

        public double SnowHeight = 1;
        public TileBase SnowTile;

        [SerializeField] public double TerrainFrequency = 1.25;

        [Header("Height Map")] [SerializeField]
        public int TerrainOctaves = 6;

        public Tilemap Tilemap;

        // Final Objects
        public TileData[,] Tiles;

        [SerializeField] public float WarmerValue = 0.8f;

        [SerializeField] public float WarmValue = 0.6f;

        public double WaterHeight = 0.25;

        private List<TileGroup> waters = new List<TileGroup>();
        public TileBase WaterTile;
        public double WetSandHeight = 0.26;
        public TileBase WetSandTile;

        [SerializeField] public float WetterValue = 0.8f;

        [SerializeField] public float WettestValue = 0.9f;

        [SerializeField] public float WetValue = 0.6f;

        // Adjustable variables for Unity Inspector
        [Header("Generator Values")] [SerializeField]
        public int Width = 512;


        // Start is called before the first frame update
        private void Start()
        {
            Initialize();
            GetData();
            LoadTiles();

            UpdateNeighbors();

            GenerateRivers();
            BuildRiverGroups();
            DigRiverGroups();
            AdjustMoistureMap();

            UpdateBitmasks();
            FloodFill();

            Vector3Int[] positions = new Vector3Int[Height * Width];
            TileBase[] tileArray = new TileBase[positions.Length];


            int index = 0;

            for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
            {
                positions[index] = new Vector3Int(x, y, 0);


                if (Tiles[x, y].Bitmask != 15) borderTiles.Add(new Vector3Int(x, y, 0));

                if (Tiles[x, y].MoistureType == MoistureType.Dryest)
                {
                    tileArray[index] = SandTile;
                    //_moistureAdjustedTiles.Add(new Vector3Int(x, y, 0));
                    index++;
                    continue;
                }

                if (Tiles[x, y].HeightType == HeightType.River)
                {
                    tileArray[index] = WaterTile;
                    index++;
                    continue;
                }

                if (Tiles[x, y].MoistureType == MoistureType.Dryer) moistureAdjustedTiles.Add(new Vector3Int(x, y, 0));

                if (Tiles[x, y].MoistureType == MoistureType.Wet) moistureAdjustedTiles.Add(new Vector3Int(x, y, 0));

                if (Tiles[x, y].MoistureType == MoistureType.Wetter)
                    moistureAdjustedTiles.Add(new Vector3Int(x, y, 0));

                if (Tiles[x, y].HeightValue <= DeepWaterHeight)
                {
                    tileArray[index] = DeepWaterTile;
                    index++;
                    continue;
                }

                if (Tiles[x, y].HeightValue <= WaterHeight)
                {
                    tileArray[index] = WaterTile;
                    index++;
                    continue;
                }

                if (Tiles[x, y].HeightValue <= WetSandHeight)
                {
                    tileArray[index] = WetSandTile;
                    index++;
                    continue;
                }

                if (Tiles[x, y].HeightValue <= SandHeight)
                {
                    tileArray[index] = SandTile;

                    index++;
                    continue;
                }

                if (Tiles[x, y].HeightValue <= GrassHeight)
                {
                    tileArray[index] = GrassTile;
                    index++;
                    continue;
                }

                if (Tiles[x, y].HeightValue <= ForestHeight)
                {
                    tileArray[index] = ForestTile;
                    index++;
                    continue;
                }


                if (Tiles[x, y].HeightValue <= DirtHeight)
                {
                    tileArray[index] = DirtTile;
                    index++;
                    continue;
                }

                if (Tiles[x, y].HeightValue <= MountainHeight)
                {
                    tileArray[index] = MountainTile;
                    index++;
                    continue;
                }

                if (Tiles[x, y].HeightValue <= SnowHeight)
                {
                    tileArray[index] = SnowTile;
                    index++;
                    continue;
                }

                Debug.Log("ErrorPlacingTile: " + Tiles[x, y].HeightValue + " MountainHeight: " +
                          (Tiles[x, y].HeightValue <= MountainHeight));
            }

            Tilemap.SetTiles(positions, tileArray);

            foreach (Vector3Int borderTile in borderTiles)
            {
                Tilemap.SetTileFlags(borderTile, TileFlags.None);
                Tilemap.SetColor(borderTile, new Color(0.9f, 0.9f, 0.9f, 1f));
            }


            foreach (Vector3Int moistureTile in moistureAdjustedTiles)
            {
                Tilemap.SetTileFlags(moistureTile, TileFlags.None);
                if (Tiles[moistureTile.x, moistureTile.y].MoistureType == MoistureType.Dryest)
                    Tilemap.SetColor(moistureTile, new Color(248f / 255f, 164f / 255f, 6f / 255f));
                if (Tiles[moistureTile.x, moistureTile.y].MoistureType == MoistureType.Dryer)
                    Tilemap.SetColor(moistureTile, new Color(225f / 255f, 182f / 255f, 102f / 255f));
                if (Tiles[moistureTile.x, moistureTile.y].MoistureType == MoistureType.Dry)
                    Tilemap.SetColor(moistureTile, new Color(248f / 255f, 215f / 255f, 152f / 255f));
                if (Tiles[moistureTile.x, moistureTile.y].MoistureType == MoistureType.Wet)
                {
                    //Tilemap.SetColor(moistureTile, new Color(0.0f, 0.0f, 0.5f));
                }

                if (Tiles[moistureTile.x, moistureTile.y].MoistureType == MoistureType.Wetter)
                {
                    //Tilemap.SetColor(moistureTile, new Color(0.0f, 0.0f, 0.8f));
                }

                if (Tiles[moistureTile.x, moistureTile.y].MoistureType == MoistureType.Wettest)
                    Tilemap.SetColor(moistureTile, new Color(190f / 255f, 196f / 255f, 255f / 255f));
            }


            /*for (int x = -50; x <= 50; x++)
            {
                for (int y = -50; y <= 50; y++)
                {
                    //Generate Heightmap
                    int tyleType = Random.Range(0, 2);
                    switch (tyleType)
                    {
                        case 0:
                            Tilemap.SetTile(new Vector3Int(x,y,0), GrassTile);
                            break;
                        case 1:
                            Tilemap.SetTile(new Vector3Int(x, y, 0), DirtTile);
                            break;
                        default:
                            break;
                    }
                }
            }*/
        }


        private void GetData()
        {
            heightData = new MapData(Width, Height);
            heatData = new MapData(Width, Height);
            moistureData = new MapData(Width, Height);

            // loop through each x,y point - get height value
            for (var x = 0; x < Width; x++)
            for (var y = 0; y < Height; y++)
            {
                // WRAP ON BOTH AXIS
                // Noise range
                float x1 = 0, x2 = 2;
                float y1 = 0, y2 = 2;
                float dx = x2 - x1;
                float dy = y2 - y1;

                // Sample noise at smaller intervals
                float s = x / (float) Width;
                float t = y / (float) Height;

                // Calculate our 4D coordinates
                float nx = x1 + Mathf.Cos(s * 2 * Mathf.PI) * dx / (2 * Mathf.PI);
                float ny = y1 + Mathf.Cos(t * 2 * Mathf.PI) * dy / (2 * Mathf.PI);
                float nz = x1 + Mathf.Sin(s * 2 * Mathf.PI) * dx / (2 * Mathf.PI);
                float nw = y1 + Mathf.Sin(t * 2 * Mathf.PI) * dy / (2 * Mathf.PI);


                float heightValue = (float) heightMap.Get(nx, ny, nz, nw);
                float heatValue = (float) heatMap.Get(nx, ny, nz, nw);
                float moistureValue = (float) moistureMap.Get(nx, ny, nz, nw);

                // keep track of the max and min values found
                if (heightValue > heightData.Max) heightData.Max = heightValue;
                if (heightValue < heightData.Min) heightData.Min = heightValue;

                if (heatValue > heatData.Max) heatData.Max = heatValue;
                if (heatValue < heatData.Min) heatData.Min = heatValue;

                if (moistureValue > moistureData.Max) moistureData.Max = moistureValue;
                if (moistureValue < moistureData.Min) moistureData.Min = moistureValue;

                heightData.Data[x, y] = heightValue;
                heatData.Data[x, y] = heatValue;
                moistureData.Data[x, y] = moistureValue;
            }
        }

        private TileData GetTop(TileData t)
        {
            return Tiles[t.X, MathHelper.Mod(t.Y - 1, Height)];
        }

        private TileData GetBottom(TileData t)
        {
            return Tiles[t.X, MathHelper.Mod(t.Y + 1, Height)];
        }

        private TileData GetLeft(TileData t)
        {
            return Tiles[MathHelper.Mod(t.X - 1, Width), t.Y];
        }

        private TileData GetRight(TileData t)
        {
            return Tiles[MathHelper.Mod(t.X + 1, Width), t.Y];
        }

        private void UpdateNeighbors()
        {
            for (var x = 0; x < Width; x++)
            for (var y = 0; y < Height; y++)
            {
                TileData t = Tiles[x, y];

                t.Top = GetTop(t);
                t.Bottom = GetBottom(t);
                t.Left = GetLeft(t);
                t.Right = GetRight(t);
            }
        }

        private void UpdateBitmasks()
        {
            for (var x = 0; x < Width; x++)
            for (var y = 0; y < Height; y++)
                Tiles[x, y].UpdateBitmask();
        }

        // Extract data from a noise module
        /*private void GetData(ImplicitModuleBase module, ref MapData mapData)
        {
            mapData = new MapData(Width, Height);

            // loop through each x,y point - get height value
            for (var x = 0; x < Width; x++)
            {
                for (var y = 0; y < Height; y++)
                {
                    //Sample the noise at smaller intervals
                    float x1 = x / (float)Width;
                    float y1 = y / (float)Height;

                    float value = (float)_heightMap.Get(x1, y1);

                    //keep track of the max and min values found
                    if (value > mapData.Max) mapData.Max = value;
                    if (value < mapData.Min) mapData.Min = value;

                    mapData.Data[x, y] = value;
                }
            }
        }*/


        // Build a Tile array from our data
        /*private void LoadTiles()
        {
            Tiles = new TileData[Width, Height];

            for (var x = 0; x < Width; x++)
            {
                for (var y = 0; y < Height; y++)
                {
                    TileData t = new TileData();
                    t.X = x;
                    t.Y = y;

                    float value = HeightData.Data[x, y];

                    //normalize our value between 0 and 1
                    value = (value - HeightData.Min) / (HeightData.Max - HeightData.Min);

                    t.HeightValue = value;

                    Tiles[x, y] = t;
                }
            }
        }*/

        private void LoadTiles()
        {
            Tiles = new TileData[Width, Height];

            for (var x = 0; x < Width; x++)
            for (var y = 0; y < Height; y++)
            {
                TileData t = new TileData();
                t.X = x;
                t.Y = y;

                float value = heightData.Data[x, y];
                value = (value - heightData.Min) / (heightData.Max - heightData.Min);

                t.HeightValue = value;

                //HeightMap Analyze
                if (value < DeepWaterHeight)
                {
                    t.HeightType = HeightType.DeepWater;
                    t.Collidable = false;
                }
                else if (value < WaterHeight)
                {
                    t.HeightType = HeightType.ShallowWater;
                    t.Collidable = false;
                }
                else if (value < WetSandHeight)
                {
                    t.HeightType = HeightType.WetSand;
                    t.Collidable = true;
                }
                else if (value < SandHeight)
                {
                    t.HeightType = HeightType.Sand;
                    t.Collidable = true;
                }
                else if (value < GrassHeight)
                {
                    t.HeightType = HeightType.Grass;
                    t.Collidable = true;
                }
                else if (value < ForestHeight)
                {
                    t.HeightType = HeightType.Forest;
                    t.Collidable = true;
                }
                else if (value < DirtHeight)
                {
                    t.HeightType = HeightType.Dirt;
                    t.Collidable = true;
                }
                else if (value < MountainHeight)
                {
                    t.HeightType = HeightType.Mountain;
                    t.Collidable = true;
                }
                else
                {
                    t.HeightType = HeightType.Snow;
                    t.Collidable = true;
                }

                //adjust moisture based on height
                if (t.HeightType == HeightType.DeepWater)
                    moistureData.Data[t.X, t.Y] += 8f * t.HeightValue;
                else if (t.HeightType == HeightType.ShallowWater)
                    moistureData.Data[t.X, t.Y] += 3f * t.HeightValue;
                else if (t.HeightType == HeightType.WetSand)
                    moistureData.Data[t.X, t.Y] += 1f * t.HeightValue;
                else if (t.HeightType == HeightType.Sand) moistureData.Data[t.X, t.Y] += 0.2f * t.HeightValue;

                //Moisture Map Analyze	
                float moistureValue = moistureData.Data[x, y];
                moistureValue = (moistureValue - moistureData.Min) / (moistureData.Max - moistureData.Min);
                t.MoistureValue = moistureValue;

                //set moisture type
                if (moistureValue < DryerValue) t.MoistureType = MoistureType.Dryest;
                else if (moistureValue < DryValue) t.MoistureType = MoistureType.Dryer;
                else if (moistureValue < WetValue) t.MoistureType = MoistureType.Dry;
                else if (moistureValue < WetterValue) t.MoistureType = MoistureType.Wet;
                else if (moistureValue < WettestValue) t.MoistureType = MoistureType.Wetter;
                else t.MoistureType = MoistureType.Wettest;


                // Adjust Heat Map based on Height - Higher == colder
                if (t.HeightType == HeightType.Forest)
                    heatData.Data[t.X, t.Y] -= 0.1f * t.HeightValue;
                else if (t.HeightType == HeightType.Mountain)
                    heatData.Data[t.X, t.Y] -= 0.25f * t.HeightValue;
                else if (t.HeightType == HeightType.Snow)
                    heatData.Data[t.X, t.Y] -= 0.4f * t.HeightValue;
                else
                    heatData.Data[t.X, t.Y] += 0.01f * t.HeightValue;

                // Set heat value
                float heatValue = heatData.Data[x, y];
                heatValue = (heatValue - heatData.Min) / (heatData.Max - heatData.Min);
                t.HeatValue = heatValue;

                // set heat type
                if (heatValue < ColdestValue) t.HeatType = HeatType.Coldest;
                else if (heatValue < ColderValue) t.HeatType = HeatType.Colder;
                else if (heatValue < ColdValue) t.HeatType = HeatType.Cold;
                else if (heatValue < WarmValue) t.HeatType = HeatType.Warm;
                else if (heatValue < WarmerValue) t.HeatType = HeatType.Warmer;
                else t.HeatType = HeatType.Warmest;

                Tiles[x, y] = t;
            }
        }

        private void Initialize()
        {
            // Initialize the HeightMap Generator
            heightMap = new ImplicitFractal(FractalType.MULTI,
                BasisType.SIMPLEX,
                InterpolationType.QUINTIC,
                TerrainOctaves,
                TerrainFrequency,
                Seed);


            // Initialize the Heat map
            ImplicitGradient gradient = new ImplicitGradient(1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1);
            ImplicitFractal heatFractal = new ImplicitFractal(FractalType.MULTI,
                BasisType.SIMPLEX,
                InterpolationType.QUINTIC,
                HeatOctaves,
                HeatFrequency,
                Seed);

            heatMap = new ImplicitCombiner(CombinerType.MULTIPLY);
            heatMap.AddSource(gradient);
            heatMap.AddSource(heatFractal);

            //moisture map
            moistureMap = new ImplicitFractal(FractalType.MULTI,
                BasisType.SIMPLEX,
                InterpolationType.QUINTIC,
                MoistureOctaves,
                MoistureFrequency,
                Seed);
        }

        private void AddMoisture(TileData t, int radius)
        {
            int startx = MathHelper.Mod(t.X - radius, Width);
            int endx = MathHelper.Mod(t.X + radius, Width);
            Vector2 center = new Vector2(t.X, t.Y);
            int curr = radius;

            while (curr > 0)
            {
                int x1 = MathHelper.Mod(t.X - curr, Width);
                int x2 = MathHelper.Mod(t.X + curr, Width);
                int y = t.Y;

                AddMoisture(Tiles[x1, y], 0.025f / (center - new Vector2(x1, y)).magnitude);

                for (int i = 0; i < curr; i++)
                {
                    AddMoisture(Tiles[x1, MathHelper.Mod(y + i + 1, Height)],
                        0.025f / (center - new Vector2(x1, MathHelper.Mod(y + i + 1, Height))).magnitude);
                    AddMoisture(Tiles[x1, MathHelper.Mod(y - (i + 1), Height)],
                        0.025f / (center - new Vector2(x1, MathHelper.Mod(y - (i + 1), Height))).magnitude);

                    AddMoisture(Tiles[x2, MathHelper.Mod(y + i + 1, Height)],
                        0.025f / (center - new Vector2(x2, MathHelper.Mod(y + i + 1, Height))).magnitude);
                    AddMoisture(Tiles[x2, MathHelper.Mod(y - (i + 1), Height)],
                        0.025f / (center - new Vector2(x2, MathHelper.Mod(y - (i + 1), Height))).magnitude);
                }

                curr--;
            }
        }

        private void AddMoisture(TileData t, float amount)
        {
            moistureData.Data[t.X, t.Y] += amount;
            t.MoistureValue += amount;
            if (t.MoistureValue > 1)
                t.MoistureValue = 1;

            //set moisture type
            if (t.MoistureValue < DryerValue) t.MoistureType = MoistureType.Dryest;
            else if (t.MoistureValue < DryValue) t.MoistureType = MoistureType.Dryer;
            else if (t.MoistureValue < WetValue) t.MoistureType = MoistureType.Dry;
            else if (t.MoistureValue < WetterValue) t.MoistureType = MoistureType.Wet;
            else if (t.MoistureValue < WettestValue) t.MoistureType = MoistureType.Wetter;
            else t.MoistureType = MoistureType.Wettest;
        }

        private void AdjustMoistureMap()
        {
            for (var x = 0; x < Width; x++)
            for (var y = 0; y < Height; y++)
            {
                TileData t = Tiles[x, y];
                if (t.HeightType == HeightType.River) AddMoisture(t, 60);
            }
        }

        private void DigRiverGroups()
        {
            for (int i = 0; i < riverGroups.Count; i++)
            {
                RiverGroup group = riverGroups[i];
                River longest = null;

                //Find longest river in this group
                for (int j = 0; j < group.Rivers.Count; j++)
                {
                    River river = group.Rivers[j];
                    if (longest == null)
                        longest = river;
                    else if (longest.Tiles.Count < river.Tiles.Count)
                        longest = river;
                }

                if (longest != null)
                {
                    //Dig out longest path first
                    DigRiver(longest);

                    for (int j = 0; j < group.Rivers.Count; j++)
                    {
                        River river = group.Rivers[j];
                        if (river != longest) DigRiver(river, longest);
                    }
                }
            }
        }

        private void BuildRiverGroups()
        {
            //loop each tile, checking if it belongs to multiple rivers
            for (var x = 0; x < Width; x++)
            for (var y = 0; y < Height; y++)
            {
                TileData t = Tiles[x, y];

                if (t.Rivers.Count > 1)
                {
                    // multiple rivers == intersection
                    RiverGroup group = null;

                    // Does a rivergroup already exist for this group?
                    for (int n = 0; n < t.Rivers.Count; n++)
                    {
                        River tileriver = t.Rivers[n];
                        for (int i = 0; i < riverGroups.Count; i++)
                        {
                            for (int j = 0; j < riverGroups[i].Rivers.Count; j++)
                            {
                                River river = riverGroups[i].Rivers[j];
                                if (river.ID == tileriver.ID) group = riverGroups[i];
                                if (group != null) break;
                            }

                            if (group != null) break;
                        }

                        if (group != null) break;
                    }

                    // existing group found -- add to it
                    if (group != null)
                    {
                        for (int n = 0; n < t.Rivers.Count; n++)
                            if (!group.Rivers.Contains(t.Rivers[n]))
                                group.Rivers.Add(t.Rivers[n]);
                    }
                    else //No existing group found - create a new one
                    {
                        group = new RiverGroup();
                        for (int n = 0; n < t.Rivers.Count; n++) group.Rivers.Add(t.Rivers[n]);
                        riverGroups.Add(group);
                    }
                }
            }
        }

        private void GenerateRivers()
        {
            int attempts = 0;
            int rivercount = RiverCount;
            rivers = new List<River>();

            // Generate some rivers
            while (rivercount > 0 && attempts < MaxRiverAttempts)
            {
                // Get a random tile
                int x = Random.Range(0, Width);
                int y = Random.Range(0, Height);
                TileData tile = Tiles[x, y];

                // validate the tile
                if (!tile.Collidable) continue;
                if (tile.Rivers.Count > 0) continue;

                if (tile.HeightValue > MinRiverHeight)
                {
                    // Tile is good to start river from
                    River river = new River(rivercount);

                    // Figure out the direction this river will try to flow
                    river.CurrentDirection = tile.GetLowestNeighbor();

                    // Recursively find a path to water
                    FindPathToWater(tile, river.CurrentDirection, ref river);

                    // Validate the generated river 
                    if (river.TurnCount < MinRiverTurns || river.Tiles.Count < MinRiverLength ||
                        river.Intersections > MaxRiverIntersections)
                    {
                        //Validation failed - remove this river
                        for (int i = 0; i < river.Tiles.Count; i++)
                        {
                            TileData t = river.Tiles[i];
                            t.Rivers.Remove(river);
                        }
                    }
                    else if (river.Tiles.Count >= MinRiverLength)
                    {
                        //Validation passed - Add river to list
                        rivers.Add(river);
                        tile.Rivers.Add(river);
                        rivercount--;
                    }
                }

                attempts++;
            }
        }

        // Dig river based on a parent river vein
        private void DigRiver(River river, River parent)
        {
            int intersectionId = 0;
            int intersectionSize = 0;

            // determine point of intersection
            for (int i = 0; i < river.Tiles.Count; i++)
            {
                TileData t1 = river.Tiles[i];
                for (int j = 0; j < parent.Tiles.Count; j++)
                {
                    TileData t2 = parent.Tiles[j];
                    if (t1 == t2)
                    {
                        intersectionId = i;
                        intersectionSize = t2.RiverSize;
                    }
                }
            }

            int counter = 0;
            int intersectionCount = river.Tiles.Count - intersectionId;
            int size = Random.Range(intersectionSize, 5);
            river.Length = river.Tiles.Count;

            // randomize size change
            int two = river.Length / 2;
            int three = two / 2;
            int four = three / 2;
            int five = four / 2;

            int twomin = two / 3;
            int threemin = three / 3;
            int fourmin = four / 3;
            int fivemin = five / 3;

            // randomize length of each size
            int count1 = Random.Range(fivemin, five);
            if (size < 4) count1 = 0;
            int count2 = count1 + Random.Range(fourmin, four);
            if (size < 3)
            {
                count2 = 0;
                count1 = 0;
            }

            int count3 = count2 + Random.Range(threemin, three);
            if (size < 2)
            {
                count3 = 0;
                count2 = 0;
                count1 = 0;
            }

            int count4 = count3 + Random.Range(twomin, two);

            // Make sure we are not digging past the river path
            if (count4 > river.Length)
            {
                int extra = count4 - river.Length;
                while (extra > 0)
                    if (count1 > 0)
                    {
                        count1--;
                        count2--;
                        count3--;
                        count4--;
                        extra--;
                    }
                    else if (count2 > 0)
                    {
                        count2--;
                        count3--;
                        count4--;
                        extra--;
                    }
                    else if (count3 > 0)
                    {
                        count3--;
                        count4--;
                        extra--;
                    }
                    else if (count4 > 0)
                    {
                        count4--;
                        extra--;
                    }
            }

            // adjust size of river at intersection point
            if (intersectionSize == 1)
            {
                count4 = intersectionCount;
                count1 = 0;
                count2 = 0;
                count3 = 0;
            }
            else if (intersectionSize == 2)
            {
                count3 = intersectionCount;
                count1 = 0;
                count2 = 0;
            }
            else if (intersectionSize == 3)
            {
                count2 = intersectionCount;
                count1 = 0;
            }
            else if (intersectionSize == 4)
            {
                count1 = intersectionCount;
            }
            else
            {
                count1 = 0;
                count2 = 0;
                count3 = 0;
                count4 = 0;
            }

            // dig out the river
            for (int i = river.Tiles.Count - 1; i >= 0; i--)
            {
                TileData t = river.Tiles[i];

                if (counter < count1)
                    t.DigRiver(river, 4);
                else if (counter < count2)
                    t.DigRiver(river, 3);
                else if (counter < count3)
                    t.DigRiver(river, 2);
                else if (counter < count4)
                    t.DigRiver(river, 1);
                else
                    t.DigRiver(river, 0);
                counter++;
            }
        }

        // Dig river
        private void DigRiver(River river)
        {
            int counter = 0;

            // How wide are we digging this river?
            int size = Random.Range(1, 5);
            river.Length = river.Tiles.Count;

            // randomize size change
            int two = river.Length / 2;
            int three = two / 2;
            int four = three / 2;
            int five = four / 2;

            int twomin = two / 3;
            int threemin = three / 3;
            int fourmin = four / 3;
            int fivemin = five / 3;

            // randomize lenght of each size
            int count1 = Random.Range(fivemin, five);
            if (size < 4) count1 = 0;
            int count2 = count1 + Random.Range(fourmin, four);
            if (size < 3)
            {
                count2 = 0;
                count1 = 0;
            }

            int count3 = count2 + Random.Range(threemin, three);
            if (size < 2)
            {
                count3 = 0;
                count2 = 0;
                count1 = 0;
            }

            int count4 = count3 + Random.Range(twomin, two);

            // Make sure we are not digging past the river path
            if (count4 > river.Length)
            {
                int extra = count4 - river.Length;
                while (extra > 0)
                    if (count1 > 0)
                    {
                        count1--;
                        count2--;
                        count3--;
                        count4--;
                        extra--;
                    }
                    else if (count2 > 0)
                    {
                        count2--;
                        count3--;
                        count4--;
                        extra--;
                    }
                    else if (count3 > 0)
                    {
                        count3--;
                        count4--;
                        extra--;
                    }
                    else if (count4 > 0)
                    {
                        count4--;
                        extra--;
                    }
            }

            // Dig it out
            for (int i = river.Tiles.Count - 1; i >= 0; i--)
            {
                TileData t = river.Tiles[i];

                if (counter < count1)
                    t.DigRiver(river, 4);
                else if (counter < count2)
                    t.DigRiver(river, 3);
                else if (counter < count3)
                    t.DigRiver(river, 2);
                else if (counter < count4)
                    t.DigRiver(river, 1);
                else
                    t.DigRiver(river, 0);
                counter++;
            }
        }

        private void FindPathToWater(TileData tile, Direction direction, ref River river)
        {
            if (tile.Rivers.Contains(river))
                return;

            // check if there is already a river on this tile
            if (tile.Rivers.Count > 0)
                river.Intersections++;

            river.AddTile(tile);

            // get neighbors
            TileData left = GetLeft(tile);
            TileData right = GetRight(tile);
            TileData top = GetTop(tile);
            TileData bottom = GetBottom(tile);

            float leftValue = int.MaxValue;
            float rightValue = int.MaxValue;
            float topValue = int.MaxValue;
            float bottomValue = int.MaxValue;

            // query height values of neighbors
            if (left.GetRiverNeighborCount(river) < 2 && !river.Tiles.Contains(left))
                leftValue = left.HeightValue;
            if (right.GetRiverNeighborCount(river) < 2 && !river.Tiles.Contains(right))
                rightValue = right.HeightValue;
            if (top.GetRiverNeighborCount(river) < 2 && !river.Tiles.Contains(top))
                topValue = top.HeightValue;
            if (bottom.GetRiverNeighborCount(river) < 2 && !river.Tiles.Contains(bottom))
                bottomValue = bottom.HeightValue;

            // if neighbor is existing river that is not this one, flow into it
            if (bottom.Rivers.Count == 0 && !bottom.Collidable)
                bottomValue = 0;
            if (top.Rivers.Count == 0 && !top.Collidable)
                topValue = 0;
            if (left.Rivers.Count == 0 && !left.Collidable)
                leftValue = 0;
            if (right.Rivers.Count == 0 && !right.Collidable)
                rightValue = 0;

            // override flow direction if a tile is significantly lower
            if (direction == Direction.Left)
                if (Mathf.Abs(rightValue - leftValue) < 0.1f)
                    rightValue = int.MaxValue;
            if (direction == Direction.Right)
                if (Mathf.Abs(rightValue - leftValue) < 0.1f)
                    leftValue = int.MaxValue;
            if (direction == Direction.Top)
                if (Mathf.Abs(topValue - bottomValue) < 0.1f)
                    bottomValue = int.MaxValue;
            if (direction == Direction.Bottom)
                if (Mathf.Abs(topValue - bottomValue) < 0.1f)
                    topValue = int.MaxValue;

            // find mininum
            float min = Mathf.Min(Mathf.Min(Mathf.Min(leftValue, rightValue), topValue), bottomValue);

            // if no minimum found - exit
            if (min == int.MaxValue)
                return;

            //Move to next neighbor
            if (min == leftValue)
            {
                if (left.Collidable)
                {
                    if (river.CurrentDirection != Direction.Left)
                    {
                        river.TurnCount++;
                        river.CurrentDirection = Direction.Left;
                    }

                    FindPathToWater(left, direction, ref river);
                }
            }
            else if (min == rightValue)
            {
                if (right.Collidable)
                {
                    if (river.CurrentDirection != Direction.Right)
                    {
                        river.TurnCount++;
                        river.CurrentDirection = Direction.Right;
                    }

                    FindPathToWater(right, direction, ref river);
                }
            }
            else if (min == bottomValue)
            {
                if (bottom.Collidable)
                {
                    if (river.CurrentDirection != Direction.Bottom)
                    {
                        river.TurnCount++;
                        river.CurrentDirection = Direction.Bottom;
                    }

                    FindPathToWater(bottom, direction, ref river);
                }
            }
            else if (min == topValue)
            {
                if (top.Collidable)
                {
                    if (river.CurrentDirection != Direction.Top)
                    {
                        river.TurnCount++;
                        river.CurrentDirection = Direction.Top;
                    }

                    FindPathToWater(top, direction, ref river);
                }
            }
        }

        private void FloodFill()
        {
            // Use a stack instead of recursion
            Stack<TileData> stack = new Stack<TileData>();

            for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
            {
                TileData t = Tiles[x, y];

                //Tile already flood filled, skip
                if (t.FloodFilled) continue;

                // Land
                if (t.Collidable)
                {
                    TileGroup group = new TileGroup();
                    group.Type = TileGroupType.Land;
                    stack.Push(t);

                    while (stack.Count > 0) FloodFill(stack.Pop(), ref group, ref stack);

                    if (group.Tiles.Count > 0)
                        lands.Add(group);
                }
                // Water
                else
                {
                    TileGroup group = new TileGroup();
                    group.Type = TileGroupType.Water;
                    stack.Push(t);

                    while (stack.Count > 0) FloodFill(stack.Pop(), ref group, ref stack);

                    if (group.Tiles.Count > 0)
                        waters.Add(group);
                }
            }
        }


        private void FloodFill(TileData tile, ref TileGroup tiles, ref Stack<TileData> stack)
        {
            // Validate
            if (tile.FloodFilled)
                return;
            if (tiles.Type == TileGroupType.Land && !tile.Collidable)
                return;
            if (tiles.Type == TileGroupType.Water && tile.Collidable)
                return;

            // Add to TileGroup
            tiles.Tiles.Add(tile);
            tile.FloodFilled = true;

            // floodfill into neighbors
            TileData t = GetTop(tile);
            if (!t.FloodFilled && tile.Collidable == t.Collidable)
                stack.Push(t);
            t = GetBottom(tile);
            if (!t.FloodFilled && tile.Collidable == t.Collidable)
                stack.Push(t);
            t = GetLeft(tile);
            if (!t.FloodFilled && tile.Collidable == t.Collidable)
                stack.Push(t);
            t = GetRight(tile);
            if (!t.FloodFilled && tile.Collidable == t.Collidable)
                stack.Push(t);
        }
    }
}