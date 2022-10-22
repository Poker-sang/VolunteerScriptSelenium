using System.Reactive.Linq;
using System.Text.Json;
using Mirai.Net.Data.Messages.Receivers;
using Mirai.Net.Sessions;

namespace VolunteerScript;

public static partial class Program
{
    private static Config _config = null!;

    public static async Task Main()
    {
        try
        {
            _config = GetConfig();

            _ = EdgeDriverManager.GetEdgeDriver();

            using var bot = new MiraiBot
            {
                Address = $"{_config.IpAddress}:{_config.Port}",
                QQ = _config.QqBot.ToString(),
                VerifyKey = _config.VerifyKey
            };

            await bot.LaunchAsync();

            _ = bot.MessageReceived
                .OfType<GroupMessageReceiver>()
                .Subscribe(OnGroupMessage);

            while (true)
            {
                if (Console.ReadLine() == "exit")
                    return;
            }
        }
        finally
        {
            EdgeDriverManager.Quit();
        }
    }

    private static Config GetConfig()
    {
        if (File.Exists("config.json"))
            if (JsonSerializer.Deserialize
                   <Config>(File.ReadAllText("config.json")) is { } config)
                return config;

        throw new("需要设置");
    }
}
