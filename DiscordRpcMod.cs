using DiscordRPC;
using Vintagestory.API.Common;

namespace VS_DiscordRPC;

public class DiscordRpcMod : ModSystem
{
    private DiscordRpcClient _client = null!;
    private ICoreAPI _api = null!;
    private const string ApplicationId = "1306757937880109066";
    
    private DateTime _startTime;

    public override void Start(ICoreAPI api)
    {
        _api = api;
        InitializeDiscordRpc();
        
        // Initialize the starting timestamp
        _startTime = DateTime.UtcNow;

        _api.Event.RegisterGameTickListener(UpdatePresence, 1000);
    }

    private void InitializeDiscordRpc()
    {
        _client = new DiscordRpcClient(ApplicationId);
        _client.Initialize();
        
        _api.Logger.Debug("[DiscordRPC] Rich Presence Initialized");
    }
    
    private void UpdatePresence(float obj)
    {
        if (!_client.IsInitialized) return;

        var presence = new RichPresence()
        {
            Details = $"In {GetPlayerMode()} Mode",
            State = GetPlayerState(),
            Assets = new Assets
            {
                LargeImageKey = "game_icon",
                LargeImageText = "Vintage Story",
                SmallImageKey = "gear_icon",
                SmallImageText = $"Total Deaths: {GetPlayerDeaths()}",
            },
            Timestamps = new Timestamps(_startTime),
        };
        _client.SetPresence(presence);
    }

    private string GetPlayerMode()
    {
        // Get first/default online player
        var player = _api.World?.AllOnlinePlayers?.FirstOrDefault();
        if (player == null) return "Unknown";
        
        // Check the player's game mode
        switch (player.WorldData.CurrentGameMode)
        {
            case EnumGameMode.Creative:
                return "Creative";
            case EnumGameMode.Survival:
                return "Survival";
            case EnumGameMode.Spectator:
                return "Spectator";
            case EnumGameMode.Guest:
                return "Guest";
            default:
                return "Unknown";
        }
    }
    
    private string GetPlayerState()
    {
        var onlinePlayers = _api.World?.AllOnlinePlayers;
        if (onlinePlayers == null || onlinePlayers.Length == 0)
        {
            return "No Players Online";
        }

        return onlinePlayers.Length == 1 ? "Playing Solo" : $"Playing with {onlinePlayers.Length - 1} Others";
    }
    
    private int GetPlayerDeaths()
    {
        var player = _api.World?.AllOnlinePlayers?.FirstOrDefault();
        if (player == null) return 0;
        
        return player.WorldData.Deaths;
    }

    public override void Dispose()
    {
        _client.Dispose();
        base.Dispose();
    }
}