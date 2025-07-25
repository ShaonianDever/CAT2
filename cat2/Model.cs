﻿using System.IO;
using System.Linq;
using System.Reflection;
using CAT2.Views;

namespace CAT2;

public abstract class Model
{
    public static readonly MainWindow MainClass = (MainWindow)Application.Current.MainWindow;
    public static readonly string Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString();

    public static readonly string FileVersion = Assembly
        .GetExecutingAssembly()
        .GetCustomAttribute<AssemblyFileVersionAttribute>()?
        .Version;

    public static readonly string Copyright = Assembly
        .GetExecutingAssembly()
        .GetCustomAttribute<AssemblyCopyrightAttribute>()?
        .Copyright;

    public static readonly string AssemblyName = Assembly.GetExecutingAssembly().GetName().Name;

    public static async void ShowTip(string title, string content, ControlAppearance appearance, SymbolRegular icon)
    {
        var globalSnackbar = new Snackbar
            (new SnackbarPresenter())
            {
                Margin = new Thickness(45, 0, 20, 40),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Bottom,
                Timeout = TimeSpan.FromMilliseconds(2000),
                IsCloseButtonEnabled = false,
                Icon = new SymbolIcon(icon)
                {
                    FontSize = 32
                },
                Appearance = appearance,
                Content = content,
                Title = title
            };

        foreach (var child in MainClass.SnackbarGrid.Children.OfType<Snackbar>().ToList())
            MainClass.SnackbarGrid.Children.Remove(child);

        MainClass.SnackbarGrid.Children.Add(globalSnackbar);
        await globalSnackbar.ShowAsync();
    }

    public static async void UpdateApp()
    {
        var jObject = await Http.GetApi("https://cat2.chmlfrp.com/update.json");
        if (jObject == null || (string)jObject["state"] != "success") return;
        if ((string)jObject["CAT2"]!["version"] == Version) return;

        ShowTip("发现新版本",
            "正在更新应用，请稍候...",
            ControlAppearance.Light,
            SymbolRegular.Add48);

        var temp = Path.GetTempFileName();
        if (!await Http.GetFile((string)jObject["CAT2"]["data"]!["url"], temp)) return;

        Process.Start(
            new ProcessStartInfo
            (
                "cmd.exe",
                $"""/c %WINDIR%\System32\timeout.exe /t 3 /nobreak & move /y "{temp}" "{Process.GetCurrentProcess().MainModule?.FileName}" & start "" "{Process.GetCurrentProcess().MainModule?.FileName}" """
            )
            {
                UseShellExecute = false,
                CreateNoWindow = true
            });

        Application.Current.Shutdown();
    }
}