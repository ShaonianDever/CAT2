using System;
using System.Windows;
using ChmlFrp.SDK;
using ChmlFrp.SDK.API;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace CAT2;

public partial class MainWindow
{
    public Snackbar GlobalSnackbar;

    public MainWindow()
    {
        InitializeComponent();
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        // 系统主题
        ApplicationThemeManager.Apply(ApplicationThemeManager.GetSystemTheme() == SystemTheme.Dark
            ? ApplicationTheme.Dark
            : ApplicationTheme.Light);

        GlobalSnackbar = new Snackbar(new SnackbarPresenter())
        {
            Margin = new Thickness(45, 0, 20, 40),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Bottom,
            Timeout = TimeSpan.FromMilliseconds(2000),
            IsCloseButtonEnabled = false
        };
        MainGrid.Children.Add(GlobalSnackbar);

        Paths.Init("CAT2");

        await Sign.Signin();
        if (Sign.IsSignin)
        {
            UserItem.Visibility = Visibility.Visible;
            TunnelItem.Visibility = Visibility.Visible;
            RootNavigation.Navigate("用户页");
        }
        else
        {
            LoginItem.Visibility = Visibility.Visible;
            RootNavigation.Navigate("登录");
        }

        Topmost = false;
    }

    private void ThemesChanged(object sender, RoutedEventArgs e)
    {
        ApplicationThemeManager.Apply(ApplicationThemeManager.GetAppTheme() is ApplicationTheme.Dark
            ? ApplicationTheme.Light
            : ApplicationTheme.Dark);
    }
}