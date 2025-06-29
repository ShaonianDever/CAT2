using Microsoft.Win32;
using Wpf.Ui.Appearance;

namespace CAT2.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty] private string _assemblyName = Model.AssemblyName;
    [ObservableProperty] private bool _isDarkTheme;

    public MainWindowViewModel()
    {
        MainClass.Loaded += async (_, _) =>
        {
            if (ApplicationThemeManager.GetSystemTheme() == SystemTheme.Dark) ThemesChanged();

            MainClass.RootNavigation.Navigate("登录");

            await Sign.Signin();
            if (Sign.IsSignin)
            {
                MainClass.LoginItem.Visibility = Visibility.Collapsed;
                MainClass.UserItem.Visibility = Visibility.Visible;
                MainClass.TunnelItem.Visibility = Visibility.Visible;
                MainClass.RootNavigation.Navigate("用户页");
            }

            MainClass.Topmost = false;
            UpdateApp();
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
        MainClass.WindowState = WindowState.Minimized;
    }

    [RelayCommand]
    private static void CloseThis()
    {
        MainClass.Close();
    }
}