using System;
using Assets.Scripts.World;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Rivers
{
    public class RiverHelper
    {

        public static void DigRiverGroups(WorldData worldData)
        {
            foreach (RiverGroup riverGroup in worldData.RiverGroups)
            {
                River longest = null;

                //Find longest river in this group
                foreach (var river in riverGroup.Rivers)
                {
                    if (longest == null)
                        longest = river;
                    else if (longest.Tiles.Count < river.Tiles.Count)
                        longest = river;
                }

                //If there was no rivers in this group, skip
                if (longest == null) continue;
                
                //Dig out longest path first
                DigRiver(longest);

                foreach (var river in riverGroup.Rivers)
                {
                    if (river != longest) DigRiver(river, longest);
                }
            }
        }

        public static void BuildRiverGroups(WorldData worldData)
        {
            //loop each tile, checking if it belongs to multiple rivers
            for (var x = 0; x < worldData.Width; x++)
            for (var y = 0; y < worldData.Height; y++)
            {
                TerrainTile t = worldData.TerrainTileMap[x, y];

                //Do not continue if there are no rivers
                if (t.Rivers.Count <= 1) continue;

                // multiple rivers == intersection
                RiverGroup riverGroup = null;
                        
                // Does a riverGroup already exist for this group?
                foreach (var tileRiver in t.Rivers)
                {
                    foreach (var t1 in worldData.RiverGroups)
                    {
                        foreach (var river in t1.Rivers)
                        {
                            if (river.ID == tileRiver.ID) riverGroup = t1;
                            if (riverGroup != null) break;
                        }
                        if (riverGroup != null) break;
                    }
                    if (riverGroup != null) break;
                }

                // existing group found -- add to it
                if (riverGroup != null)
                {
                    foreach (var t1 in t.Rivers)
                        if (!riverGroup.Rivers.Contains(t1)) riverGroup.Rivers.Add(t1);
                            
                }
                else //No existing group found - create a new one
                {
                    riverGroup = new RiverGroup();
                    foreach (var t1 in t.Rivers) riverGroup.Rivers.Add(t1);
                    worldData.RiverGroups.Add(riverGroup);
                }
            }
        }

        public static void GenerateRiverData(WorldData worldData)
        {
            int attempts = 0;
            int riverCount = worldData.RiverCount;

            // Generate some rivers
            while (riverCount > 0 && attempts < worldData.MaxRiverAttempts)
            {
                // Get a random tile
                int x = Random.Range(0, worldData.Width);
                int y = Random.Range(0, worldData.Height);
                TerrainTile terrainTile = worldData.TerrainTileMap[x, y];

                // validate the tile
                if (!terrainTile.Collidable) continue;
                if (terrainTile.Rivers.Count > 0) continue;
                attempts++;

                //if tile is higher than the minimum river height, skip
                if (!(terrainTile.HeightValue > worldData.MinRiverHeight)) continue;

                // Tile is good to start river from
                // Figure out the direction this river will try to flow
                River river = new River(riverCount) {CurrentDirection = terrainTile.GetLowestNeighbor()};

                // Recursively find a path to water
                FindPathToWater(terrainTile, river.CurrentDirection, ref river, worldData);

                // Validate the generated river 
                if (river.TurnCount < worldData.MinRiverTurns || river.Tiles.Count < worldData.MinRiverLength ||
                    river.Intersections > worldData.MaxRiverIntersections)
                {
                    //Validation failed - remove this river
                    foreach (var t in river.Tiles)
                    {
                        t.Rivers.Remove(river);
                    }
                }
                else if (river.Tiles.Count >= worldData.MinRiverLength)
                {
                    //Validation passed - Add river to list
                    worldData.Rivers.Add(river);
                    terrainTile.Rivers.Add(river);
                    riverCount--;
                }
            }
        }

        private static void DigRiver(River river, River parent)
        {
            int intersectionId = 0;
            int intersectionSize = 0;

            // determine point of intersection
            for (int i = 0; i < river.Tiles.Count; i++)
            {
                TerrainTile t1 = river.Tiles[i];
                foreach (var t2 in parent.Tiles)
                {
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

            int twoMin = two / 3;
            int threeMin = three / 3;
            int fourMin = four / 3;
            int fiveMin = five / 3;

            // randomize length of each size
            int count1 = Random.Range(fiveMin, five);
            if (size < 4) count1 = 0;
            int count2 = count1 + Random.Range(fourMin, four);
            if (size < 3)
            {
                count2 = 0;
                count1 = 0;
            }

            int count3 = count2 + Random.Range(threeMin, three);
            if (size < 2)
            {
                count3 = 0;
                count2 = 0;
                count1 = 0;
            }

            int count4 = count3 + Random.Range(twoMin, two);

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
                TerrainTile t = river.Tiles[i];

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

        private static void DigRiver(River river)
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

            int twoMin = two / 3;
            int threeMin = three / 3;
            int fourMin = four / 3;
            int fiveMin = five / 3;

            // randomize length of each size
            int count1 = Random.Range(fiveMin, five);
            if (size < 4) count1 = 0;
            int count2 = count1 + Random.Range(fourMin, four);
            if (size < 3)
            {
                count2 = 0;
                count1 = 0;
            }

            int count3 = count2 + Random.Range(threeMin, three);
            if (size < 2)
            {
                count3 = 0;
                count2 = 0;
                count1 = 0;
            }

            int count4 = count3 + Random.Range(twoMin, two);

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
                TerrainTile t = river.Tiles[i];

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

        private static void FindPathToWater(TerrainTile terrainTile, Direction direction, ref River river, WorldData worldData)
        {
            if (terrainTile.Rivers.Contains(river))
                return;

            // check if there is already a river on this tile
            if (terrainTile.Rivers.Count > 0)
                river.Intersections++;

            river.AddTile(terrainTile);

            // get neighbors
            TerrainTile left = worldData.GetLeft(terrainTile);
            TerrainTile right = worldData.GetRight(terrainTile);
            TerrainTile top = worldData.GetTop(terrainTile);
            TerrainTile bottom = worldData.GetBottom(terrainTile);

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

            // find minimum
            float min = Mathf.Min(Mathf.Min(Mathf.Min(leftValue, rightValue), topValue), bottomValue);

            // if no minimum found - exit
            if (Math.Abs(min - int.MaxValue) < 0.01)
                return;

            //Move to next neighbor
            if (Math.Abs(min - leftValue) < 0.01)
            {
                if (left.Collidable)
                {
                    if (river.CurrentDirection != Direction.Left)
                    {
                        river.TurnCount++;
                        river.CurrentDirection = Direction.Left;
                    }

                    FindPathToWater(left, direction, ref river, worldData);
                }
            }
            else if (Math.Abs(min - rightValue) < 0.01)
            {
                if (right.Collidable)
                {
                    if (river.CurrentDirection != Direction.Right)
                    {
                        river.TurnCount++;
                        river.CurrentDirection = Direction.Right;
                    }

                    FindPathToWater(right, direction, ref river, worldData);
                }
            }
            else if (Math.Abs(min - bottomValue) < 0.01)
            {
                if (bottom.Collidable)
                {
                    if (river.CurrentDirection != Direction.Bottom)
                    {
                        river.TurnCount++;
                        river.CurrentDirection = Direction.Bottom;
                    }

                    FindPathToWater(bottom, direction, ref river, worldData);
                }
            }
            else if (Math.Abs(min - topValue) < 0.01)
            {
                if (top.Collidable)
                {
                    if (river.CurrentDirection != Direction.Top)
                    {
                        river.TurnCount++;
                        river.CurrentDirection = Direction.Top;
                    }

                    FindPathToWater(top, direction, ref river, worldData);
                }
            }
        }
    }
}