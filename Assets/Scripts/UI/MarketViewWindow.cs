using System;
using System.Collections.Generic;
using Assets.Scripts._3rdparty;
using Assets.Scripts.Market;
using Assets.Scripts.Villages;
using Assets.Scripts.World;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public class MarketViewWindow : MonoBehaviour
    {
        public GameObject Row;
        public GameObject RowGroup;

        public ArrayByEnum<List<SellOrder>, ResourceType> SellOrders = new ArrayByEnum<List<SellOrder>, ResourceType>();
        public ArrayByEnum<List<BuyOrder>, ResourceType> BuyOrders = new ArrayByEnum<List<BuyOrder>, ResourceType>();
        // Start is called before the first frame update
        public GameObject[] RowGroups;
        void Start()
        {
            RowGroups = new[] { Instantiate(RowGroup,transform), Instantiate(RowGroup,transform) };
            foreach (ResourceType resource in Enum.GetValues(typeof(ResourceType)))
            {
                SellOrders[resource] = new List<SellOrder>();
                BuyOrders[resource] = new List<BuyOrder>();
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (!GenerateWorld.MapGenerated) return;

            foreach (ResourceType resource in Enum.GetValues(typeof(ResourceType)))
            {
                foreach (SellOrder sellOrder in Market.Market.SellOrders[resource])
                {
                    if (!SellOrders[resource].Contains(sellOrder))
                    {
                        RowUpdate rowUpdate = Instantiate(Row, RowGroups[0].transform).GetComponent<RowUpdate>();
                        rowUpdate.CreateSellRow(this, sellOrder);
                        SellOrders[resource].Add(sellOrder);
                    }
                }
            }

            foreach (ResourceType resource in Enum.GetValues(typeof(ResourceType)))
            {
                foreach (BuyOrder buyOrder in Market.Market.BuyOrders[resource])
                {
                    if (!BuyOrders[resource].Contains(buyOrder))
                    {
                        RowUpdate rowUpdate = Instantiate(Row, RowGroups[1].transform).GetComponent<RowUpdate>();
                        rowUpdate.CreateBuyRow(this, buyOrder);
                        BuyOrders[resource].Add(buyOrder);
                    }
                }
            }
        }
    }
}
