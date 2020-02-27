using System.Collections.Generic;
using Assets.Scripts.World;
using Pathfinding;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Scripts.Villages
{
    public class Village : MonoBehaviour
    {
        private Seeker seeker;
        public Path Path = null;

        public void FindPathToVillage(GameObject village)
        {
            seeker = gameObject.GetComponent<Seeker>();
            Path = seeker.StartPath(transform.position, village.transform.position, null);
            Path.BlockUntilCalculated();
            Debug.Log("Path Completed: " + Path.vectorPath.Count);
        }
    }
}
