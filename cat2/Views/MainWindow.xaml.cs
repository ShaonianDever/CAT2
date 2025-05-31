using System.Threading.Tasks;
using Microsoft.Win32;

namespace CAT2.Views;

public partial class MainWindow
{
    public Snackbar GlobalSnackbar;

    public MainWindow()
    {
        InitializeComponent();
        SystemEvents.UserPreferenceChanged += OnSystemThemeChanged;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        // 系统主题
        if (ApplicationThemeManager.GetSystemTheme() is SystemTheme.Dark) ThemeButton.IsChecked = true;

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

    private async void OnSystemThemeChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        ThemeButton.IsChecked = !ThemeButton.IsChecked;
        SystemEvents.UserPreferenceChanged -= OnSystemThemeChanged;
        await Task.Delay(100);
        SystemEvents.UserPreferenceChanged += OnSystemThemeChanged;
    }

    private void ThemesChanged(object sender, RoutedEventArgs e)
    {
        ApplicationThemeManager.Apply(ApplicationThemeManager.GetAppTheme() is ApplicationTheme.Dark
            ? ApplicationTheme.Light
            : ApplicationTheme.Dark);
    }

    private void CloseThis(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void MinimizeThis(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }
}