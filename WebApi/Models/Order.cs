using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Models
{
    public class Order
    {
        public string Id { get; set; }
        public string UserEmail { get; set; }
        public string Url { get; set; }
        public string Content { get; set; }
        public string Message { get; set; }
        public OrderItem OrderItem { get; set; }
    }

    public class OrderItem
    {
        public int ProductId { get; set; }
        public int UserId { get; set; }
    }
}