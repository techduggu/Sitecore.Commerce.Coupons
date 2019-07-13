using System;
using System.Linq;
using Sitecore.Commerce.Engine.Connect.Entities;
using Sitecore.Commerce.Engine.Connect.Pipelines.Arguments;
using Sitecore.Commerce.Entities;
using Sitecore.Commerce.Entities.Carts;
using Sitecore.Commerce.Entities.Prices;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.Pricing;
using Sitecore.Data;
using Sitecore.Diagnostics;

namespace CommerceServices.Pipelines.Carts
{
    public class TranslateCartLineToEntity : Sitecore.Commerce.Engine.Connect.Pipelines.Carts.TranslateCartLineToEntity
    {
        private const string CartAdjustmentTypePropertyName = "__adjustmentType";
        public TranslateCartLineToEntity(IEntityFactory entityFactory) : base(entityFactory)
        {
        }

        protected override void TranslateProduct(TranslateCartLineToEntityRequest request, CartLineComponent source, CommerceCartLine destination, bool isSubLine = false)
        {
            Assert.ArgumentNotNull((object)request, nameof(request));
            Assert.ArgumentNotNull((object)source, nameof(source));
            Assert.ArgumentNotNull((object)destination, nameof(destination));
            CommerceCartProduct commerceCartProduct = this.EntityFactory.Create<CommerceCartProduct>("CartProduct");
            if (source.CartLineComponents != null && !string.IsNullOrEmpty(source.ItemId))
            {
                CartProductComponent productComponent = source.CartLineComponents.OfType<CartProductComponent>().FirstOrDefault<CartProductComponent>();
                if (productComponent != null)
                {
                    string[] strArray = source.ItemId.Split("|".ToCharArray());
                    commerceCartProduct.ProductCatalog = strArray[0];
                    commerceCartProduct.ProductId = productComponent.Id;
                    commerceCartProduct.DisplayName = productComponent.DisplayName;
                    commerceCartProduct.ProductName = string.IsNullOrEmpty(productComponent.ProductName) ? productComponent.DisplayName : productComponent.ProductName;
                    commerceCartProduct.SitecoreProductItemId = this.GetSitecoreItemId(strArray[1], strArray[2]);
                    destination.SetPropertyValue("_product_Images", productComponent.Image == null || string.IsNullOrEmpty(productComponent.Image.SitecoreId) ? (object)string.Empty : (object)productComponent.Image.SitecoreId);
                    commerceCartProduct.SetPropertyValue("Color", string.IsNullOrEmpty(productComponent.Color) ? (object)(string)null : (object)productComponent.Color);
                    commerceCartProduct.SetPropertyValue("Size", string.IsNullOrEmpty(productComponent.Size) ? (object)(string)null : (object)productComponent.Size);
                    commerceCartProduct.SetPropertyValue("Style", string.IsNullOrEmpty(productComponent.Style) ? (object)(string)null : (object)productComponent.Style);
                    ID result;
                    if (!string.IsNullOrEmpty(productComponent.ExternalId) && ID.TryParse(productComponent.ExternalId, out result))
                        commerceCartProduct.SitecoreProductItemId = result.ToGuid();
                }
                ItemVariationSelectedComponent selectedComponent = source.CartLineComponents.OfType<ItemVariationSelectedComponent>().FirstOrDefault<ItemVariationSelectedComponent>();
                if (selectedComponent != null)
                    commerceCartProduct.ProductVariantId = selectedComponent.VariationId;

                //Set an additional property to determine the Promotion Awarding Blocks [Promotion Plugin Issue#2 described in Known Issues with Promotion plugin blog post]
                if (source.Adjustments != null && source.Adjustments.Any())
                {
                    destination.SetPropertyValue(CartAdjustmentTypePropertyName, string.Join("|", source.Adjustments.Select(x => x.AwardingBlock)));
                }
            }
            CommercePrice commercePrice = this.EntityFactory.Create<CommercePrice>("Price");
            if (source.UnitListPrice != null)
            {
                PurchaseOptionMoneyPolicy optionMoneyPolicy = source.Policies.OfType<PurchaseOptionMoneyPolicy>().FirstOrDefault<PurchaseOptionMoneyPolicy>();
                if (optionMoneyPolicy != null && source.UnitListPrice.Amount != optionMoneyPolicy.SellPrice.Amount)
                {
                    commercePrice.CurrencyCode = optionMoneyPolicy.SellPrice.CurrencyCode;
                    commercePrice.ListPrice = optionMoneyPolicy.SellPrice.Amount;
                }
                else
                {
                    commercePrice.CurrencyCode = source.UnitListPrice.CurrencyCode;
                    commercePrice.ListPrice = source.UnitListPrice.Amount;
                }
                commercePrice.Amount = commercePrice.ListPrice;
            }
            commerceCartProduct.Price = (Price)commercePrice;
            destination.Product = (CartProduct)commerceCartProduct;
        }
    }
}