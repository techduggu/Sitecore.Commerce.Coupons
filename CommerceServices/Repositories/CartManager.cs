using Sitecore.Commerce.Engine.Connect.Entities;
using Sitecore.Commerce.Engine.Connect.Pipelines.Arguments;
using Sitecore.Commerce.Engine.Connect.Services.Carts;
using Sitecore.Commerce.Entities.Carts;
using Sitecore.Commerce.Services.Carts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CommerceServices.Repositories
{
    public class CartManager
    {
        private const string DefaultCartName = "Default";
        private const string CartAdjustmentTypePropertyName = "__adjustmentType";
        private const string CustomPercentOffAction = "CustomPercentOffAction";
        private const string CustomAmountOffAction = "CustomAmountOffAction";

        public CartManager(string shopName)
        {
            CartServiceProvider = new CommerceCartServiceProvider();
            ShopName = shopName;
        }

        private string ShopName { get; }
        private CommerceCartServiceProvider CartServiceProvider { get; }

        public CommerceCart GetCart(string userId)
        {
            var cartResult = LoadCart(DefaultCartName, userId);
            if (cartResult.Success && cartResult.Cart != null)
            {
                return cartResult.Cart as CommerceCart;
            }
            return null;
        }

        private CartResult LoadCart(string cartName, string userId)
        {

            var request = new LoadCartByNameRequest(ShopName, cartName, userId);

            var result = CartServiceProvider.LoadCart(request);

            return result;
        }

        public bool AddPromotionCodeToCart(string userId, string promotionCode)
        {
            //Load the cart
            var cartResult = LoadCart(DefaultCartName, userId);
            bool success = false;
            if (cartResult != null && cartResult.Success && cartResult.Cart != null)
            {
                var cart = cartResult.Cart as CommerceCart;

                //Apply coupon
                var addPromoCodeResult = CartServiceProvider.AddPromoCode(new AddPromoCodeRequest(cart, promotionCode));
                if (addPromoCodeResult.Success && addPromoCodeResult.Cart != null)
                {
                    
                    cartResult = LoadCart(DefaultCartName, userId);

                    //Double check if discount is applied (due to known issue with Promotion plugin)
                    
                    //var discount = (cartResult.Cart.Lines.Sum<CartLine>((Func<CartLine, Decimal>)(lineitem => ((CommerceTotal)lineitem.Total).LineItemDiscountAmount)) + ((CommerceTotal)cartResult.Cart.Total).OrderLevelDiscountAmount);
                    //if (discount > 0)
                    //    success = true;


                    //Note: Above code has been commented and replaced with below code to workaround the Promotion Plugin Issue#2 described in Known Issues with Promotion plugin blog post
                    if (cartResult.Cart.Lines.Any(x => x.ContainsKey(CartAdjustmentTypePropertyName)))
                    {
                        var cartLineWithAdjustment = cartResult.Cart.Lines.FirstOrDefault(x => x.ContainsKey(CartAdjustmentTypePropertyName));
                        var adjustmentType = cartLineWithAdjustment.GetPropertyValue(CartAdjustmentTypePropertyName) as string;
                        List<string> distinctAdjustmentTypes = new List<string>() { adjustmentType };
                        if (adjustmentType.Contains("|"))
                            distinctAdjustmentTypes = adjustmentType.Split('|').Distinct().ToList();
                        var totalDiscount = (cartResult.Cart.Lines.Sum<CartLine>((Func<CartLine, Decimal>)(lineitem => ((CommerceTotal)lineitem.Total).LineItemDiscountAmount)) + ((CommerceTotal)cartResult.Cart.Total).OrderLevelDiscountAmount);
                        if (totalDiscount > 0)
                            success = true;
                        else if (totalDiscount == 0 && (distinctAdjustmentTypes.Contains(CustomPercentOffAction) || distinctAdjustmentTypes.Contains(CustomAmountOffAction)))
                            success = true;
                    }
                }

                if (!success && addPromoCodeResult.Success)
                {
                    //Remove coupon if coupon has been added but without any discount (due to known issue with Promotion plugin)
                    CartServiceProvider.RemovePromoCode(new RemovePromoCodeRequest(cart, promotionCode));
                }
            }
            return success;
        }

        public bool RemovePromotionCodeToCart(string userId, string promotionCode)
        {

            var cartResult = LoadCart(DefaultCartName, userId);

            if (cartResult != null && cartResult.Success && cartResult.Cart != null)
            {
                var cart = cartResult.Cart as CommerceCart;
                var removePromoCodeResult = CartServiceProvider.RemovePromoCode(new RemovePromoCodeRequest(cart, promotionCode));
                return removePromoCodeResult.Success;
            }
            return false;
        }

    }
}