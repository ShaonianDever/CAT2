using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using ChmlFrp.SDK.API;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wpf.Ui.Controls;

namespace CAT2.ViewModels;

public partial class LoginPageViewModel : ObservableObject
{
    [ObservableProperty] private string _password;
    [ObservableProperty] private string _username;

    [RelayCommand]
    private async Task LoginClick()
    {
        if (string.IsNullOrWhiteSpace(Username) && string.IsNullOrWhiteSpace(Password))
        {
            Model.ShowTip(
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
        Process.Start(new ProcessStartInfo("https://panel.chmlfrp.cn/sign"));
    }
}