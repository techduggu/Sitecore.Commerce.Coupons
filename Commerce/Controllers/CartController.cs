using Commerce.Repositories;
using Sitecore.Mvc.Controllers;
using System.Web.Mvc;

namespace Commerce.Controllers
{
    public class CartController : SitecoreController
    {
        [HttpPost]
        public JsonResult ApplyCoupon(string promotionCode)
        {
            var result = CheckoutProcessor.AddPromotionCodeToCart(promotionCode);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult RemoveCoupon(string promotionCode)
        {
            var result = CheckoutProcessor.RemovePromotionCodeToCart(promotionCode);

            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}