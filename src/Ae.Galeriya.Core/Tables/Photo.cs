﻿using Ae.Galeriya.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ae.Galeriya.Core.Tables
{
    [Index(nameof(Hash), IsUnique = true)]
    public sealed class Photo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public uint PhotoId { get; set; }
        [Required]
        public string FileName { get; set; } = null!;
        [Required]
        public string Hash { get; set; } = null!;
        [Required]
        public Guid Blob { get; set; }
        public Guid? SnapshotBlob { get; set; }
        [Required]
        public ulong FileSize { get; set; }
        [Required]
        public uint Width { get; set; }
        [Required]
        public uint Height { get; set; }
        [Required]
        public string Name { get; set; } = null!;
        [Required]
        public string Extension { get; set; } = null!;
        public string? Comment { get; set; }
        public string? Author { get; set; }
        public string? Make { get; set; }
        public string? Model { get; set; }
        public string? Software { get; set; }
        public float? Latitude { get; set; }
        public float? Longitude { get; set; }
        public float? Duration { get; set; }
        [Required]
        public MediaOrientation Orientation { get; set; }
        [Required]
        public DateTimeOffset CreatedOn { get; set; }
        public DateTimeOffset? UpdatedOn { get; set; }

        public ICollection<Category> Categories { get; set; } = new List<Category>();
    }
}
