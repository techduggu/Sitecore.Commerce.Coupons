using Sitecore.Commerce.Core;

namespace Sample.Plugin.Promotions.Components
{
    //This class is created just for reference for this demo. You need to populate and add it to existing cart json.
    public class CustomPriceInfoComponent : Component
    {
        public double BasePrice { get; set; }
        public double ActivationPrice { get; set; }
        public double DeliveryPrice { get; set; }
    }
}
