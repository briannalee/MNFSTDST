using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts._3rdparty;
using Assets.Scripts._3rdparty.AccidentalNoise.Enums;
using Assets.Scripts._3rdparty.AccidentalNoise.Implicit;
using Assets.Scripts.Rivers;
using Assets.Scripts.Sprites;
using Pathfinding;
using UnityEngine;

namespace Assets.Scripts.World
{
    public class WorldData : WorldBase
    {
        private ImplicitFractal heightMap;
        private ImplicitCombiner heatMap;
        private ImplicitFractal moistureMap;
        public TerrainTile[,] TerrainTileMap;
        private MapData heightData;
        private MapData heatData;
        private MapData moistureData;
        
        public List<RiverGroup> RiverGroups = new List<RiverGroup>();
        public List<River> Rivers = new List<River>();
        public List<TileGroup> Waters = new List<TileGroup>();
        public List<TileGroup> Lands = new List<TileGroup>();

        public WorldData(int width, int height) : base(width, height)
        {
            Initialize();
            GetData();
            LoadTiles();

            UpdateNeighbors();
            
            RiverHelper.GenerateRiverData(this);
            RiverHelper.BuildRiverGroups(this);
            RiverHelper.DigRiverGroups(this);
            AdjustMoistureMap();

            UpdateBitmasks();
            FloodFill();
            GenerateBiomeMap();
            GridGraph gridGraph = AstarPath.active.data.gridGraph;
            gridGraph.GetNodes(node => gridGraph.CalculateConnections((GridNodeBase)node));
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
                float s = x / (float)Width;
                float t = y / (float)Height;

                // Calculate our 4D coordinates
                float nx = x1 + Mathf.Cos(s * 2 * Mathf.PI) * dx / (2 * Mathf.PI);
                float ny = y1 + Mathf.Cos(t * 2 * Mathf.PI) * dy / (2 * Mathf.PI);
                float nz = x1 + Mathf.Sin(s * 2 * Mathf.PI) * dx / (2 * Mathf.PI);
                float nw = y1 + Mathf.Sin(t * 2 * Mathf.PI) * dy / (2 * Mathf.PI);


                float heightValue = (float)heightMap.Get(nx, ny, nz, nw);
                float heatValue = (float)heatMap.Get(nx, ny, nz, nw);
                float moistureValue = (float)moistureMap.Get(nx, ny, nz, nw);

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

        public TerrainTile GetTop(TerrainTile t)
        {
            return TerrainTileMap[t.X, MathHelper.Mod(t.Y - 1, Height)];
        }

        public TerrainTile GetBottom(TerrainTile t)
        {
            return TerrainTileMap[t.X, MathHelper.Mod(t.Y + 1, Height)];
        }

        public TerrainTile GetLeft(TerrainTile t)
        {
            return TerrainTileMap[MathHelper.Mod(t.X - 1, Width), t.Y];
        }

        public TerrainTile GetRight(TerrainTile t)
        {
            return TerrainTileMap[MathHelper.Mod(t.X + 1, Width), t.Y];
        }

        private void UpdateNeighbors()
        {
            for (var x = 0; x < Width; x++)
            for (var y = 0; y < Height; y++)
            {
                TerrainTile t = TerrainTileMap[x, y];

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
                TerrainTileMap[x, y].UpdateBitmask();
        }


        private void LoadTiles()
        {
            TerrainTileMap = new TerrainTile[Width, Height];
            GridGraph gridGraph = AstarPath.active.data.gridGraph;

            for (var x = 0; x < Width; x++)
            for (var y = 0; y < Height; y++)
            {
                GridNodeBase node = gridGraph.GetNode(x, y);

                /*Wrap node connections around the map, if edge tile
                if (x < 1)
                {
                    GridNodeBase oppositeNode = gridGraph.GetNode(Width-1, y);
                    node.AddConnection(oppositeNode,1000);
                    oppositeNode.AddConnection(node, 1000);
                }
                if (y < 1)
                {
                    GridNodeBase oppositeNode = gridGraph.GetNode(x, Height-1);
                    node.AddConnection(oppositeNode, 1000);
                    oppositeNode.AddConnection(node, 1000);
                }*/
                
                TerrainTile t = ScriptableObject.CreateInstance<TerrainTile>();
                t.X = x;
                t.Y = y;
                //TileData t = new TileData {X = x, Y = y};

                float value = heightData.Data[x, y];
                value = (value - heightData.Min) / (heightData.Max - heightData.Min);

                t.HeightValue = value;

                //HeightMap Analyze
                if (value < DeepWater)
                {
                    t.HeightType = HeightType.DeepWater;
                    t.Collidable = false;
                    node.Walkable = false;
                }
                else if (value < ShallowWater)
                {
                    t.HeightType = HeightType.ShallowWater;
                    t.Collidable = false;
                    node.Walkable = false;
                }
                else if (value < Shore)
                {
                    t.HeightType = HeightType.WetSand;
                    t.Collidable = true;
                    node.Penalty = 1500;
                }
                else if (value < Sand)
                {
                    t.HeightType = HeightType.Sand;
                    t.Collidable = true;
                }
                else if (value < Dirt)
                {
                    t.HeightType = HeightType.Dirt;
                    t.Collidable = true;
                }
                else if (value < Grass)
                {
                    t.HeightType = HeightType.Grass;
                    t.Collidable = true;
                }
                else if (value < Forest)
                {
                    t.HeightType = HeightType.Forest;
                    t.Collidable = true;
                    node.Penalty = 2000;
                }
                else if (value < Mountain)
                {
                    t.HeightType = HeightType.Mountain;
                    t.Collidable = true;
                    node.Walkable = true;
                    node.Penalty = 5000;
                }
                else
                {
                    t.HeightType = HeightType.Snow;
                    t.Collidable = false;
                    node.Walkable = false;
                    node.Penalty = 6000;
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
                if (moistureValue < DryerValue) {t.MoistureType = MoistureType.Dryest;}
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

                //Cold+Wet Areas = Snow
                if (t.HeatType == HeatType.Coldest && t.MoistureType == MoistureType.Wettest)
                    t.HeightType = HeightType.Snow;

                TerrainTileMap[x, y] = t;
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

        private void AddMoisture(TerrainTile t, int radius)
        {
            Vector2 center = new Vector2(t.X, t.Y);
            int index = radius;

            while (index > 0)
            {
                int x1 = MathHelper.Mod(t.X - index, Width);
                int x2 = MathHelper.Mod(t.X + index, Width);
                int y = t.Y;

                AddMoisture(TerrainTileMap[x1, y], 0.025f / (center - new Vector2(x1, y)).magnitude);

                for (int i = 0; i < index; i++)
                {
                    AddMoisture(TerrainTileMap[x1, MathHelper.Mod(y + i + 1, Height)],
                        0.025f / (center - new Vector2(x1, MathHelper.Mod(y + i + 1, Height))).magnitude);
                    AddMoisture(TerrainTileMap[x1, MathHelper.Mod(y - (i + 1), Height)],
                        0.025f / (center - new Vector2(x1, MathHelper.Mod(y - (i + 1), Height))).magnitude);

                    AddMoisture(TerrainTileMap[x2, MathHelper.Mod(y + i + 1, Height)],
                        0.025f / (center - new Vector2(x2, MathHelper.Mod(y + i + 1, Height))).magnitude);
                    AddMoisture(TerrainTileMap[x2, MathHelper.Mod(y - (i + 1), Height)],
                        0.025f / (center - new Vector2(x2, MathHelper.Mod(y - (i + 1), Height))).magnitude);
                }

                index--;
            }
        }

        private void AddMoisture(TerrainTile t, float amount)
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
                    TerrainTile t = TerrainTileMap[x, y];
                    if (t.HeightType == HeightType.River) AddMoisture(t, 60);
                }
        }

        // Dig river based on a parent river vein

        // Dig river

        private void FloodFill()
        {
            // Use a stack instead of recursion
            Stack<TerrainTile> stack = new Stack<TerrainTile>();

            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {
                    TerrainTile t = TerrainTileMap[x, y];

                    //Tile already flood filled, skip
                    if (t.FloodFilled) continue;

                    // Land
                    if (t.Collidable)
                    {
                        TileGroup tileGroup = new TileGroup();
                        tileGroup.Type = TileGroupType.Land;
                        stack.Push(t);

                        while (stack.Count > 0) FloodFill(stack.Pop(), ref tileGroup, ref stack);

                        if (tileGroup.Tiles.Count > 0)
                            Lands.Add(tileGroup);
                    }
                    // Water
                    else
                    {
                        TileGroup group = new TileGroup();
                        group.Type = TileGroupType.Water;
                        stack.Push(t);

                        while (stack.Count > 0) FloodFill(stack.Pop(), ref group, ref stack);

                        if (group.Tiles.Count > 0)
                            Waters.Add(group);
                    }
                }
        }


        private void FloodFill(TerrainTile terrainTile, ref TileGroup tiles, ref Stack<TerrainTile> stack)
        {
            // Validate
            if (terrainTile.FloodFilled)
                return;
            if (tiles.Type == TileGroupType.Land && !terrainTile.Collidable)
                return;
            if (tiles.Type == TileGroupType.Water && terrainTile.Collidable)
                return;

            // Add to TileGroup
            tiles.Tiles.Add(terrainTile);
            terrainTile.FloodFilled = true;

            // FloodFill into neighbors
            TerrainTile t = GetTop(terrainTile);
            if (!t.FloodFilled && terrainTile.Collidable == t.Collidable)
                stack.Push(t);
            t = GetBottom(terrainTile);
            if (!t.FloodFilled && terrainTile.Collidable == t.Collidable)
                stack.Push(t);
            t = GetLeft(terrainTile);
            if (!t.FloodFilled && terrainTile.Collidable == t.Collidable)
                stack.Push(t);
            t = GetRight(terrainTile);
            if (!t.FloodFilled && terrainTile.Collidable == t.Collidable)
                stack.Push(t);
        }

        private void GenerateBiomeMap()
        {
            for (var x = 0; x < Width; x++)
            {
                for (var y = 0; y < Height; y++)
                {

                    if (!TerrainTileMap[x, y].Collidable) continue;

                    TerrainTile t = TerrainTileMap[x, y];
                    t.BiomeType = GetBiomeType(t);
                }
            }
        }

        public BiomeType GetBiomeType(TerrainTile terrainTile)
        {
            return BiomeTable[(int)terrainTile.MoistureType, (int)terrainTile.HeatType];
        }
    }

    public static class TileArray<T>
    {
        public static T[] GetColumn(T[,] matrix, int columnNumber)
        {
            return Enumerable.Range(0, matrix.GetLength(0))
                .Select(x => matrix[x, columnNumber])
                .ToArray();
        }

        public static T[] GetRow(T[,] matrix, int rowNumber)
        {
            return Enumerable.Range(0, matrix.GetLength(1))
                .Select(x => matrix[rowNumber, x])
                .ToArray();
        }
    }
}
