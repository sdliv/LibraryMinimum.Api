using System.Net.Mime;
using System.Text;

namespace LibraryMinimumAPI
{
    public static class ResultExtensions
    {
        public static IResult Html(this IResultExtensions extensions, string html)
        {
            return new HtmlResult(html);
        }

        private class HtmlResult : IResult
        {
            private readonly string _html;

            public HtmlResult(string html)
            {
                _html = html;
            }

            public Task ExecuteAsync(HttpContext httpContext)
            {
                httpContext.Response.ContentType = MediaTypeNames.Text.Html;
                httpContext.Response.ContentLength = Encoding.UTF8.GetByteCount(_html);
                return httpContext.Response.WriteAsync(_html);
            }
        }
    }
}
