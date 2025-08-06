using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TechStore_BE.Models
{
    [Table("Products")]
    public class Products
    {
        [Key]
        [Column("product_id")]
        public int product_id { get; set; }

        [Column("product_name")]
        public string? product_name { get; set; }

        [Column("brand_id")]
        public int brand_id { get; set; }

        [Column("category_id")]
        public int category_id { get; set; }

        [Column("price")]
        public decimal price { get; set; }

        [Column("quantity")]
        public int quantity { get; set; }

        [Column("description")]
        public string? description { get; set; }

        [Column("image_url")]
        public string? image_url { get; set; }

        // Navigation property
        [JsonIgnore]
        [ForeignKey("brand_id")]
        public virtual Brands? Brand { get; set; }

        [JsonIgnore]
        [ForeignKey("category_id")]
        [InverseProperty("Products")]
        public virtual Categories? Category { get; set; }

        [JsonIgnore]
        public virtual ICollection<ProductReviews>? ProductReviews { get; set; }

        [JsonIgnore]
        public virtual ICollection<Order_Details>? OrderDetails { get; set; }
    }
}
