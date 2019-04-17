using Sitecore.Commerce.Engine.Connect.Entities;
using Sitecore.Commerce.Engine.Connect.Pipelines.Arguments;
using Sitecore.Commerce.Engine.Connect.Services.Carts;
using Sitecore.Commerce.Entities.Carts;
using Sitecore.Commerce.Services.Carts;
using System;
using System.Linq;

namespace CommerceServices.Repositories
{
    public class CartManager
    {
        private const string DefaultCartName = "Default";

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
                    //Double check if discount is applied (due to known issue with Promotion plugin)
                    cartResult = LoadCart(DefaultCartName, userId);
                    var discount = (cartResult.Cart.Lines.Sum<CartLine>((Func<CartLine, Decimal>)(lineitem => ((CommerceTotal)lineitem.Total).LineItemDiscountAmount)) + ((CommerceTotal)cartResult.Cart.Total).OrderLevelDiscountAmount);
                    if (discount > 0)
                        success = true;
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