using Sample.Plugin.Promotions.Components;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Coupons;
using Sitecore.Commerce.Plugin.Pricing;
using Sitecore.Framework.Rules;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sample.Plugin.Promotions.Actions
{
    [EntityIdentifier("CustomAmountOffAction")]
    public class CustomAmountOffAction : ICartLineAction, ICartsAction, IAction
    {
        public IRuleValue<string> Tag { get; set; }
        public IRuleValue<Decimal> AmountOff { get; set; }
        public IRuleValue<bool> BasePrice { get; set; }
        public IRuleValue<bool> ActivationPrice { get; set; }
        public IRuleValue<bool> DeliveryPrice { get; set; }

        public void Execute(IRuleExecutionContext context)
        {
            CommerceContext commerceContext = context.Fact<CommerceContext>((string)null);
            CommerceContext commerceContext1 = commerceContext;
            Cart cart = commerceContext1 != null ? commerceContext1.GetObjects<Cart>().FirstOrDefault<Cart>() : (Cart)null;
            CommerceContext commerceContext2 = commerceContext;
            CartTotals totals = commerceContext2 != null ? commerceContext2.GetObjects<CartTotals>().FirstOrDefault<CartTotals>() : (CartTotals)null;
            if (cart == null || !cart.Lines.Any<CartLineComponent>() || (totals == null || !totals.Lines.Any<KeyValuePair<string, Totals>>()))
                return;

            List<CartLineComponent> list = new List<CartLineComponent>();
            //Get the valid cartline items that matches specific tag provided
            foreach (var cartLine in cart.Lines.Where(x => x.HasComponent<CartProductComponent>()))
            {
                var firstOrDefault = cartLine.GetComponent<CartProductComponent>().Tags.FirstOrDefault(t => t.Name == this.Tag.Yield(context));
                if (!string.IsNullOrEmpty(firstOrDefault?.Name))
                {
                    list.Add(cartLine);
                }
            }
            if (!list.Any<CartLineComponent>())
                return;

            list.ForEach((Action<CartLineComponent>)(line =>
            {
                if (!totals.Lines.ContainsKey(line.Id))
                    return;
                PropertiesModel propertiesModel = commerceContext.GetObject<PropertiesModel>();
                string discountPolicy = commerceContext.GetPolicy<KnownCartAdjustmentTypesPolicy>().Discount;

                //Extract the coupon code in-order to associate coupon code with adjustment (1 to 1 mapping)
                string couponCode = string.Empty;
                if (cart.HasComponent<CartCouponsComponent>())
                {
                    if (cart.GetComponent<CartCouponsComponent>().List != null && cart.GetComponent<CartCouponsComponent>().List.Any())
                        couponCode = cart.GetComponent<CartCouponsComponent>().List.FirstOrDefault(c => c.Promotion.EntityTarget == (string)propertiesModel?.GetPropertyValue("PromotionId"))?.CouponId;
                }
                
                //calculate discounts on specified custom pricing
                if (line.HasComponent<CustomPriceInfoComponent>())
                {
                    var customPriceInfo = line.GetComponent<CustomPriceInfoComponent>();
                    Decimal discount = 0;
                    if (this.BasePrice.Yield(context) && customPriceInfo.BasePrice > 0)
                    {
                        discount += this.AmountOff.Yield(context);
                    }
                    if (this.ActivationPrice.Yield(context) && customPriceInfo.ActivationPrice > 0)
                    {
                        discount += this.AmountOff.Yield(context);
                    }
                    if (this.DeliveryPrice.Yield(context) && customPriceInfo.DeliveryPrice > 0)
                    {
                        discount += this.AmountOff.Yield(context);
                    }
                    if (commerceContext.GetPolicy<GlobalPricingPolicy>().ShouldRoundPriceCalc)
                        discount = Decimal.Round(discount, commerceContext.GetPolicy<GlobalPricingPolicy>().RoundDigits, commerceContext.GetPolicy<GlobalPricingPolicy>().MidPointRoundUp ? MidpointRounding.AwayFromZero : MidpointRounding.ToEven);
                    Decimal amount = discount * Decimal.MinusOne;

                    //Create an adjustment and associate coupon code with coupon description delimited by '|' [Workaround for known issue with Promotion Plugin]
                    IList<AwardedAdjustment> adjustments = line.Adjustments;
                    adjustments.Add((AwardedAdjustment)new CartLineLevelAwardedAdjustment()
                    {
                        Name = (propertiesModel?.GetPropertyValue("PromotionText") as string ?? discountPolicy),
                        DisplayName = couponCode + "|" + (propertiesModel?.GetPropertyValue("PromotionCartText") as string ?? discountPolicy),
                        Adjustment = new Money(commerceContext.CurrentCurrency(), amount),
                        AdjustmentType = discountPolicy,
                        IsTaxable = false,
                        AwardingBlock = nameof(CustomAmountOffAction)
                    });
                    totals.Lines[line.Id].SubTotal.Amount = totals.Lines[line.Id].SubTotal.Amount + amount;
                    line.GetComponent<MessagesComponent>().AddMessage(commerceContext.GetPolicy<KnownMessageCodePolicy>().Promotions, string.Format("PromotionApplied: {0}", propertiesModel?.GetPropertyValue("PromotionId") ?? (object)nameof(CustomAmountOffAction)));
                }
            }));
        }
    }
}
