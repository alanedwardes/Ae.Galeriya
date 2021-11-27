using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace Ae.Galeriya.Web.Models
{
    public class EditUserModelErrorFail : EditUserModel
    {
        public IReadOnlyList<IdentityError> Errors { get; set; } = Array.Empty<IdentityError>();
    }
}
