using API.Entities;
using System;
using System.Collections.Generic;

namespace API.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Features { get; set; }
        public bool Available { get; set; }
        public double Amount { get; set; }
        public string PhotoUrl { get; set; }
    }

    public class ProductMiniDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Amount { get; set; }
        public string PhotoUrl { get; set; }
    }

    public class CategoryProductDto
    {
        public string Category { get; set; }
        public List<ProductMiniDto> Products { get; set; } = new();
    }
    public class ProductFavorite
    {
        public int Id { get; set; }
        public string UserId { get; set; } // assuming using Identity
        public int ProductId { get; set; }
        public DateTime FavoritedAt { get; set; }
        public Product Product { get; set; }
        public User User { get; set; } // assuming ApplicationUser : IdentityUser
    }
}
