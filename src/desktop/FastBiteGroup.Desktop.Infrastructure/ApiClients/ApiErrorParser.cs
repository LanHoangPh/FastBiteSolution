using System.Text.Json;

namespace FastBiteGroup.Desktop.Infrastructure.ApiClients;

internal static class ApiErrorParser
{
    public static string GetErrorMessage(string? content, string fallback = "An unexpected error occurred.")
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return fallback;
        }

        try
        {
            using var document = JsonDocument.Parse(content);
            var root = document.RootElement;

            if (root.TryGetProperty("detail", out var detail) &&
                detail.ValueKind == JsonValueKind.String)
            {
                var detailMessage = detail.GetString();
                if (!string.IsNullOrWhiteSpace(detailMessage))
                {
                    return detailMessage;
                }
            }

            if (root.TryGetProperty("errors", out var errors) &&
                errors.ValueKind == JsonValueKind.Array)
            {
                foreach (var error in errors.EnumerateArray())
                {
                    if (error.TryGetProperty("message", out var message) &&
                        message.ValueKind == JsonValueKind.String)
                    {
                        var errorMessage = message.GetString();
                        if (!string.IsNullOrWhiteSpace(errorMessage))
                        {
                            return errorMessage;
                        }
                    }
                }
            }
        }
        catch (JsonException)
        {
            return fallback;
        }

        return fallback;
    }
}
