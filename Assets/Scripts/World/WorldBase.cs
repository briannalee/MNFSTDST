using UnityEngine;

namespace Assets.Scripts.World
{
    public class WorldBase
    {
        // Map Size
        public int Width;
        public int Height;

        // Map Seed
        public int Seed;

        // Height Map Settings
        public int TerrainOctaves = 6;
        public double TerrainFrequency = 1.25;
        public float DeepWater = 0.2f;
        public float ShallowWater = 0.4f;
        public float Shore = 0.49f;
        public float Sand = 0.5f;
        public float Dirt = 0.55f;
        public float Grass = 0.7f;
        public float Forest = 0.8f;
        public float Mountain = 0.9f;
        public float Snow = 0.95f;

        // Heat Map Settings
        public int HeatOctaves = 4;
        public double HeatFrequency = 3.0;
        public float ColdestValue = 0.05f;
        public float ColderValue = 0.18f;
        public float ColdValue = 0.4f;
        public float WarmValue = 0.6f;
        public float WarmerValue = 0.8f;

        // Moisture Map Settings
        public int MoistureOctaves = 4;
        public double MoistureFrequency = 3.0;
        public float DryerValue = 0.27f;
        public float DryValue = 0.4f;
        public float WetValue = 0.6f;
        public float WetterValue = 0.8f;
        public float WettestValue = 0.9f;

        // River Settings
        public int RiverCount = 40;
        public float MinRiverHeight = 0.6f;
        public int MaxRiverAttempts = 1000;
        public int MinRiverTurns = 18;
        public int MinRiverLength = 20;
        public int MaxRiverIntersections = 2;

        //Biome Types
        public BiomeType[,] BiomeTable = {   
            //COLDEST        //COLDER          //COLD                  //HOT                          //HOTTER                       //HOTTEST
            { BiomeType.Ice, BiomeType.Tundra, BiomeType.Grassland,    BiomeType.Desert,              BiomeType.Desert,              BiomeType.Desert },              //DRYEST
            { BiomeType.Ice, BiomeType.Tundra, BiomeType.Grassland,    BiomeType.Desert,              BiomeType.Desert,              BiomeType.Desert },              //DRYER
            { BiomeType.Ice, BiomeType.Tundra, BiomeType.Woodland,     BiomeType.Woodland,            BiomeType.Savanna,             BiomeType.Savanna },             //DRY
            { BiomeType.Ice, BiomeType.Tundra, BiomeType.BorealForest, BiomeType.Woodland,            BiomeType.Savanna,             BiomeType.Savanna },             //WET
            { BiomeType.Ice, BiomeType.Tundra, BiomeType.BorealForest, BiomeType.SeasonalForest,      BiomeType.TropicalRainforest,  BiomeType.TropicalRainforest },  //WETTER
            { BiomeType.Ice, BiomeType.Tundra, BiomeType.BorealForest, BiomeType.TemperateRainforest, BiomeType.TropicalRainforest,  BiomeType.TropicalRainforest }   //WETTEST
        };

        public WorldBase(int width, int height)
        {
            Width = width;
            Height = height;
        }
    }
}
