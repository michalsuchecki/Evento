using Microsoft.AspNetCore.Builder;

namespace Evento.Api.Framework
{
    public static class Extensions
    {
        public static IApplicationBuilder UseErrorHandler(this IApplicationBuilder appBuilder)
            => appBuilder.UseMiddleware<ErrorHandlerMiddleware>();
            
    }
}