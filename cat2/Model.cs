using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using CAT2.Views;

namespace CAT2;

public abstract class Model
{
    public static readonly MainWindow MainClass = (MainWindow)Application.Current.MainWindow;

    public static async void ShowTip(string title, string content, ControlAppearance appearance, SymbolRegular icon)
    {
        var globalSnackbar = new Snackbar(new SnackbarPresenter())
        {
            Margin = new Thickness(45, 0, 20, 40),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Bottom,
            Timeout = TimeSpan.FromMilliseconds(2000),
            IsCloseButtonEnabled = false,
            Title = title,
            Content = content,
            Appearance = appearance,
            Icon = new SymbolIcon(icon)
            {
                FontSize = 32
            }
        };

        foreach (var child in MainClass.MainGrid.Children.OfType<Snackbar>().ToList())
            MainClass.MainGrid.Children.Remove(child);

        MainClass.MainGrid.Children.Add(globalSnackbar);
        await globalSnackbar.ShowAsync();
    }

    public static async void UpdateApp()
    {
        var jObject = await Http.GetApi("https://cat2.chmlfrp.com/update.json");
        if (jObject == null || (string)jObject["state"] != "success") return;
        if ((string)jObject["CAT2"]!["version"] == Assembly.GetExecutingAssembly().GetName().Version.ToString()) return;

        var temp = Path.GetTempFileName();
        if (!await Http.GetFile((string)jObject["CAT2"]["data"]!["url"], temp)) return;

        var processPath = Process.GetCurrentProcess().MainModule?.FileName;
        Process.Start(new ProcessStartInfo(
            "cmd.exe",
            $"/c timeout /t 1 /nobreak & move /y \"{temp}\" \"{processPath}\" & start \"\" \"{processPath}\" & exit"
        )
        {
            UseShellExecute = false,
            CreateNoWindow = true
        });

        MainClass.Close();
    }
}