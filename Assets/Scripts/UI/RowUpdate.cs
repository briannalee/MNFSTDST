using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Assets.Scripts.Market;
using Assets.Scripts.Villages;
using Assets.Scripts.World;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class RowUpdate : MonoBehaviour
    {
        public GameObject ColumnEntry;
        private Text[] columns;
        private MarketViewWindow marketViewWindow;
        private BuyOrder buyOrder;
        private SellOrder sellOrder;
        private bool isSellOrder;

        private bool wasEnabled;
        // Start is called before the first frame update
        void Start()
        {
        }

        void OnEnable()
        {
            if (wasEnabled)
            {
                if (isSellOrder) StartCoroutine(UpdateMarketSellRow());
                else StartCoroutine(UpdateMarketBuyRow());
            }
        }

        public void CreateSellRow(MarketViewWindow _marketViewWindow, SellOrder _sellOrder, int _numColumns = 7)
        {
            isSellOrder = true;
            marketViewWindow = _marketViewWindow;
            sellOrder = _sellOrder;
            columns = new Text[_numColumns];
            for (int i = 0; i < _numColumns; i++)
            {
                Text columnText = Instantiate(ColumnEntry, transform).GetComponent<Text>();
                columns[i] = columnText;
            }

            StartCoroutine(UpdateMarketSellRow());
        }

        public void CreateBuyRow(MarketViewWindow _marketViewWindow, BuyOrder _buyOrder, int _numColumns = 7)
        {
            isSellOrder = false;
            marketViewWindow = _marketViewWindow;
            buyOrder = _buyOrder;
            columns = new Text[_numColumns];
            for (int i = 0; i < _numColumns; i++)
            {
                Text columnText = Instantiate(ColumnEntry, transform).GetComponent<Text>();
                columns[i] = columnText;
            }

            StartCoroutine(UpdateMarketBuyRow());
        }

        private IEnumerator<WaitForSeconds> UpdateMarketSellRow()
        {
            ResourceType resourceType = sellOrder.Resource;
            wasEnabled = true;
            while (true)
            {
                if (!Market.Market.SellOrders[resourceType].Contains(sellOrder))
                {
                    marketViewWindow.SellOrders[resourceType].Remove(sellOrder);
                    break;
                }
                int index = Market.Market.SellOrders[resourceType].IndexOf(sellOrder);
                sellOrder = Market.Market.SellOrders[resourceType][index];
                columns[0].text = resourceType.ToString();
                columns[1].text = sellOrder.Village.name;
                columns[2].text = sellOrder.Price.ToString(CultureInfo.InvariantCulture);
                columns[3].text = sellOrder.Units.ToString(CultureInfo.InvariantCulture);
                columns[4].text = sellOrder.Escrow.ToString(CultureInfo.InvariantCulture);
                columns[5].text = (sellOrder.Escrow - sellOrder.Units).ToString(CultureInfo.InvariantCulture);
                columns[6].text = sellOrder.Village.Money.ToString(CultureInfo.InvariantCulture);
                yield return new WaitForSeconds(2f);
            }

            Destroy(gameObject);
            Destroy(this);
        }


        private IEnumerator<WaitForSeconds> UpdateMarketBuyRow()
        {
            ResourceType resourceType = buyOrder.Resource;
            wasEnabled = true;
            while (true)
            {
                if (!Market.Market.BuyOrders[resourceType].Contains(buyOrder))
                {
                    marketViewWindow.BuyOrders[resourceType].Remove(buyOrder);
                    break;
                }
                int index = Market.Market.BuyOrders[resourceType].IndexOf(buyOrder);
                buyOrder = Market.Market.BuyOrders[resourceType][index];
                columns[0].text = resourceType.ToString();
                columns[1].text = buyOrder.Village.name;
                columns[2].text = buyOrder.Price.ToString();
                columns[3].text = buyOrder.Units.ToString();
                columns[4].text = buyOrder.Escrow.ToString(CultureInfo.InvariantCulture);
                columns[5].text = (buyOrder.Escrow - (buyOrder.Units * buyOrder.Price)).ToString(CultureInfo.InvariantCulture);
                columns[6].text = buyOrder.Village.Money.ToString(CultureInfo.InvariantCulture);
                yield return new WaitForSeconds(2f);
            }

            Destroy(gameObject);
            Destroy(this);
        }
    }
}
