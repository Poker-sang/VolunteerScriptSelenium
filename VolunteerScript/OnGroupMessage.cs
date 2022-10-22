using System.Data;
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
        var file = receiver.MessageChain.Where(t => t is FileMessage)
            .Cast<FileMessage>().FirstOrDefault();
        if (file is null)
            return;

        var fileDownloadInfo = FileManager.GetFileAsync(_config.GroupObserved.ToString(), file.FileId, true).GetAwaiter().GetResult().DownloadInfo;
        var byteArray = Image.Load(fileDownloadInfo.Url.DownloadStreamAsync().GetAwaiter().GetResult()).QrDecode()[0];
        var url = System.Text.Encoding.Default.GetString(byteArray);
        FillForm(url);
    }

    private static void FillForm(string url)
    {
        var driver = EdgeDriverManager.EdgeDriver;

        driver.Url = url;
        var elements = driver.FindElements(By.XPath("/html/body/div[1]/form/ul/child::*"));

        foreach (var element in elements)
        {
            try
            {
                var label = element.FindElement(By.XPath("./label"));
                var name = label.Text.TrimStart('*');

                if (name.Contains("时间"))
                {
                    foreach (var span in element.FindElements(By.XPath("./div/span")))
                    {
                        var input = span.FindElement(By.XPath("./input"));
                        if (!input.Enabled)
                            continue;
                        var ratio = span.FindElement(By.XPath("./label"));
                        ratio.Click();
                        break;
                    }
                }
                else
                {
                    var input = element.FindElement(By.XPath("./div/input"));
                    var value = "";
                    switch (name)
                    {
                        case "姓名":
                            value = _config.Name;
                            break;
                        case "学号":
                            value = _config.Id.ToString();
                            break;
                        case "年级":
                            value = _config.Grade;
                            break;
                        case "专业":
                            value = _config.Major;
                            break;
                        case "班级":
                            value = _config.Class;
                            break;
                        case "专业班级":
                            value = _config.Major + _config.Class;
                            break;
                        case "手机":
                        case "电话":
                        case "联系方式":
                            value = _config.Tel.ToString();
                            break;
                        case "QQ":
                            value = _config.Qq.ToString();
                            break;
                        default:
                            value = new DataTable().Compute(name, "").ToString();
                            break;

                    }
                    input.SendKeys(value);
                }

            }
            catch (Exception)
            {
                continue;
            }
        }
    }
}
