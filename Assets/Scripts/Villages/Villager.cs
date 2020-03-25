using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.World;
using Pathfinding;
using UnityEngine;
using UnityEngine.Assertions.Must;

namespace Assets.Scripts.Villages
{
    public class Villager : MonoBehaviour
    {
        public VillageTile Village;
        private GenerateWorld generateWorld;
        private AILerp movement;
        private bool hasDestination;
        private bool isActive;
        private bool neverWandered = true;
        private Vector3Int lastCell;
        public List<VillageTile> ReachableVillages;
        private int currentVillage;
        public float id;
        public List<VillageTile> RemovedVillages = new List<VillageTile>();

        public bool WanderEnabled;

        private Seeker seeker;
        // Start is called before the first frame update
        void Start()
        {
            id = Random.value;
            seeker = GetComponent<Seeker>();
            movement = GetComponent<AILerp>();
        }

        public void GenerateVillager(GenerateWorld world, VillageTile village)
        {
            generateWorld = world;
            Village = village;
            gameObject.transform.position = village.WorldPosition;
            isActive = true;
            WanderEnabled = true;
            lastCell = village.CellPosition;
            ReachableVillages = generateWorld.VillagesDictionary.Values.ToList();
        }

        // Update is called once per frame
        void Update()
        {
            if (!isActive) return;

            if (movement.reachedDestination && movement.hasPath) hasDestination = false;

            if (WanderEnabled && !movement.pathPending && (neverWandered || !hasDestination))
            {
                neverWandered = false;
                Wander();
            }

            //See what tile the villager is under when moving
            if ( movement.hasPath && !movement.reachedDestination)
            {
                //Cell position
                Vector3Int cell = generateWorld.Tilemap.WorldToCell(transform.position);

                if (!lastCell.Equals(cell) && !generateWorld.RoadHandler.RoadDictionary.ContainsKey(cell))
                {
                    TerrainTile tile = GenerateWorld.World.TerrainTileMap[cell.x, cell.y];
                    Color currentColor = generateWorld.Tilemap.GetColor(cell);

                    Color.RGBToHSV(currentColor,out float h, out float s, out float v);
                    v -= 0.01f;
                    if (v < 0.2f) v = 0.2f;
                    generateWorld.Tilemap.SetColor(cell,Color.HSVToRGB(h,s,v));
                    lastCell = cell;
                }
            }
        }

        private void Wander()
        {
            if (ReachableVillages.Count > 0)
            {
                currentVillage = Random.Range(0, ReachableVillages.Count - 1);
                NewDestination(ReachableVillages[currentVillage].WorldPosition);
                hasDestination = true;
            }
            else
            {
                movement.canSearch = false;
                movement.canMove = false;
                WanderEnabled = false;
                movement.destination = Vector3.positiveInfinity;
            }
        }

        public void NewDestination(Vector3 destination)
        {
            movement.destination = destination;
            seeker.pathCallback += CheckValidPath;
        }

        private void CheckValidPath(Path p)
        {
            //Don't try to reach unreachable places
            if (currentVillage > ReachableVillages.Count-1) return;
            if (p.error && !RemovedVillages.Contains(ReachableVillages[currentVillage]))
            {
                RemovedVillages.Add(ReachableVillages[currentVillage]);
                ReachableVillages.RemoveAt(currentVillage);
                hasDestination = false;
            }
        }
    }
}
