using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ae.Galeriya.Core.Tables
{
    [Index(nameof(Name), IsUnique = true)]
    public sealed class Tag
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public uint TagId { get; set; }
        [Required]
        public string Name { get; set; } = null!;

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

        public string GenerateSlug() => Name.ToLower().Trim().Replace(' ', '-');
    }
}
