﻿using Ae.Galeriya.Core.Tables;
using System.Collections.Generic;

namespace Ae.Galeriya.Web.Models
{
    public sealed class HomeModel
    {
        public IReadOnlyCollection<Category> Categories { get; set; } = new List<Category>();
        public IReadOnlyCollection<Photo> Photos { get; set; } = new List<Photo>();
    }
}
