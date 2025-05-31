using System.Diagnostics;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using static CAT2.Views.Constant;

namespace CAT2.Views;

public partial class App
{
    public App()
    {
        Paths.Init("CAT2");
        InitializeComponent();

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
    }
}

public partial class LoginPageViewModel : ObservableObject
{
    [ObservableProperty] private string _username = User.Username;
    [ObservableProperty] private string _password = User.Password;

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
                MainWinClass.LoginItem.Visibility = Visibility.Collapsed;
                MainWinClass.UserItem.Visibility = Visibility.Visible;
                MainWinClass.TunnelItem.Visibility = Visibility.Visible;
                MainWinClass.RootNavigation.Navigate("用户页");
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