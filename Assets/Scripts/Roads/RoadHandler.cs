using System.Collections.Generic;
using System.Linq;
using Pathfinding;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Scripts.Roads
{
    public class RoadHandler : MonoBehaviour
    {
        public Tilemap RoadTilemap;
        public List<Road> Roads = new List<Road>();
        public List<RoadTile> RoadPoints = new List<RoadTile>();
        public Dictionary<Vector3Int, RoadTile> RoadDictionary = new Dictionary<Vector3Int, RoadTile>();

        /// <summary>
        /// Accepts a rough Seeker Path and runs it through optimizations and formatting
        /// Adds the Road to the Roads List once completed
        /// Adds all the road points to the master RoadPoints and CellPoints lists
        /// </summary>
        /// <param name="pathPoints">The raw Vector data from an A* Seeker Path</param>
        /// <param name="mergeDistance">Max distance to search for possible road merges</param>
        /// <returns></returns>
        private IEnumerator<Coroutine> PathToRoad(Vector3[] pathPoints, int mergeDistance)
        {
            //Create new Road Class to hold our road data for this road
            Road road = new Road(pathPoints.Length, RoadTilemap);


            for (int i = 0; i < pathPoints.Length; i++)
            {
                //Find closest road point
                RoadTile nearestNeighbour = FindNearestNeighbour(pathPoints[i], mergeDistance);

                // If nearest neighbour is set, the path optimizer will assume the path needs combining
                road.AddPointToSection(pathPoints[i], nearestNeighbour);
            }
            yield return StartCoroutine(MergeRoads(road));
            FinalizeRoad(road);
            Roads.Add(road);
            RoadPoints.AddRange(road.Points);
        }

        /// <summary>
        /// Finalizes the road points from the list of RoadSections
        /// </summary>
        /// <param name="road">Road to finalize</param>
        private void FinalizeRoad(Road road)
        {
            foreach (RoadSection section in road.Sections)
            {
                foreach (RoadTile roadTile in section.RoadPoints)
                {
                    road.AddFinalizedRoadPoint(roadTile);
                    if (!RoadDictionary.ContainsKey(roadTile.CellPosition)) RoadDictionary.Add(roadTile.CellPosition,roadTile);
                }
            }
        }

        /// <summary>
        /// Runs through the rough RoadSections list, merges with existing roads where possible
        /// and combines multiple adjusted sections that connect to the same road that are close together
        /// </summary>
        /// <param name="road">Road to be merged</param>
        /// <returns></returns>
        private IEnumerator<Coroutine> MergeRoads(Road road)
        {
            Seeker seeker = gameObject.AddComponent<Seeker>();
            List<RoadSection> updatedSections = new List<RoadSection>();
            for (var sIndex = 0; sIndex < road.Sections.Count; sIndex++)
            {
                RoadSection section = road.Sections[sIndex];

                //If section is non-adjusted, just add as is
                if (!section.IsAdjustedSection)
                {
                    updatedSections.Add(section);
                    //Go to next section
                    continue;
                }

                RoadSection updatedSection = new RoadSection(road);
                RoadTile startTile = section.RoadPoints.First();
                RoadTile startTileDestination = section.RoadPoints.First().NearestNeighbour;
                RoadTile endTile = section.RoadPoints.Last();
                RoadTile endTileDestination = section.RoadPoints.Last().NearestNeighbour;
                bool pathFailed = false;

                //Check if we can combine any subsequent adjusted sections into one large section
                for (int csIndex = sIndex + 1; csIndex < road.Sections.Count; csIndex++)
                {
                    RoadSection nextSection = road.Sections[csIndex];
                    //If it's a non-adjusted section, we ignore it
                    if (!nextSection.IsAdjustedSection)
                    {
                        continue;
                    }

                    RoadTile nextFirstTile = nextSection.RoadPoints.First();
                    RoadTile nextEndTile = nextSection.RoadPoints.Last();
                    RoadTile nextEndDestination = nextSection.RoadPoints.Last().NearestNeighbour;

                    
                    if (startTileDestination.ThisRoad == nextEndDestination.ThisRoad &&
                        Vector3.Distance(endTile.Position, nextFirstTile.Position) <= 15)
                    {

                        
                        //Add Points Between the end of our current path, and the start of the next one
                        //To make up for the regular paths we skipped above
                        Path pathToNextSectionStart = seeker.StartPath(endTile.Position, nextFirstTile.Position, null);
                        yield return StartCoroutine(pathToNextSectionStart.WaitForPath());
                        if (pathToNextSectionStart.error)
                        {
                            pathFailed = true;
                            updatedSections.Add(section);
                            break;
                        }
                        for (int a = 0; a < pathToNextSectionStart.vectorPath.Count; a++)
                        {
                            updatedSection.Add(pathToNextSectionStart.vectorPath[a]);
                        }

                        endTile = nextEndTile;
                        endTileDestination = nextEndDestination;

                        //On next iteration skip to after the section merge we just created above
                        sIndex = csIndex + 1;
                    }
                }

                if (pathFailed) continue;

                //Now we have our full adjusted section. We just need to connect the start and end points to their respective connections
                Path pathFromStart = seeker.StartPath(startTile.Position, startTileDestination.Position, null);
                yield return StartCoroutine(pathFromStart.WaitForPath());
                if (pathFromStart.error)
                {
                    updatedSections.Add(section);
                    continue;
                }
                Path pathFromEnd = seeker.StartPath(endTile.Position, endTileDestination.Position, null);
                yield return StartCoroutine(pathFromEnd.WaitForPath());
                if (pathFromEnd.error)
                {
                    updatedSections.Add(section);
                    continue;
                }

                for (int i = 0; i < pathFromStart.vectorPath.Count; i++)
                {
                    updatedSection.Add(pathFromStart.vectorPath[i]);
                }

                for (int i = 0; i < pathFromEnd.vectorPath.Count; i++)
                {
                    updatedSection.Add(pathFromEnd.vectorPath[i]);
                }

                updatedSections.Add(updatedSection);
            }
            road.Sections = updatedSections.ToList();
        }

        private RoadTile FindNearestNeighbour(Vector3 searchPoint, int searchDistance)
        {
            //If there are no road points, return null
            if (RoadPoints.Count < 1) return null;

            // Search for the nearest neighbour out of the list of all road points
            RoadTile nearestNeighbour = RoadPoints.Aggregate(((point1, point2) =>
                Vector3.Distance(searchPoint, point1.Position) < 
                Vector3.Distance(searchPoint, point2.Position) ? 
                    point1 : point2));

            //If the nearestNeighbour is further than our max searchDistance, return null
            return Vector3.Distance(searchPoint, nearestNeighbour.Position) > searchDistance ? null : nearestNeighbour;
        }


        public IEnumerator<Coroutine> CreateRoad(Vector3 start, Vector3 end, int mergeDistance = 10)
        {
            Seeker seeker = gameObject.AddComponent<Seeker>();
            Path path = seeker.StartPath(start,end);
            if (!path.error)
            {
                yield return StartCoroutine(path.WaitForPath());
                yield return StartCoroutine(PathToRoad(path.vectorPath.ToArray(), mergeDistance));
            }
        }
    }
}
