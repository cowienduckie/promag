using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace IdentityServer.Common.Attributes;

[SuppressMessage("Usage", "ASP0019:Suggest using IHeaderDictionary.Append or the indexer")]
public class SecurityHeadersAttribute : ActionFilterAttribute
{
    public override void OnResultExecuting(ResultExecutingContext context)
    {
        var result = context.Result;

        if (result is not ViewResult)
        {
            return;
        }

        // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/X-Content-Type-Options
        if (!context.HttpContext.Response.Headers.ContainsKey("X-Content-Type-Options"))
        {
            context.HttpContext.Response.Headers.Add("X-Content-Type-Options", "nosniff");
        }

        // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/X-Frame-Options
        if (!context.HttpContext.Response.Headers.ContainsKey("X-Frame-Options"))
        {
            context.HttpContext.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN");
        }

        // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Content-Security-Policy
        var csp =
            @"default-src 'self'; object-src 'none'; frame-ancestors 'none'; sandbox allow-forms allow-same-origin allow-scripts; base-uri 'self';";
        csp += "style-src 'self' https://fonts.googleapis.com; font-src 'self' https://fonts.gstatic.com;";

        // and once again for IE
        if (!context.HttpContext.Response.Headers.ContainsKey("X-Content-Security-Policy"))
        {
            context.HttpContext.Response.Headers.Add("X-Content-Security-Policy", csp);
        }

        // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Referrer-Policy
        const string referrerPolicy = "no-referrer";
        if (!context.HttpContext.Response.Headers.ContainsKey("Referrer-Policy"))
        {
            context.HttpContext.Response.Headers.Add("Referrer-Policy", referrerPolicy);
        }
    }
}