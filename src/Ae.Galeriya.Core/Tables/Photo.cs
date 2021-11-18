using Ae.Galeriya.Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ae.Galeriya.Core.Tables
{
    public sealed class Photo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public uint PhotoId { get; set; }
        public string ObjectKey { get; set; }
        public string FileName { get; set; }
        public Guid Blob { get; set; }
        public Guid Thumbnail { get; set; }
        public ulong FileSize { get; set; }
        public uint Width { get; set; }
        public uint Height { get; set; }
        public string Name { get; set; }
        public string Comment { get; set; }
        public string CentreOfInterest { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string Software { get; set; }
        public float? Latitude { get; set; }
        public float? Longitude { get; set; }
        public float Duration { get; set; }
        public MediaOrientation Orientation { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public DateTimeOffset UpdatedOn { get; set; }

        public ICollection<Category> Categories { get; set; }
    }
}
