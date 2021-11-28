using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace Ae.Galeriya.Core.Tables
{
    public sealed class User : IdentityUser<uint>
    {
        public ICollection<Photo> CreatedPhotos { get; set; } = new List<Photo>();
        public ICollection<Category> CreatedCategories { get; set; } = new List<Category>();
        public ICollection<Tag> CreatedTags { get; set; } = new List<Tag>();
        public ICollection<Category> AccessibleCategories { get; set; } = new List<Category>();
        public ICollection<Photo> FavouritePhotos { get; set; } = new List<Photo>();
    }
}
