using System.Collections.Generic;

namespace MyPOS.Domain.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public ICollection<Order> Orders { get; set; }
    }
}
