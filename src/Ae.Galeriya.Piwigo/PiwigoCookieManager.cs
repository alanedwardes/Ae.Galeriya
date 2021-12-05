using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;

namespace Ae.Galeriya.Piwigo
{
    public sealed class PiwigoCookieManager : ICookieManager
    {
        private readonly ChunkingCookieManager _cookieManager = new();

        public string GetRequestCookie(HttpContext context, string key)
        {
            if (context.Request.Query.TryGetValue(key, out var queryValue) &&
                HttpMethods.IsGet(context.Request.Method))
            {
                return queryValue;
            }

            return _cookieManager.GetRequestCookie(context, key);
        }

        public void AppendResponseCookie(HttpContext context, string key, string value, CookieOptions options)
        {
            _cookieManager.AppendResponseCookie(context, key, value, options);
        }

        public void DeleteCookie(HttpContext context, string key, CookieOptions options)
        {
            _cookieManager.DeleteCookie(context, key, options);
        }
    }
}
