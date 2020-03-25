using System.Collections.Generic;
using Assets.Scripts.World;
using JetBrains.Annotations;
using UnityEngine;

namespace Assets.Scripts.UI
{
    [UsedImplicitly]
    public class MapOverlayControl : MonoBehaviour
    {
        private bool heatOverlayEnabled = false;
        private bool heightOverlayEnabled = false;
        private bool moistureOverlayEnabled = false;
        private bool biomeOverlayEnabled = false;
        public GenerateWorld WorldGenerator;
        private WorldData worldData;

        // Our texture output
        private MeshRenderer textureRenderer;

        // Start is called before the first frame update
        public void Start()
        {
            textureRenderer = gameObject.GetComponent<MeshRenderer>();
            textureRenderer.enabled = false;
            textureRenderer.sortingLayerName = "Overlay";
            StartCoroutine(WaitForMapData());

        }

        private IEnumerator<Coroutine> WaitForMapData()
        {
            while (!GenerateWorld.MapGenerated)
            {
                yield return null;
            }

            worldData = GenerateWorld.World;
        }

        public void EnableHeatMap()
        {
            if (heatOverlayEnabled)
            {
                heatOverlayEnabled = false;
                textureRenderer.enabled = false;
                return;
            }

            textureRenderer.enabled = true;
            heatOverlayEnabled = true;
            textureRenderer.materials[0].mainTexture = MapOverlay.GetHeatMapTexture(worldData.Width, worldData.Height, worldData.TerrainTileMap);
        }

        public void EnableHeightMap()
        {
            if (heightOverlayEnabled)
            {
                heightOverlayEnabled = false;
                textureRenderer.enabled = false;
                return;
            }

            textureRenderer.enabled = true;
            heightOverlayEnabled = true;
            textureRenderer.materials[0].mainTexture = MapOverlay.GetHeightMapTexture(worldData.Width, worldData.Height, worldData.TerrainTileMap);
        }

        public void EnableMoistureMap()
        {
            if (moistureOverlayEnabled)
            {
                moistureOverlayEnabled = false;
                textureRenderer.enabled = false;
                return;
            }

            textureRenderer.enabled = true;
            moistureOverlayEnabled = true;
            textureRenderer.materials[0].mainTexture = MapOverlay.GetMoistureMapTexture(worldData.Width, worldData.Height, worldData.TerrainTileMap);
        }

        public void EnableBiomeMap()
        {
            if (biomeOverlayEnabled)
            {
                biomeOverlayEnabled = false;
                textureRenderer.enabled = false;
                return;
            }

            textureRenderer.enabled = true;
            biomeOverlayEnabled = true;
            textureRenderer.materials[0].mainTexture = MapOverlay.GetBiomeMapTexture(worldData.Width, worldData.Height, worldData.TerrainTileMap, worldData.ColdestValue, worldData.ColderValue, worldData.ColdestValue);
        }

    }
}
