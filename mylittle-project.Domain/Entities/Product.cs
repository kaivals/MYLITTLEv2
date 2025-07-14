using System.ComponentModel.DataAnnotations;

namespace mylittle_project.Domain.Entities
{
    public class Product : BaseEntity
    {
       

        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        public decimal? Price { get; set; }

        public Guid? BrandId { get; set; }

        // Relations
        public BrandProduct? Brand { get; set; }

        // NEW Many-to-Many Categories
        public ICollection<Category> Categories { get; set; } = new List<Category>();

        // Existing
        public ICollection<ProductFieldValue> FieldValues { get; set; } = new List<ProductFieldValue>();
        public ICollection<ProductReview> Reviews { get; set; } = new List<ProductReview>();
        public ICollection<ProductTag> Tags { get; set; } = new List<ProductTag>();
    }
}
