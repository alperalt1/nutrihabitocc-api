using System.Text.Json;

namespace Nutrihabitocc.Domain.Helpers;

public static class JsonHelper
{
    public static string NormalizeRawJson(JsonElement element, string fieldName)
    {
        if (element.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            throw new InvalidOperationException($"El campo {fieldName} es obligatorio.");
        }

        if (element.ValueKind == JsonValueKind.String)
        {
            var rawValue = element.GetString();
            return ValidateRawJson(rawValue, fieldName);
        }

        return ValidateRawJson(element.GetRawText(), fieldName);
    }

    public static string ValidateRawJson(string? rawJson, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(rawJson))
        {
            throw new InvalidOperationException($"El campo {fieldName} es obligatorio.");
        }

        try
        {
            using var document = JsonDocument.Parse(rawJson);
            if (document.RootElement.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
            {
                throw new InvalidOperationException($"El campo {fieldName} debe contener un JSON válido.");
            }

            return document.RootElement.GetRawText();
        }
        catch (JsonException)
        {
            throw new InvalidOperationException($"El campo {fieldName} no contiene un JSON válido.");
        }
    }

    public static string NormalizeStructureJson(string? rawJson)
    {
        var validatedJson = ValidateRawJson(rawJson, "Estructura");

        try
        {
            using var document = JsonDocument.Parse(validatedJson);
            var root = document.RootElement;

            var hasOwnSections =
                root.ValueKind == JsonValueKind.Object &&
                (root.TryGetProperty("secciones", out _) || root.TryGetProperty("sections", out _));

            if (root.ValueKind == JsonValueKind.Object &&
                !hasOwnSections &&
                root.TryGetProperty("estructura", out var nested) &&
                nested.ValueKind == JsonValueKind.Object &&
                (nested.TryGetProperty("secciones", out _) ||
                 nested.TryGetProperty("sections", out _)))
            {
                return nested.GetRawText();
            }

            return root.GetRawText();
        }
        catch (JsonException)
        {
            throw new InvalidOperationException("El campo Estructura no contiene un JSON válido.");
        }
    }

    public static JsonElement ParseElement(string rawJson)
    {
        if (string.IsNullOrWhiteSpace(rawJson))
        {
            using var nullDocument = JsonDocument.Parse("null");
            return nullDocument.RootElement.Clone();
        }

        try
        {
            using var document = JsonDocument.Parse(rawJson);
            return document.RootElement.Clone();
        }
        catch (JsonException)
        {
            throw new InvalidOperationException("El JSON almacenado no es válido.");
        }
    }
}
