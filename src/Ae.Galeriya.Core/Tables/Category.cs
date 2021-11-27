using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ae.Galeriya.Core.Tables
{
    public sealed class Category
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public uint CategoryId { get; set; }
        [Required]
        public string Name { get; set; } = null!;
        public string? Comment { get; set; }

        public uint? CoverPhotoId { get; set; }
        [ForeignKey(nameof(CoverPhotoId))]
        public Photo? CoverPhoto { get; set; }

        public uint? ParentCategoryId { get; set; }
        [ForeignKey(nameof(ParentCategoryId))]
        public Category? ParentCategory { get; set; }

        public ICollection<Photo> Photos { get; set; } = new List<Photo>();
        public ICollection<Category> Categories { get; set; } = new List<Category>();
        public ICollection<CategoryUser> Users { get; set; } = new List<CategoryUser>();
    }

    public sealed class CategoryUser
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public uint CategoryUserId { get; set; }

        [Required]
        public uint CategoryId { get; set; }
        [ForeignKey(nameof(CategoryId))]
        public Category Category { get; set; }

        [Required]
        public string UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public IdentityUser User { get; set; }
    }
}
