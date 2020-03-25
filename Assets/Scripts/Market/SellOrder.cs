using Assets.Scripts.Villages;
using Assets.Scripts.World;

namespace Assets.Scripts.Market
{
    public class SellOrder : Order
    {
        public int Escrow;

        public SellOrder(VillageAI village, ResourceType resourceType, decimal price, int units) : base(village, resourceType, price, units)
        {
            Escrow = Village.DeductResources(Resource, Units);
        }

        public bool HasEscrow()
        {
            if (Escrow < 1) return false;
            return true;
        }

        public bool IncreaseOrderPrice(decimal additionalAmount)
        {
            if (Locked) return false;
            Price += additionalAmount;
            Market.SortSellOrders(Resource);
            return true;
        }

        public bool DecreaseOrderPrice(decimal decreaseAmount)
        {
            if (Locked) return false;
            Price -= decreaseAmount;
            if (Price < 0.01m) Price = 0.01m;
            Market.SortSellOrders(Resource);
            return true;
        }

        public bool IncreaseOrderUnits(int additionalUnits)
        {
            if (Locked) return false;
            int resources = Village.DeductResources(Resource, additionalUnits);
            if (resources < 1) return false;
            Escrow += resources;
            Units += resources;
            return true;
        }

        public int DeductFromEscrow(int amount)
        {
            if (!Locked) return 0;
            if (Escrow < amount) return 0;

            Escrow -= amount;
            if (Escrow < 0)
            {
                Escrow += amount;
                return 0;
            }

            Units -= amount;
            return amount;
        }

        public void ReturnToEscrow(int amount)
        {
            if (amount < 1) return;
            Escrow += amount;
            Units += amount;
        }

        public bool Destroy()
        {
            bool tryDestroy = Market.RemoveSellOrder(this);
            if (tryDestroy) Village.AddResources(Resource, Escrow);
            return tryDestroy;
        }
    }
}
