using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using ChmlFrp.SDK;
using ChmlFrp.SDK.API;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wpf.Ui.Controls;

namespace CAT2;

public partial class LoginPageViewModel : ObservableObject
{
    [ObservableProperty] private string _password = User.Password;
    [ObservableProperty] private string _username = User.Username;

    [RelayCommand]
    private async Task LoginClick()
    {
        if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
        {
            Constant.ShowTip("登录错误", "请输入用户名和密码", ControlAppearance.Danger, SymbolRegular.TagError24);
            return;
        }

        var msg = await Sign.Signin(Username, Password);

        if (Sign.IsSignin)
        {
            Constant.MainWindow.LoginItem.Visibility = Visibility.Collapsed;
            Constant.MainWindow.UserItem.Visibility = Visibility.Visible;
            Constant.MainWindow.RootNavigation.Navigate("用户页");
            Constant.ShowTip("登录成功！", $"欢迎回来，{Username}！", ControlAppearance.Success,
                SymbolRegular.PresenceAvailable24);
        }
        else
        {
            Constant.ShowTip("登录错误", $"{msg}", ControlAppearance.Danger, SymbolRegular.TagError24);
        }
    }

    [RelayCommand]
    private async Task RegisterClick()
    {
        Constant.ShowTip("跳转至网页中...", "请稍等...", ControlAppearance.Info, SymbolRegular.Tag24);
        await Task.Delay(500);
        Process.Start(new ProcessStartInfo("https://panel.chmlfrp.cn/sign"));
    }
}