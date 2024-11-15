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
        // TODO: Get player mode (Eg. Survival, Creative)
        
        return "Survival";
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