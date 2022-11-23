using System.Data;
using System.Text;
using OpenQA.Selenium;
using SixLabors.ImageSharp;
using VolunteerScript.Utilities;

namespace VolunteerScript.Services;

public static class FillForm
{
    public static void From(Stream stream)
    {
        try
        {
            var image = Image.Load(stream).QrDecode()[0];
            var form = Encoding.Default.GetString(image);
            Fill(form, Program.Config);
        }
        catch (Exception)
        {
            return;
        }
    }

    public static void From(string path)
    {
        try
        {
            var image = Image.Load(path).QrDecode()[0];
            var form = Encoding.Default.GetString(image);
            Fill(form, Program.Config);
        }
        catch (Exception)
        {
            return;
        }
    }

    public static void Fill(string url, Config config)
    {
        var driver = EdgeDriverManager.EdgeDriver;

        driver.Url = url;
        var elements = driver.FindElements(By.XPath("/html/body/div[1]/form/ul/child::*"));

        foreach (var element in elements)
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
                    var value = name switch
                    {
                        "姓名" => config.Name,
                        "学号" => config.Id.ToString(),
                        "年级" => config.Grade,
                        "专业" => config.Major,
                        "班级" => config.Class,
                        "专业班级" => config.Major + config.Class,
                        "手机" => config.Tel.ToString(),
                        "电话" => config.Tel.ToString(),
                        "联系方式" => config.Tel.ToString(),
                        "QQ" => config.Qq.ToString(),
                        _ => name.Contains("专业班级")
                            ? config.Major + config.Class
                            : new DataTable().Compute(name.TrimEnd('='), "").ToString()
                    };

                    input.SendKeys(value);
                }

            }
            catch (Exception)
            {
                continue;
            }
    }
}
