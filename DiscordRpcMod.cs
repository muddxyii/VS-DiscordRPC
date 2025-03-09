using DiscordRPC;
using Vintagestory.API.Common;

namespace VS_DiscordRPC;

public class DiscordRpcMod : ModSystem
{
    private const string ApplicationId = "1306757937880109066";
    private const string LargeImageKey = "game_icon";
    private const string LargeImageText = "Vintage Story";
    private const string SmallImageKey = "gear_icon";
    private const int UpdateMsInterval = 5000;

    private ICoreAPI? _api;
    private DiscordRpcClient? _client;
    private long _listenerId;

    private DateTime _startTime;

    public override void Start(ICoreAPI api)
    {
        _api = api;
        InitializeDiscordRpc();

        // Initialize the starting timestamp
        _startTime = DateTime.UtcNow;

        _listenerId = _api.Event.RegisterGameTickListener(UpdatePresence, UpdateMsInterval);
    }

    private void InitializeDiscordRpc()
    {
        try
        {
            _client = new DiscordRpcClient(ApplicationId);
            _client.Initialize();
            _api?.Logger.Debug("[DiscordRPC] Rich Presence Initialized");
        }
        catch (Exception ex)
        {
            _api?.Logger.Error("[DiscordRPC] Failed to initialize: " + ex.Message);
        }
    }

    private void UpdatePresence(float obj)
    {
        if (_client?.IsInitialized != true) return;

        try
        {
            var presence = new RichPresence
            {
                Details = $"In {GetPlayerMode()} Mode",
                State = GetPlayerState(),
                Assets = new Assets
                {
                    LargeImageKey = LargeImageKey,
                    LargeImageText = LargeImageText,
                    SmallImageKey = SmallImageKey,
                    SmallImageText = $"Total Deaths: {GetPlayerDeaths()}"
                },
                Timestamps = new Timestamps(_startTime)
            };
            _client?.SetPresence(presence);
        }
        catch (Exception ex)
        {
            _api?.Logger.Error($"[DiscordRPC] Failed to update presence: {ex.Message}");
        }
    }

    private string GetPlayerMode()
    {
        return _api?.World?.AllOnlinePlayers?.FirstOrDefault()?.WorldData.CurrentGameMode switch
        {
            EnumGameMode.Creative => "Creative",
            EnumGameMode.Survival => "Survival",
            EnumGameMode.Spectator => "Spectator",
            EnumGameMode.Guest => "Guest",
            _ => "Unknown"
        };
    }

    private string GetPlayerState()
    {
        var onlinePlayers = _api?.World?.AllOnlinePlayers;
        if (onlinePlayers == null || onlinePlayers.Length == 0) return "No Players Online";

        return onlinePlayers.Length == 1 ? "Playing Solo" : $"Playing with {onlinePlayers.Length - 1} Others";
    }

    private int GetPlayerDeaths()
    {
        var player = _api?.World?.AllOnlinePlayers?.FirstOrDefault();
        if (player == null) return 0;

        return player.WorldData.Deaths;
    }

    public override void Dispose()
    {
        _api?.Event.UnregisterGameTickListener(_listenerId);
        _client?.Dispose();
        base.Dispose();
    }
}