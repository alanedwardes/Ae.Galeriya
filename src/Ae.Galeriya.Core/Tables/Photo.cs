﻿using Ae.MediaMetadata.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ae.Galeriya.Core.Tables
{
    [Index(nameof(BlobId), IsUnique = true)]
    public sealed class Photo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public uint PhotoId { get; set; }
        [Required]
        public string FileName { get; set; } = null!;
        [Required]
        public string BlobId { get; set; } = null!;
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
        [Required]
        public MediaOrientation Orientation { get; set; }
        public string? Metadata { get; set; }

        public string? Comment { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public float? Duration { get; set; }
        public string? SnapshotBlob { get; set; }
        public DateTimeOffset? TakenOn { get; set; }

        [Required]
        public uint CreatedById { get; set; }
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; } = null!;
        [Required]
        public DateTimeOffset CreatedOn { get; set; }

        public uint? UpdatedById { get; set; }
        [ForeignKey(nameof(UpdatedById))]
        public User? UpdatedBy { get; set; }
        public DateTimeOffset? UpdatedOn { get; set; }

        public ICollection<Category> Categories { get; set; } = new List<Category>();
        public ICollection<Tag> Tags { get; set; } = new List<Tag>();
        public ICollection<User> FavouritedBy { get; set; } = new List<User>();
    }
}
