using System.Reactive.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Mirai.CSharp.Builders;
using Mirai.Net.Data.Messages.Receivers;
using Mirai.Net.Sessions;
using Mirai.CSharp.HttpApi.Builder;
using Mirai.CSharp.HttpApi.Invoking;
using Mirai.CSharp.HttpApi.Options;
using Mirai.CSharp.HttpApi.Session;
using VolunteerScript.Services;
using VolunteerScript.Utilities;

namespace VolunteerScript;

public static partial class Program
{
    public static Config Config = null!;

    private enum Mode
    {
        MiraiNet,
        MiraiCSharp,
        Local
    }

    private static readonly Mode _bot = Mode.MiraiCSharp;

    public static async Task Main()
    {
        try
        {
            static void Block()
            {
                while (true)
                    if (Console.ReadLine() is "exit")
                        return;
            }

            HttpClientExtensions.Initialize();
            Config = GetConfig();

            _ = EdgeDriverManager.GetEdgeDriver();
            switch (_bot)
            {
                case Mode.MiraiNet:
                {
                    using var bot = new MiraiBot
                    {
                        Address = $"{Config.IpAddress}:{Config.Port}",
                        QQ = Config.QqBot.ToString(),
                        VerifyKey = Config.VerifyKey
                    };

                    await bot.LaunchAsync();

                    _ = bot.MessageReceived
                        .OfType<GroupMessageReceiver>()
                        .Subscribe(MiraiNet.GroupMessage.OnGroupMessage);

                    Block();
                    break;
                }
                case Mode.MiraiCSharp:
                {
                    var services = (IServiceProvider)new ServiceCollection()
                        .AddMiraiBaseFramework() // 表示使用基于基础框架的构建器
                        .AddHandler<MiraiCSharp.GroupMessage>() // 虽然可以把 HttpApiPlugin 作为泛型参数塞入, 但不建议这么做
                        .Services
                        .AddDefaultMiraiHttpFramework() // 表示使用 mirai-api-http 实现的构建器
                        .AddInvoker<MiraiHttpMessageHandlerInvoker>() // 使用默认的调度器
                        .AddClient<MiraiHttpSession>() // 使用默认的客户端
                        .Services
                        .Configure<MiraiHttpSessionOptions>(options =>
                        {
                            options.Host = Config.IpAddress;
                            options.Port = Config.Port; // 端口
                            options.AuthKey = Config.VerifyKey; // 凭据
                        })
                        .AddLogging()
                        .BuildServiceProvider();
                    await using var scope = services.CreateAsyncScope();
                    services = scope.ServiceProvider;
                    var session = services.GetRequiredService<IMiraiHttpSession>();
                    await session.ConnectAsync(Config.QqBot); // 填入期望连接到的机器人QQ号
                    Block();
                    break;
                }
                case Mode.Local:
                {
                    var fileSystemWatcher = new FileSystemWatcher($@"C:\Softwares\Tencent\{Config.QqBot}\FileRecv")
                    {
                        EnableRaisingEvents = true
                    };
                    fileSystemWatcher.Created += (o, e) =>
                    {
                        if (MyRegex().IsMatch(e.FullPath))
                            while (true)
                                try
                                {
                                    FillForm.From(e.FullPath);
                                    break;
                                }
                                catch (IOException)
                                {
                                    _ = Task.Delay(100);
                                }
                    };
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        finally
        {
            EdgeDriverManager.Quit();
        }
    }

    private const string ConfigPath = "config.json";

    private static Config GetConfig()
    {
        if (File.Exists(ConfigPath) &&
            JsonSerializer.Deserialize<Config>(File.ReadAllText(ConfigPath))
                is { } config)
            return config;

        throw new("需要设置");
    }

    [GeneratedRegex(@"[\\\w]+\.(webp|png|jpeg|jpg|bmp)$", RegexOptions.Compiled)]
    private static partial Regex MyRegex();
}
