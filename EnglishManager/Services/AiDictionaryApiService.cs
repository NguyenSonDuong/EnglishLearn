using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using EnglishManager.Models;

namespace EnglishManager.Services;

/// <summary>
/// Triển khai gọi AI Dictionary API nội bộ tại http://localhost:8000.
/// Sử dụng IHttpClientFactory (đăng ký trong DI) để quản lý vòng đời HttpClient.
/// </summary>
public class AiDictionaryApiService : IAiDictionaryApiService
{
    private readonly HttpClient _httpClient;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public AiDictionaryApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <inheritdoc />
    public async Task<AiDictionaryResponse?> GenerateByLevelAsync(string level, List<string>? excludeWords = null, CancellationToken ct = default)
    {
        var requestBody = new
        {
            level,
            exclude_words = excludeWords ?? new List<string>()
        };

        var response = await _httpClient.PostAsJsonAsync("api/vocabulary/generate", requestBody, _jsonOptions, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AiDictionaryResponse>(_jsonOptions, ct);
    }

    /// <inheritdoc />
    public async Task<AiDictionaryResponse?> LookupWordAsync(string word, CancellationToken ct = default)
    {
        var requestBody = new
        {
            word = word.Trim()
        };

        var response = await _httpClient.PostAsJsonAsync("api/vocabulary/lookup", requestBody, _jsonOptions, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AiDictionaryResponse>(_jsonOptions, ct);
    }
}
