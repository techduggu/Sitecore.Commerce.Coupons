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
        public IEnumerable<string> PromoCodes { get; set; }

        public void Initialize(CommerceCart cart, CartManager manager)
        {
            if (cart.OrderForms != null && cart.OrderForms.Any())
            {
                this.PromoCodes = cart.OrderForms.FirstOrDefault().PromoCodes;
                var cartEntity = (Cart)cart;
                this.TotalDiscount = (cartEntity.Lines.Sum<CartLine>((Func<CartLine, Decimal>)(lineitem => ((CommerceTotal)lineitem.Total).LineItemDiscountAmount)) + ((CommerceTotal)cartEntity.Total).OrderLevelDiscountAmount);
            }
        }
    }
}