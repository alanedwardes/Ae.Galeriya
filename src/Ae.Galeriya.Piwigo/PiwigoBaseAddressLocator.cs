using Microsoft.AspNetCore.Http;
using System;

namespace Ae.Galeriya.Piwigo
{
    public sealed class PiwigoBaseAddressLocator : IPiwigoBaseAddressLocator
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PiwigoBaseAddressLocator(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Uri GetBaseAddress()
        {
            return new UriBuilder
            {
                Scheme = _httpContextAccessor.HttpContext.Request.Scheme,
                Host = _httpContextAccessor.HttpContext.Request.Host.Host,
                Port = _httpContextAccessor.HttpContext.Request.Host.Port ?? -1
            }.Uri;
        }
    }
}