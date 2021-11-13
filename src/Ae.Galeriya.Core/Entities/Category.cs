using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ae.Galeriya.Core.Entities
{
    public sealed class Category
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public uint CategoryId { get; set; }
        public string Name { get; set; }
        public string Comment { get; set; }
        public bool Visible { get; set; }
        public string Status { get; set; }
        public bool Commentable { get; set; }

        public uint? ParentCategoryId { get; set; }
        [ForeignKey(nameof(ParentCategoryId))]
        public Category ParentCategory { get; set; }

        public ICollection<Photo> Photos { get; set; }
        public ICollection<Category> Categories { get; set; }
    }
}
