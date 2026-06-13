namespace FastBiteGroup.API.Middleware
{
    public sealed class JsonControlCharacterEscaperMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (context.Request.ContentType?.StartsWith("application/json", StringComparison.OrdinalIgnoreCase) == true &&
                (context.Request.Method == HttpMethods.Post ||
                 context.Request.Method == HttpMethods.Put ||
                 context.Request.Method == HttpMethods.Patch))
            {
                // Enable buffering so the body can be read multiple times
                context.Request.EnableBuffering();

                // Read the raw body as a string
                using var reader = new StreamReader(
                    context.Request.Body,
                    encoding: Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: true,
                    leaveOpen: true);

                var rawBody = await reader.ReadToEndAsync(context.RequestAborted);
                context.Request.Body.Position = 0;

                if (!string.IsNullOrEmpty(rawBody))
                {
                    var cleanedBody = EscapeControlCharactersInJsonStrings(rawBody, out var modified);
                    if (modified)
                    {
                        var cleanedBytes = Encoding.UTF8.GetBytes(cleanedBody);
                        var newBodyStream = new MemoryStream(cleanedBytes);
                        context.Request.Body = newBodyStream;
                    }
                }
            }

            await next(context);
        }

        private static string EscapeControlCharactersInJsonStrings(string json, out bool modified)
        {
            modified = false;
            var sb = new StringBuilder(json.Length);
            var insideString = false;
            var escaped = false;

            foreach (var c in json)
            {
                if (escaped)
                {
                    sb.Append(c);
                    escaped = false;
                    continue;
                }

                switch (c)
                {
                    case '\\':
                        sb.Append(c);
                        escaped = true;
                        continue;
                    case '"':
                        insideString = !insideString;
                        sb.Append(c);
                        continue;
                }

                if (insideString)
                {
                    switch (c)
                    {
                        // Control characters (0-31) inside JSON strings must be escaped.
                        // We handle common ones: \n (10), \r (13), \t (9)
                        case '\n':
                            sb.Append("\\n");
                            modified = true;
                            break;
                        case '\r':
                            sb.Append("\\r");
                            modified = true;
                            break;
                        case '\t':
                            sb.Append("\\t");
                            modified = true;
                            break;
                        default:
                        {
                            if (c < 32)
                            {
                                // Escape other control characters as \u00XX
                                sb.Append($"\\u{(int)c:x4}");
                                modified = true;
                            }
                            else
                            {
                                sb.Append(c);
                            }

                            break;
                        }
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }

            return modified ? sb.ToString() : json;
        }
    }

    public static class JsonControlCharacterEscaperMiddlewareExtensions
    {
        public static IApplicationBuilder UseJsonControlCharacterEscaper(this IApplicationBuilder app)
            => app.UseMiddleware<JsonControlCharacterEscaperMiddleware>();
    }
}
