using System;
using System.Collections.Generic;

namespace Models.Models
{
    public partial class OrderDetail
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int FoodId { get; set; }
        public int Quantity { get; set; }

        public virtual Menu Food { get; set; } = null!;
        public virtual Order Order { get; set; } = null!;
    }
}
