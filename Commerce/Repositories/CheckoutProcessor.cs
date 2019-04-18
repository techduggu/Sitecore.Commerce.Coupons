using Commerce.Models;
using CommerceServices.Repositories;
using Sitecore.Analytics;
using System;

namespace Commerce.Repositories
{
    public class CheckoutProcessor
    {
        const string ShopName = "yourshopname";

        public static CartModel GetCartModel()
        {
            var cartManager = new CartManager(ShopName);
            try
            {
                var userid = GetCurrentUserId();
                if (!string.IsNullOrEmpty(userid))
                {
                    var cart = cartManager.GetCart(userid);
                    CartModel cartResult = new CartModel();
                    if (cart != null)
                    {
                        cartResult.Initialize(cart, cartManager);
                    }
                    return cartResult;
                }
            }
            catch (Exception ex)
            {
                Sitecore.Diagnostics.Log.Error("GetCartModel failed: ", ex, "");
            }
            return null;
        }

        public static bool AddPromotionCodeToCart(string promotionCode)
        {
            try
            {
                var cartManager = new CartManager(ShopName);
                var userid = GetCurrentUserId();
                if (!string.IsNullOrEmpty(userid))
                {
                    var result = cartManager.AddPromotionCodeToCart(userid, promotionCode);
                    return result;
                }
            }
            catch (Exception ex)
            {
                Sitecore.Diagnostics.Log.Error("AddPromotionCodeToCart failed: ", ex, "");
            }
            return false;
        }

        public static bool RemovePromotionCodeToCart(string promotionCode)
        {
            try
            {
                var cartManager = new CartManager(ShopName);
                var userid = GetCurrentUserId();
                if (!string.IsNullOrEmpty(userid))
                {
                    var result = cartManager.RemovePromotionCodeToCart(userid, promotionCode);
                    return result;
                }
            }
            catch (Exception ex)
            {
                Sitecore.Diagnostics.Log.Error("RemovePromotionCodeToCart failed: ", ex, "");
            }
            return false;
        }

        private static string GetCurrentUserId()
        {
            if (Tracker.Current != null && Tracker.Current.Contact != null && Tracker.Current.Contact.ContactId != Guid.Empty)
            {
                return Tracker.Current.Contact.ContactId.ToString();
            }

            return null;
        }
    }
}