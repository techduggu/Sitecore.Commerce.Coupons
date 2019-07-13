using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Commerce.Models
{
    [Serializable]
    public class PromotionModel
    {
        public string CouponCode { get; set; }

        public string DisplayCartText { get; set; }
    }
}