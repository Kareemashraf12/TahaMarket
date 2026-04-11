using Microsoft.AspNetCore.Http;

namespace TahaMarket.Application.Services.Common
{
    public class FileUrlService
    {
        private readonly IHttpContextAccessor _http;

        public FileUrlService(IHttpContextAccessor http)
        {
            _http = http;
        }

        public string? GetFullUrl(string? path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            var request = _http.HttpContext.Request;

            return $"{request.Scheme}://{request.Host}{path}";
        }
    }
}