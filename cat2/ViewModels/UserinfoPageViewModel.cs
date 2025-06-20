using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ChmlFrp.SDK.API;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wpf.Ui.Controls;

namespace CAT2.ViewModels;

public partial class UserinfoPageViewModel : ObservableObject
{
    [ObservableProperty] private string _name;
    [ObservableProperty] private string _email;
    [ObservableProperty] private string _group;
    [ObservableProperty] private string _regtime;
    [ObservableProperty] private string _integral;
    [ObservableProperty] private string _bandwidth;
    [ObservableProperty] private string _tunnelCount;
    [ObservableProperty] private Visibility _ringVisibility;
    [ObservableProperty] private Visibility _cardVisibility;
    [ObservableProperty] private BitmapImage _currentImage;
    [ObservableProperty] private bool _isFlyoutOpen;

    public UserinfoPageViewModel()
    {
        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
        timer.Tick += Loading;
        timer.Start();
        Loading(null, null);
    }

    [RelayCommand]
    private void ButtonClick()
    {
        IsFlyoutOpen = true;
    }

    private async void Loading(object sender, EventArgs e)
    {
        RingVisibility = Visibility.Visible;
        CardVisibility = Visibility.Collapsed;

        var userInfo = await User.GetUserInfo();
        if (userInfo == null)
        {
            Model.ShowTip(
                "加载用户信息失败",
                "请检查网络连接或稍后重试。",
                ControlAppearance.Danger,
                SymbolRegular.TagError24);
            RingVisibility = Visibility.Collapsed;
            return;
        }

        var tempUserImage = Path.GetTempFileName();
        if (await Http.GetFile(userInfo.userimg, tempUserImage))
        {
            CurrentImage = new BitmapImage();
            CurrentImage.BeginInit();
            CurrentImage.CacheOption = BitmapCacheOption.OnLoad;
            CurrentImage.UriSource = new Uri(tempUserImage);
            CurrentImage.EndInit();
        }

        File.Delete(tempUserImage);

        Name = userInfo.username;
        Email = userInfo.email;
        Group = $"用户组：{userInfo.usergroup}";
        Integral = $"积分：{userInfo.integral}";
        Regtime = $"注册时间：{userInfo.regtime}";
        TunnelCount = $"隧道使用：{userInfo.tunnelCount}/{userInfo.tunnel}";
        Bandwidth = $"带宽限制：国内{userInfo.bandwidth}m | 国外{userInfo.bandwidth * 4}m";

        RingVisibility = Visibility.Collapsed;
        CardVisibility = Visibility.Visible;
    }

    [RelayCommand]
    private static async Task OnSignOut()
    {
        Sign.Signout();
        Model.ShowTip(
            "已退出登录",
            "请重新登录以继续使用。",
            ControlAppearance.Info,
            SymbolRegular.SignOut24);
        await Task.Delay(1000);
        Process.Start(Assembly.GetExecutingAssembly().Location);
        Model.MainClass.Close();
    }
}