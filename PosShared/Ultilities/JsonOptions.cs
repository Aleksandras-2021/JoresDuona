using System.Text.Json;


public static class JsonOptions
{
    public static readonly JsonSerializerOptions? Default = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };
}
