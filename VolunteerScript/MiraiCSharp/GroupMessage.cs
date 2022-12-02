using Mirai.CSharp.HttpApi.Handlers;
using Mirai.CSharp.HttpApi.Models.EventArgs;
using Mirai.CSharp.HttpApi.Parsers.Attributes;
using Mirai.CSharp.HttpApi.Parsers;
using Mirai.CSharp.HttpApi.Session;

namespace VolunteerScript.MiraiCSharp;

[RegisterMiraiHttpParser(typeof(DefaultMappableMiraiHttpMessageParser<IGroupMessageEventArgs, GroupMessageEventArgs>))]
public class GroupMessage : IMiraiHttpMessageHandler<IGroupMessageEventArgs>
{
    public async Task HandleMessageAsync(IMiraiHttpSession session, IGroupMessageEventArgs e) // 法1: 使用 IMessageBase[]
    {

    }
}
