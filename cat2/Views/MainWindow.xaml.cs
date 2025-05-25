using System;
using System.Windows;
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
        Loaded += MainWindow_OnLoaded;
    }

    private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        // 系统主题
        ApplicationThemeManager.Apply(ApplicationThemeManager.GetSystemTheme() == SystemTheme.Dark
            ? ApplicationTheme.Dark
            : ApplicationTheme.Light);

        RootNavigation.Navigate(typeof(ProgressPage));

        GlobalSnackbar = new Snackbar(new SnackbarPresenter())
        {
            VerticalAlignment = VerticalAlignment.Bottom,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Margin = new Thickness(60, 0, 20, 40),
            Timeout = TimeSpan.FromMilliseconds(2000)
        };
        MainGrid.Children.Add(GlobalSnackbar);

        await Sign.Signin();
        if (Sign.IsSignin)
        {
            UserItem.Visibility = Visibility.Visible;
            RootNavigation.Navigate("用户页");
        }
        else
        {
            LoginItem.Visibility = Visibility.Visible;
            RootNavigation.Navigate("登录");
        }
    }

    private void ThemesChanged(object sender, RoutedEventArgs e)
    {
        ApplicationThemeManager.Apply(ApplicationThemeManager.GetAppTheme() is ApplicationTheme.Dark
            ? ApplicationTheme.Light
            : ApplicationTheme.Dark);
    }
}