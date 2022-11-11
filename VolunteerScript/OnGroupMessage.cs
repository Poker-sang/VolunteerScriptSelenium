using System.Data;
using System.Text;
using Mirai.Net.Data.Messages.Concretes;
using Mirai.Net.Data.Messages.Receivers;
using Mirai.Net.Sessions.Http.Managers;
using OpenQA.Selenium;
using SixLabors.ImageSharp;

namespace VolunteerScript;

public static partial class Program
{
    private static void OnGroupMessage(GroupMessageReceiver receiver)
    {
        if (receiver.GroupId != _config.GroupObserved.ToString())
            return;
        var url = "";
        foreach (var message in receiver.MessageChain)
            switch (message)
            {
                case FileMessage file:
                    var fileDownloadInfo = FileManager.GetFileAsync(_config.GroupObserved.ToString(), file.FileId, true).GetAwaiter().GetResult().DownloadInfo;
                    url = fileDownloadInfo.Url;
                    break;
                case ImageMessage img:
                    url = img.Url;
                    break;
            }

        if (url is "")
            return;
        Utility(url.DownloadStreamAsync().GetAwaiter().GetResult());
    }
}
