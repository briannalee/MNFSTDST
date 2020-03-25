using Assets.Scripts.Villages;
using Assets.Scripts.World;

namespace Assets.Scripts.Market
{
    public class BuyOrder : Order
    {
        public decimal Escrow;

        public BuyOrder(VillageAI village, ResourceType resourceType, decimal price, int units) : base(village, resourceType, price, units)
        {
            //Deduct the resources from the seller, and place safely in escrow
            Escrow = village.DeductMoney(price * units);
        }

        public bool IncreaseOrderPrice(decimal additionalAmount)
        {
            if (Locked) return false;
            decimal totalCost = additionalAmount * Units;
            decimal currentTotalCost = Price * Units;
            decimal additionalFundsNeeded = totalCost - (Escrow - currentTotalCost);
            if (additionalFundsNeeded < 0m) additionalFundsNeeded = 0m;
            decimal funds = Village.DeductMoney(additionalFundsNeeded);
            if (funds + (Escrow - currentTotalCost) < totalCost) return false;
            Escrow += funds;
            Price += additionalAmount;
            Market.SortBuyOrders(Resource);
            return true;
        }

        public bool IncreaseOrderUnits(int additionalUnits)
        {
            if (Locked) return false;
            decimal totalCost = Price * additionalUnits;
            decimal currentTotalCost = Price * Units;
            decimal additionalFundsNeeded = totalCost - (Escrow - currentTotalCost);
            if (additionalFundsNeeded < 0m) additionalFundsNeeded = 0m;
            decimal funds = Village.DeductMoney(additionalFundsNeeded);
            if (funds + (Escrow - currentTotalCost) < totalCost) return false;
            Escrow += funds;
            Units += additionalUnits;
            return true;
        }

        public bool DecreaseOrderUnits(int decreaseUnits)
        {
            if (Locked) return false;
            Units -= decreaseUnits;
            return true;
        }

        public bool HasEscrow()
        {
            if (Escrow <= 0m) return false;
            return true;
        }

        public decimal DeductFromEscrow(decimal amount)
        {
            if (!Locked) return 0m;
            if (Escrow < amount) return 0m;

            Escrow -= amount;
            if (Escrow < 0m)
            {
                Escrow += amount;
                return 0m;
            }

            return amount;
        }

        public void ReturnToEscrow(decimal amount)
        {
            if (amount < 0m) return;
            Escrow += amount;
        }

        public bool Destroy()
        {
            bool tryDestroy = Market.RemoveBuyOrder(this);
            if (tryDestroy) Village.AddMoney(Escrow);;
            return tryDestroy;
        }
    }
}