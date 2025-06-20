using System.Windows;
using ChmlFrp.SDK.API;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Wpf.Ui.Appearance;

namespace CAT2.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty] private bool _isDarkTheme;
    [ObservableProperty] private string _assemblyName = Model.AssemblyName;

    public MainWindowViewModel()
    {
        Model.MainClass.Loaded += async (_, _) =>
        {
            if (ApplicationThemeManager.GetSystemTheme() == SystemTheme.Dark) ThemesChanged();

            Model.MainClass.RootNavigation.Navigate("登录");

            await Sign.Signin();
            if (Sign.IsSignin)
            {
                Model.MainClass.LoginItem.Visibility = Visibility.Collapsed;
                Model.MainClass.UserItem.Visibility = Visibility.Visible;
                Model.MainClass.TunnelItem.Visibility = Visibility.Visible;
                Model.MainClass.RootNavigation.Navigate("用户页");
            }

            Model.MainClass.Topmost = false;
            Model.UpdateApp();
        };

        SystemEvents.UserPreferenceChanged += (_, _) =>
        {
            var theme = ApplicationThemeManager.GetSystemTheme() == SystemTheme.Light;
            ApplicationThemeManager.Apply(theme ? ApplicationTheme.Light : ApplicationTheme.Dark);
            IsDarkTheme = !theme;
        };
    }

    [RelayCommand]
    private void ThemesChanged()
    {
        var theme = ApplicationThemeManager.GetAppTheme() == ApplicationTheme.Light;
        ApplicationThemeManager.Apply(theme ? ApplicationTheme.Dark : ApplicationTheme.Light);
        IsDarkTheme = theme;
    }

    [RelayCommand]
    private static void MinimizeThis()
    {
        Model.MainClass.WindowState = WindowState.Minimized;
    }

    [RelayCommand]
    private static void CloseThis()
    {
        Model.MainClass.Close();
    }
}