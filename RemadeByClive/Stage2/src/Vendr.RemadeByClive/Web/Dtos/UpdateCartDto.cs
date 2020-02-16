using System;

namespace Vendr.RemadeByClive.Web.Dtos
{
    public class UpdateCartDto
    {
        public OrderLineQuantityDto[] OrderLines { get; set; }
    }

    public class OrderLineQuantityDto
    {
        public Guid Id { get; set; }

        public decimal Quantity { get; set; }
    }
}
