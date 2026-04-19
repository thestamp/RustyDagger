using System.Net.Http.Headers;
using System.Net.Http.Json;
using RustyDagger.Shared.Dto;

namespace RustyDagger.Client.Services;

public class ApiClient
{
    private readonly HttpClient _http;
    private string? _token;

    public ApiClient(HttpClient http) => _http = http;

    public bool IsAuthenticated => !string.IsNullOrEmpty(_token);

    public void SetToken(string token)
    {
        _token = token;
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    public void ClearToken()
    {
        _token = null;
        _http.DefaultRequestHeaders.Authorization = null;
    }

    // Auth
    public async Task<AuthResponse> LoginAsync(LoginRequest req) =>
        await PostAsync<AuthResponse>("api/auth/login", req);

    public async Task<AuthResponse> RegisterAsync(RegisterRequest req) =>
        await PostAsync<AuthResponse>("api/auth/register", req);

    // Hero
    public async Task<List<HeroSummary>> GetHeroesAsync() =>
        await GetAsync<List<HeroSummary>>("api/hero") ?? new();

    public async Task<HeroSummary?> CreateHeroAsync(CreateHeroRequest req) =>
        await PostAsync<HeroSummary>("api/hero", req);

    // Game
    public async Task<GameStateResponse?> GetGameStateAsync(int heroId) =>
        await GetAsync<GameStateResponse>($"api/game/{heroId}/state");

    public async Task<GameStateResponse?> TravelAsync(int heroId, string destination) =>
        await PostAsync<GameStateResponse>($"api/game/{heroId}/travel",
            new GameActionRequest { HeroId = heroId, Target = destination });

    public async Task<GameStateResponse?> QuestAsync(int heroId) =>
        await PostAsync<GameStateResponse>($"api/game/{heroId}/quest", new { });

    public async Task<GameStateResponse?> CombatAsync(int heroId, string action) =>
        await PostAsync<GameStateResponse>($"api/game/{heroId}/combat",
            new GameActionRequest { HeroId = heroId, Action = action });

    public async Task<GameStateResponse?> RestAsync(int heroId) =>
        await PostAsync<GameStateResponse>($"api/game/{heroId}/rest", new { });

    // Rankings
    public async Task<List<RankingEntry>> GetRankingsAsync() =>
        await GetAsync<List<RankingEntry>>("api/ranking") ?? new();

    private async Task<T?> GetAsync<T>(string url)
    {
        var response = await _http.GetAsync(url);
        if (!response.IsSuccessStatusCode) return default;
        return await response.Content.ReadFromJsonAsync<T>();
    }

    private async Task<T> PostAsync<T>(string url, object body) where T : new()
    {
        var response = await _http.PostAsJsonAsync(url, body);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(error);
        }
        return await response.Content.ReadFromJsonAsync<T>() ?? new T();
    }
}
