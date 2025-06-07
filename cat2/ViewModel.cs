using System.Diagnostics;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using static CAT2.Views.Constant;

namespace CAT2.ViewModels;

public partial class App
{
    public App()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            if (args.ExceptionObject is not Exception ex) return;
            Paths.WritingLog($"Error: \n{ex.Message}");
            Process.Start(new ProcessStartInfo
            {
                FileName = Paths.LogFilePath,
                UseShellExecute = true
            });
        };

        Paths.Init("CAT2");
    }
}

public partial class LoginPageViewModel : ObservableObject
{
    [ObservableProperty] private string _password = User.Password;
    [ObservableProperty] private string _username = User.Username;

    [RelayCommand]
    private async Task LoginClick()
    {
        if (string.IsNullOrWhiteSpace(Username) && string.IsNullOrWhiteSpace(Password))
        {
            ShowTip(
                "登录错误",
                "请输入用户名和密码",
                ControlAppearance.Danger,
                SymbolRegular.TagError24);
        }
        else
        {
            var msg = await Sign.Signin(Username, Password);

            if (Sign.IsSignin)
            {
                MainClass.LoginItem.Visibility = Visibility.Collapsed;
                MainClass.UserItem.Visibility = Visibility.Visible;
                MainClass.TunnelItem.Visibility = Visibility.Visible;
                MainClass.RootNavigation.Navigate("用户页");
                ShowTip(
                    "登录成功！",
                    $"欢迎回来，{Username}！",
                    ControlAppearance.Success,
                    SymbolRegular.PresenceAvailable24);
            }
            else
            {
                ShowTip(
                    "登录错误",
                    $"{msg}",
                    ControlAppearance.Danger,
                    SymbolRegular.TagError24);
            }
        }
    }

    [RelayCommand]
    private async Task RegisterClick()
    {
        ShowTip(
            "跳转至网页中...",
            "请稍等...",
            ControlAppearance.Info,
            SymbolRegular.Tag24);

        await Task.Delay(500);
        Process.Start(new ProcessStartInfo("https://panel.chmlfrp.cn/sign"));
    }
}

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty] private bool _isDarkTheme;

    [RelayCommand]
    private void MinimizeThis()
    {
        MainClass.WindowState = WindowState.Minimized;
    }

    [RelayCommand]
    private void CloseThis()
    {
        MainClass.Close();
    }

    [RelayCommand]
    private void ThemesChanged()
    {
        ApplicationThemeManager.Apply(ApplicationThemeManager.GetAppTheme() is ApplicationTheme.Dark
            ? ApplicationTheme.Light
            : ApplicationTheme.Dark);
    }

    [RelayCommand]
    private async Task Loaded()
    {
        if (ApplicationThemeManager.GetSystemTheme() is SystemTheme.Dark)
        {
            IsDarkTheme = true;
            ApplicationThemeManager.Apply(ApplicationTheme.Dark);
        }

        await Sign.Signin();
        if (Sign.IsSignin)
        {
            MainClass.UserItem.Visibility = Visibility.Visible;
            MainClass.TunnelItem.Visibility = Visibility.Visible;
            MainClass.RootNavigation.Navigate("用户页");
        }
        else
        {
            MainClass.LoginItem.Visibility = Visibility.Visible;
            MainClass.RootNavigation.Navigate("登录");
        }

        MainClass.Topmost = false;
    }
}