using System;
using System.Collections.Generic;

namespace API.Entities
{
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
