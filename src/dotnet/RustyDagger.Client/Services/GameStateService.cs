using RustyDagger.Shared.Dto;

namespace RustyDagger.Client.Services;

public class GameStateService
{
    public string? AccountName { get; set; }
    public int? ActiveHeroId { get; set; }
    public GameStateResponse? CurrentState { get; set; }
    public List<string> GameLog { get; set; } = new();

    public event Action? OnStateChanged;

    public void NotifyStateChanged() => OnStateChanged?.Invoke();

    public void AddLog(string message)
    {
        GameLog.Add(message);
        if (GameLog.Count > 50) GameLog.RemoveAt(0);
        NotifyStateChanged();
    }

    public void AddLog(IEnumerable<string> messages)
    {
        foreach (var msg in messages) GameLog.Add(msg);
        while (GameLog.Count > 50) GameLog.RemoveAt(0);
        NotifyStateChanged();
    }

    public void SetState(GameStateResponse state)
    {
        CurrentState = state;
        if (state.Log.Count > 0) AddLog(state.Log);
        else NotifyStateChanged();
    }

    public void Clear()
    {
        AccountName = null;
        ActiveHeroId = null;
        CurrentState = null;
        GameLog.Clear();
        NotifyStateChanged();
    }
}
