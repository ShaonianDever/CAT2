using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using Wpf.Ui.Controls;

namespace CAT2.ViewModels;

public partial class LoginPageViewModel : ObservableObject
{
    [ObservableProperty] private bool _isButtonEnabled;
    [ObservableProperty] private string _password;
    [ObservableProperty] private string _username;

    partial void OnUsernameChanged(string value)
    {
        IsButtonEnabled = !string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(Password);
    }

    partial void OnPasswordChanged(string value)
    {
        IsButtonEnabled = !string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(Username);
    }

    [RelayCommand]
    private async Task LoginClick()
    {
        var msg = await Sign.Signin(Username, Password);

        if (Sign.IsSignin)
        {
            Model.MainClass.LoginItem.Visibility = Visibility.Collapsed;
            Model.MainClass.UserItem.Visibility = Visibility.Visible;
            Model.MainClass.TunnelItem.Visibility = Visibility.Visible;
            Model.MainClass.RootNavigation.Navigate("用户页");
            Model.ShowTip(
                "登录成功！",
                $"欢迎回来，{Username}！",
                ControlAppearance.Success,
                SymbolRegular.PresenceAvailable24);
        }
        else
        {
            Model.ShowTip(
                "登录错误",
                $"{msg}",
                ControlAppearance.Danger,
                SymbolRegular.TagError24);
        }
    }

    [RelayCommand]
    private async Task RegisterClick()
    {
        Model.ShowTip(
            "跳转至网页中...",
            "请稍等...",
            ControlAppearance.Info,
            SymbolRegular.Tag24);

        await Task.Delay(500);
        Process.Start(new ProcessStartInfo("https://panel.chmlfrp.cn/sign") { UseShellExecute = true });
    }
}