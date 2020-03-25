using Assets.Scripts.Villages;

namespace Assets.Scripts.Market
{
    public class Order
    {
        public ResourceType Resource { get; }
        public decimal Price { get; set; }
        public int Units { get; set; }
        public VillageAI Village { get; }
        public bool Locked { get; set; }

        public Order(VillageAI village, ResourceType resourceType, decimal price, int units)
        {
            Village = village;
            Resource = resourceType;
            Price = price;
            Units = units;
        }
    }
}
