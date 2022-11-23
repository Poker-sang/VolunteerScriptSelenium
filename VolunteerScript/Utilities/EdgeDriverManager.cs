using OpenQA.Selenium;
using OpenQA.Selenium.Edge;

namespace VolunteerScript.Utilities;

public static class EdgeDriverManager
{
    public static EdgeDriver EdgeDriver { get; set; } = null!;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="timeout">minutes</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static EdgeDriver GetEdgeDriver(int timeout = 1)
    {
        var options = new EdgeOptions();

        options.AddArguments(
            // "--headless",
            "blink-settings=imagesEnabled=false",
            "--disable-blink-features=AutomationControlled");
        return EdgeDriver = new(EdgeDriverService.CreateDefaultService(), options, new(0, timeout, 0));

        // throw new EdgeDriverBusyException("Only one EdgeDriver instance can exist at a time.");
        // throw new("EdgeDriver is busy! You should not make two requests at the same time.");
    }

    public static IWebElement? TryFindElement(this WebDriver webDriver, By by)
    {
        try
        {
            return webDriver.FindElement(by);
        }
        catch (NoSuchElementException)
        {
            return null;
        }
    }

    public static void Quit()
    {
        if (EdgeDriver == null!)
            return;
        EdgeDriver.Quit();
        EdgeDriver.Dispose();
        EdgeDriver = null!;
        GC.Collect();
    }
}
