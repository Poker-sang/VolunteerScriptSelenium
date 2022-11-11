using System.Reactive.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using Mirai.Net.Data.Messages.Receivers;
using Mirai.Net.Sessions;

namespace VolunteerScript;

public static partial class Program
{
    private static Config _config = null!;
    private const bool Bot = false;

    public static async Task Main()
    {
        try
        {
            _config = GetConfig();

            _ = EdgeDriverManager.GetEdgeDriver();
            if (Bot)
            {
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
            }
            else
            {
                var fileSystemWatcher = new FileSystemWatcher($@"C:\Softwares\Tencent\{_config.QqBot}\FileRecv")
                {
                    EnableRaisingEvents = true
                };
                fileSystemWatcher.Created += (o, e) =>
                {
                    if (Regex.IsMatch(e.FullPath, @"[\\\w]+\.(webp|png|jpeg|jpg|bmp)$", RegexOptions.Compiled))
                        while (true)
                            try
                            {
                                Utility(e.FullPath);
                                break;
                            }
                            catch (IOException)
                            {
                                _ = Task.Delay(100);
                            }
                };
            }
            while (true)
            {
                if (Console.ReadLine() is "exit")
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
