using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using Assets.Scripts._3rdparty;
using Assets.Scripts.Market;
using Assets.Scripts.World;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Villages
{
    public class VillageAI : MonoBehaviour
    {
        public VillageTile VillageTile;
        public decimal Money;
        public ArrayByEnum<int, ResourceType> Resources = new ArrayByEnum<int, ResourceType>();
        public ArrayByEnum<bool, ResourceType> CurrentlySelling = new ArrayByEnum<bool, ResourceType>();
        public ArrayByEnum<bool, ResourceType> CurrentlyBuying = new ArrayByEnum<bool, ResourceType>();
        public ArrayByEnum<float, ResourceType> PassiveGeneration = new ArrayByEnum<float, ResourceType>();
        public ArrayByEnum<int, ResourceType> MinResources = new ArrayByEnum<int, ResourceType>();
        public ArrayByEnum<int, ResourceType> ResourceBufferBeforeSale = new ArrayByEnum<int, ResourceType>();
        public decimal Miserliness;
        public int Wood;
        public int Water;
        public int Food;
        public float PassiveWood;
        public float PassiveWater;
        public float PassiveFood;
        public bool Unreachable { get; set; }

        void Start()
        {
            Miserliness = Random.Range(0, 100)/100m;
            Resources[ResourceType.Water] = Random.Range(50, 100);
            Resources[ResourceType.Food] = Random.Range(40, 100);
            Resources[ResourceType.Wood] = Random.Range(30, 100);
            MinResources[ResourceType.Water] = 80;
            MinResources[ResourceType.Food] = 60;
            MinResources[ResourceType.Wood] = 50;

            ResourceBufferBeforeSale[ResourceType.Water] = 5;
            ResourceBufferBeforeSale[ResourceType.Food] = 10;
            ResourceBufferBeforeSale[ResourceType.Wood] = 15;

            foreach (ResourceType resource in Enum.GetValues(typeof(ResourceType)))
            {
                PassiveGeneration[resource] = VillageTile.ContainsResources[resource];
            }

            Money = Random.Range(50, 250);
            StartCoroutine(MonitorVillage());
        }

        /// <summary>
        /// Runs every 2 seconds and monitors the villages resources, sourcing where needed, and selling excess to generate income
        /// </summary>
        /// <returns></returns>
        public IEnumerator<WaitForSeconds> MonitorVillage()
        {
            while (true)
            {
                PassiveWood = PassiveGeneration[ResourceType.Wood];
                PassiveFood = PassiveGeneration[ResourceType.Food];
                PassiveWater = PassiveGeneration[ResourceType.Water];
                Wood = Resources[ResourceType.Wood];
                Water = Resources[ResourceType.Water];
                Food = Resources[ResourceType.Food];
                foreach (ResourceType resource in Enum.GetValues(typeof(ResourceType)))
                {
                    Resources[resource] += (int)PassiveGeneration[resource];
                    Resources[resource] --;
                    if (Resources[resource] < 0) Resources[resource] = 0;
                    if (Resources[resource] < MinResources[resource] && !CurrentlyBuying[resource])
                    {
                        StartCoroutine(AttemptToBuyResource(resource));
                    }

                    if (Resources[resource] > MinResources[resource] + ResourceBufferBeforeSale[resource] && !CurrentlySelling[resource])
                    {
                        StartCoroutine(AttemptToSellResource(resource));
                    }
                }

                yield return new WaitForSeconds(2);
            }
        }

        private IEnumerator<WaitForSeconds> AttemptToBuyResource(ResourceType resourceType)
        {
            //How many units would we like?
            int desiredUnits = (MinResources[resourceType] + (ResourceBufferBeforeSale[resourceType] / 2)) - Resources[resourceType];
            
            //Start below market price
            decimal desiredPrice = GenerateWorld.ResourceMarketPrices[resourceType] - Miserliness;
            if (desiredPrice.CompareTo(0.01m) < 0) desiredPrice = 0.01m;
            float timeSinceLastPriceIncrease = Time.time;

            //Check how many units we could afford at this price
            if (desiredPrice * desiredUnits > Money)
            {
                desiredUnits = (int)(Money / desiredPrice);
                //If desiredUnits is less than one, we cant afford to buy anything. Exit
                if (desiredUnits < 1) yield break;
            }

            //Place the money we will use to buy into escrow
            //decimal escrowMoney = desiredUnits * desiredPrice;
            //Money -= escrowMoney;
            //MoneyEscrow[resourceType] += escrowMoney;

            //GenerateWorld.BuyOrders[resourceType].Add(this, new[] { desiredPrice, desiredUnits });
            BuyOrder buyOrder = Market.Market.CreateBuyOrder(this, resourceType, desiredPrice, desiredUnits);
            CurrentlyBuying[resourceType] = true;

            // Will loop, and either buy from market or update our buy order until we obtain enough resources
            while (buyOrder.HasEscrow() && buyOrder.Units > 0)
            {
                //First, buy from the market where possible.
                bool buyFromMarket = CheckAndBuyFromMarket(ref buyOrder);
                    //BuyFromMarket(resourceType, desiredPrice, (int) GenerateWorld.BuyOrders[resourceType][this][1]);
                //If BuyFromMarket() returns true, it was able to buy all the resources we needed. We can exit
                if (buyFromMarket)
                {
                    break;
                }


                if ((Time.time - timeSinceLastPriceIncrease) > 5f)
                {
                    decimal newDesiredPrice = desiredPrice + 0.5m;
                    decimal priceDifference = newDesiredPrice - desiredPrice;
                    int newDesiredUnits = MinResources[resourceType] +
                                          (ResourceBufferBeforeSale[resourceType] / 2) - Resources[resourceType];
                    decimal costOfDesiredPrice = priceDifference * desiredUnits;
                    if (newDesiredUnits < 0)
                    {
                        //If newDesiredUnits is less 0, then we no longer need to buy resources
                        //we have over our minimum + buffer of 0.5 ResourceBufferBeforeSale
                        break;
                    }

                    //First, try and bump up the buy price
                    if (!buyOrder.IncreaseOrderPrice(priceDifference))
                    {
                        //We cant afford the new amount, adjust our buy units so we can increase price
                        int unitsWeCanAfford = (int) (Money / priceDifference);
                        if (unitsWeCanAfford >= 1)
                        {
                            //Decrease units, then try to buy again
                            if (buyOrder.DecreaseOrderUnits(buyOrder.Units - unitsWeCanAfford))
                            {
                                buyOrder.IncreaseOrderPrice(priceDifference);
                            }
                        }
                    }


                    if (newDesiredUnits > desiredUnits)
                    {
                        int unitsDifference = newDesiredUnits - desiredUnits;
                        //Attempt to increase the units we want to buy
                        buyOrder.IncreaseOrderUnits(unitsDifference);
                    }

                    timeSinceLastPriceIncrease = Time.time;
                }
                

                yield return new WaitForSeconds(1f);
            }

            while (true)
            {
                if (buyOrder.Destroy()) break;
                yield return new WaitForSeconds(1f);
            }

            CurrentlyBuying[resourceType] = false;
        }

        private IEnumerator<WaitForSeconds> AttemptToSellResource(ResourceType resourceType)
        {
            //How many excess units do we have to sell?
            int desiredUnits = Resources[resourceType] - (MinResources[resourceType] + ResourceBufferBeforeSale[resourceType]);

            //Start above market price
            decimal desiredPrice = GenerateWorld.ResourceMarketPrices[resourceType] + Miserliness;
            float timeSinceLastPriceDecrease = Time.time;

            //If we have less than 1 unit to sell, exit
            if (desiredUnits < 1) yield break;

            //Place the units we would like to sell into escrow
            //ResourceEscrow[resourceType] += desiredUnits;
            //Resources[resourceType] -= desiredUnits;

            CurrentlySelling[resourceType] = true;
            //GenerateWorld.SellOrders[resourceType].Add(this, new[] { desiredPrice, desiredUnits });
            SellOrder sellOrder = Market.Market.CreateSellOrder(this, resourceType, desiredPrice, desiredUnits);
            // Will loop, and either sell to buyer from market or update our sell order until we sell our excess resource
            while (sellOrder.HasEscrow() && sellOrder.Units > 0)
            {
                //First, sell to buyer from the market where possible.
                bool sellToBuyerFromMarket = CheckAndSellToMarket(ref sellOrder);
                //SellToMarket(resourceType, desiredPrice);
                //If BuyFromMarket() returns true, it was able to buy all the resources we needed. We can exit
                if (sellToBuyerFromMarket)
                {
                    break;
                }
                if ((Time.time - timeSinceLastPriceDecrease) > 5f)
                {
                    if (sellOrder.DecreaseOrderPrice(0.5m))
                    {
                        int additionalUnitsToSell = Resources[resourceType] - (MinResources[resourceType] + ResourceBufferBeforeSale[resourceType]);
                        sellOrder.IncreaseOrderUnits(additionalUnitsToSell);
                        timeSinceLastPriceDecrease = Time.time;
                    }
                }
                
                yield return new WaitForSeconds(1f);
            }

            while (true)
            {
                if (sellOrder.Destroy()) break;
                yield return new WaitForSeconds(1f);
            }
            
            CurrentlySelling[resourceType] = false;
        }

        private bool CheckAndBuyFromMarket(ref BuyOrder buyOrder)
        {
            ResourceType resourceType = buyOrder.Resource;
            //See if there are even any sell orders for our resource
            if (Market.Market.SortedSellOrders[resourceType].Count > 0)
            {
                //Get sorted array of all sale orders, sorted by price

                SellOrder[] sellOrders = Market.Market.SortedSellOrders[resourceType].ToArray();

                //Check the sell orders, starting with the cheapest, and see if we want to buy
                for (var i = 0; i < sellOrders.Length; i++)
                {
                    if (sellOrders[i].Price <= buyOrder.Price)
                    {
                        //Attempt to buy these resources
                        int indexOfSeller = Market.Market.SellOrders[resourceType].IndexOf(sellOrders[i]);
                        SellOrder order = Market.Market.SellOrders[resourceType][indexOfSeller];
                        int buyResources = Market.Market.WorkDeal(ref order, ref buyOrder);
                        Market.Market.SellOrders[resourceType][indexOfSeller] = order;
                        if (buyResources > 0)
                        {
                            // If the buy order went through, check if we need to keep sourcing more of this resource, of if we can quit
                            if (buyOrder.Units < 1) return true;
                        }
                    }
                    else
                    {
                        //The sale orders are sorted by price, and the orders are now above our desired price, we can stop looking at the market
                        return false;
                    }
                }
            }
            return false;
        }

        private bool CheckAndSellToMarket(ref SellOrder sellOrder)
        {
            ResourceType resourceType = sellOrder.Resource;
            //See if there are even any sell orders for our resource
            if (Market.Market.SortedBuyOrders[resourceType].Count > 0)
            {
                //Get sorted array of all buy orders, sorted by price (highest first)
                BuyOrder[] buyOrders = Market.Market.SortedBuyOrders[resourceType].ToArray();

                //Check the sell orders, starting with the cheapest, and see if we want to buy
                for (var i = 0; i < buyOrders.Length; i++)
                {
                    if (buyOrders[i].Price >= sellOrder.Price)
                    {
                        //Attempt to sell these resources
                        int indexOfBuyer = Market.Market.BuyOrders[resourceType].IndexOf(buyOrders[i]);
                        BuyOrder order = Market.Market.BuyOrders[resourceType][indexOfBuyer];
                        int sellResources = Market.Market.WorkDeal(ref sellOrder, ref order);
                        Market.Market.BuyOrders[resourceType][indexOfBuyer] = order;
                        if (sellResources > 0)
                        {
                            // If the buy order went through, check if we need to keep sourcing more of this resource, of if we can quit
                            if (sellOrder.Units < 1) return true;
                        }
                    }
                    else
                    {
                        //The sale orders are sorted by price, and the orders are now above our desired price, we can stop looking at the market
                        return false;
                    }
                }
            }
            return false;
        }

        public int DeductResources(ResourceType resourceType, int units)
        {
            //Attempt to claim resources
            //Quick check first
            if (Resources[resourceType] < units) return 0;

            //Actually try and claim the resources... 
            Resources[resourceType] -= units;

            //...and make sure it's still valid
            if (Resources[resourceType] < 0)
            {
                //Failed, return resources and send nothing to the method caller
                Resources[resourceType] += units;
                return 0;
            }

            //The resources were claimed, give them to whomever called this method
            return units;
        }

        public decimal DeductMoney(decimal amount)
        {
            //Attempt to claim money
            //Quick check first 
            if (Money < amount) return 0m;

            //Actually try and claim the money...
            Money -= amount;

            //...and make sure it's still valid
            if (Money < 0m)
            {
                //Failed, return our money and send nothing to the method caller
                Money += amount;
                return 0m;
            }

            //The money was claimed, give it to whomever called this method
            return amount;
        }

        public void AddMoney(decimal amount)
        {
            Money += amount;
        }
        public void AddResources(ResourceType resourceType, int units)
        {
            Resources[resourceType] += units;
        }
    }
}
