using System;
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

        [Required]
        public uint CreatedById { get; set; }
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }
        [Required]
        public DateTimeOffset CreatedOn { get; set; }

        public uint? UpdatedById { get; set; }
        [ForeignKey(nameof(UpdatedById))]
        public User? UpdatedBy { get; set; }
        public DateTimeOffset? UpdatedOn { get; set; }

        public ICollection<Photo> Photos { get; set; } = new List<Photo>();
        public ICollection<Category> Categories { get; set; } = new List<Category>();
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
