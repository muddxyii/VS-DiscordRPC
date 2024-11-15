using DiscordRPC;
using Vintagestory.API.Common;

namespace VS_DiscordRPC;

public class DiscordRpcMod : ModSystem
{
    private DiscordRpcClient _client;
    private ICoreAPI _api;
    private const string APPLICATION_ID = "1306757937880109066";

    public override void Start(ICoreAPI api)
    {
        _api = api;
        InitializeDiscordRpc();

        _api.Event.RegisterGameTickListener(UpdatePresence, 250);
    }

    private void InitializeDiscordRpc()
    {
        _client = new DiscordRpcClient(APPLICATION_ID);
        _client.Initialize();
        
        _api.Logger.Debug("[DiscordRPC] Rich Presence Initialized");
    }
    
    private void UpdatePresence(float obj)
    {
        if (!_client.IsInitialized) return;

        var presence = new RichPresence()
        {
            Details = $"In {GetPlayerMode()} mode",
            State = GetPlayerState(),
            Assets = new Assets
            {
                LargeImageKey = "game_icon",
                LargeImageText = "Vintage Story",
                SmallImageKey = "gear_icon",
                SmallImageText = $"Health: {GetPlayerHealth()}%",
            },
            Timestamps = new Timestamps(DateTime.UtcNow),
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
        // TODO: Get player count
        
        return "Playing Solo";
    }
    
    private int GetPlayerHealth()
    {
        // TODO: Get player health
        
        return 100;
    }

    public override void Dispose()
    {
        _client.Dispose();
        base.Dispose();
    }
}