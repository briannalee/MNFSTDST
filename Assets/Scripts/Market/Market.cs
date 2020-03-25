using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts._3rdparty;
using Assets.Scripts.Villages;
using UnityEngine;

namespace Assets.Scripts.Market
{
    public static class Market
    {
        public static ArrayByEnum<List<SellOrder>, ResourceType> SortedSellOrders = new ArrayByEnum<List<SellOrder>, ResourceType>();
        public static ArrayByEnum<List<BuyOrder>, ResourceType> SortedBuyOrders = new ArrayByEnum<List<BuyOrder>, ResourceType>();
        public static ArrayByEnum<List<SellOrder>, ResourceType> SellOrders = new ArrayByEnum<List<SellOrder>, ResourceType>();
        public static ArrayByEnum<List<BuyOrder>, ResourceType> BuyOrders = new ArrayByEnum<List<BuyOrder>, ResourceType>();

        public static void Init()
        {
            foreach (ResourceType resource in Enum.GetValues(typeof(ResourceType)))
            {
                SellOrders[resource] = new List<SellOrder>();
                SortedSellOrders[resource] = new List<SellOrder>();
                BuyOrders[resource] = new List<BuyOrder>();
                SortedBuyOrders[resource] = new List<BuyOrder>();
            }
        }

        public static SellOrder CreateSellOrder(VillageAI village, ResourceType resourceType, decimal price, int units)
        {
            SellOrder sellOrder = new SellOrder(village, resourceType, price, units);
            //Ensure that the SellOrder was able to place the resources into its Escrow
            if (sellOrder.HasEscrow())
            {
                SellOrders[resourceType].Add(sellOrder);
                SortedSellOrders[resourceType] = SellOrders[resourceType].OrderBy(p => p.Price).ToList();
            }
            return sellOrder;
        }

        public static BuyOrder CreateBuyOrder(VillageAI village, ResourceType resourceType, decimal price, int units)
        {
            BuyOrder buyOrder = new BuyOrder(village, resourceType, price, units);
            //Ensure that the SellOrder was able to place the resources into its Escrow
            if (buyOrder.HasEscrow())
            {
                BuyOrders[resourceType].Add(buyOrder);
                SortedBuyOrders[resourceType] = BuyOrders[resourceType].OrderByDescending(p => p.Price).ToList();
            }
            return buyOrder;
        }

        public static bool RemoveBuyOrder(BuyOrder buyOrder)
        {
            if (buyOrder.Locked) return false;
            buyOrder.Locked = true;
            bool tryRemove = BuyOrders[buyOrder.Resource].Remove(buyOrder);
            if (tryRemove) SortBuyOrders(buyOrder.Resource);
            return true;
        }

        public static bool RemoveSellOrder(SellOrder sellOrder)
        {
            if (sellOrder.Locked) return false;
            sellOrder.Locked = true;
            bool tryRemove = SellOrders[sellOrder.Resource].Remove(sellOrder);
            if (tryRemove) SortSellOrders(sellOrder.Resource);
            return tryRemove;
        }

        public static void SortSellOrders(ResourceType resourceType)
        {
            SortedSellOrders[resourceType] = SellOrders[resourceType].OrderBy(p => p.Price).ToList();
        }

        public static void SortBuyOrders(ResourceType resourceType)
        {
            SortedBuyOrders[resourceType] = BuyOrders[resourceType].OrderByDescending(p => p.Price).ToList();
        }

        public static int WorkDeal(ref SellOrder sellOrder, ref BuyOrder buyOrder)
        {
            if (sellOrder.Locked || buyOrder.Locked) return 0;
            sellOrder.Locked = true;
            buyOrder.Locked = true;
            int deal = AttemptDeal(ref sellOrder, ref buyOrder);
            sellOrder.Locked = false;
            buyOrder.Locked = false;
            return deal;
        }

        private static int AttemptDeal(ref SellOrder sellOrder, ref BuyOrder buyOrder)
        {
            if (!sellOrder.HasEscrow() || !buyOrder.HasEscrow()) return 0;

            int sellerAvailableUnits = sellOrder.Escrow;
            int buyerDesiredUnits = buyOrder.Units;
            if (buyerDesiredUnits < 1) return 0;
            //Sell the amount the buyer would like, up to the maximum the seller has available
            int unitsToWork = Mathf.Clamp(buyerDesiredUnits, 1, sellerAvailableUnits);
            //We always sell at the sellers price
            decimal totalCost = unitsToWork * sellOrder.Price;
            //Ensure Buyer has the funds
            if (buyOrder.Escrow < totalCost) return 0;
            decimal buyersFunds = buyOrder.DeductFromEscrow(totalCost);
            int sellersResources = sellOrder.DeductFromEscrow(unitsToWork);
            if (buyersFunds.CompareTo(0m) <= 0)
            {
                sellOrder.ReturnToEscrow(unitsToWork);
                return 0;
            }
            if (sellersResources.CompareTo(0) <= 0)
            {
                buyOrder.ReturnToEscrow(totalCost);
                return 0;
            }

            buyOrder.Units -= unitsToWork;
            buyOrder.Village.AddResources(buyOrder.Resource, sellersResources);
            sellOrder.Village.AddMoney(buyersFunds);
            return unitsToWork;
        }
    }
}
