using CommerceServices.Repositories;
using Sitecore.Commerce.Engine.Connect.Entities;
using Sitecore.Commerce.Entities.Carts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Commerce.Models
{
    [Serializable]
    public class CartModel
    {
        //Bind this property under shoppingcart view to display Total Discount
        public decimal TotalDiscount { get; set; }

        //Bind this property under shoppingcart view to display Coupon codes applied
        //public IEnumerable<string> PromoCodes { get; set; }

        //Note: The above PromoCodes property has been commented and replaced by below as part of workaround for Promotion Plugin Issue#3 described in Known Issues with Promotion plugin blog post
        public List<PromotionModel> PromoCodes { get; set; }

        public void Initialize(CommerceCart cart, CartManager manager)
        {
            if (cart.OrderForms != null && cart.OrderForms.Any())
            {
                //this.PromoCodes = cart.OrderForms.FirstOrDefault().PromoCodes;
                var cartEntity = (Cart)cart;
                this.TotalDiscount = (cartEntity.Lines.Sum<CartLine>((Func<CartLine, Decimal>)(lineitem => ((CommerceTotal)lineitem.Total).LineItemDiscountAmount)) + ((CommerceTotal)cartEntity.Total).OrderLevelDiscountAmount);
            }

            PromoCodes = new List<PromotionModel>();
            foreach (var line in cart.Lines)
            {
                if (line.Adjustments != null && line.Adjustments.Any())
                {
                    foreach (var item in line.Adjustments)
                    {
                        if (item.Description.Contains('|'))
                        {
                            PromoCodes.Add(new PromotionModel()
                            {
                                CouponCode = item.Description.Split('|').First(),
                                DisplayCartText = item.Description.Split('|').Last()
                            });
                        }
                    }
                }
            }
        }
    }
}