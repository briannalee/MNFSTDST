using System;
using Assets.Scripts._3rdparty;
using Assets.Scripts.Villages;
using Assets.Scripts.World;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class UpdateMarketDisplay : MonoBehaviour
    {
        public GameObject TextPrefab;
        public ArrayByEnum<Text, ResourceType> TextObjects = new ArrayByEnum<Text, ResourceType>();

        // Use this for initialization
        void Start()
        {
            foreach (ResourceType resource in Enum.GetValues(typeof(ResourceType)))
            {
                TextObjects[resource] = Instantiate(TextPrefab, gameObject.transform).GetComponent<Text>();
            }
        }

        // Update is called once per frame
        void Update()
        {
            foreach (ResourceType resource in Enum.GetValues(typeof(ResourceType)))
            {
                TextObjects[resource].text = resource +
                                             " Market Price: " + GenerateWorld.ResourceMarketPrices[resource] +
                                             " | Active Sell Orders: " + Market.Market.SortedSellOrders[resource].Count +
                                             " | Active Buy Orders: " + Market.Market.SortedBuyOrders[resource].Count;
            }
        }
    }
}
