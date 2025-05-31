using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Wpf.Ui.Controls;

namespace CAT2.Views.Pages;

public partial class UserinfoPage
{
    private readonly string _tempUserImage = Path.GetTempFileName();
    private BitmapImage _currentImage;

    public UserinfoPage()
    {
        InitializeComponent();
        LoadUserInfo(null, null);
        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
        timer.Tick += LoadUserInfo;
        timer.Start();
    }

    private async void LoadUserInfo(object sender, EventArgs e)
    {
        LoadingRing.Visibility = Visibility.Visible;
        TunnelCard.Visibility = Visibility.Collapsed;

        var userInfo = await ChmlFrp.SDK.User.GetUserInfo();
        if (userInfo != null)
        {
            if (await ChmlFrp.SDK.Constant.GetFile(userInfo.userimg, _tempUserImage))
            {
                _currentImage = new BitmapImage();
                _currentImage.BeginInit();
                _currentImage.CacheOption = BitmapCacheOption.OnLoad;
                _currentImage.UriSource = new Uri(_tempUserImage);
                _currentImage.EndInit();
                Image.Source = _currentImage;
            }

            NameBlock.Text = userInfo.username;
            EmailBlock.Text = userInfo.email;
            GroupBlock.Text = $"用户组：{userInfo.usergroup}";
            IntegralBlock.Text = $"积分：{userInfo.integral}";
            RegtimeBlock.Text = $"注册时间：{userInfo.regtime}";
            TunnelBlock.Text = $"隧道使用：{userInfo.tunnelCount}/{userInfo.tunnel}";
            BandwidthBlock.Text = $"带宽限制：国内{userInfo.bandwidth}m | 国外{userInfo.bandwidth * 4}m";
        }
        else
        {
            Constant.ShowTip(
                "加载用户信息失败",
                "请检查网络连接或稍后重试。",
                ControlAppearance.Danger,
                SymbolRegular.TagError24);
        }

        LoadingRing.Visibility = Visibility.Collapsed;
        TunnelCard.Visibility = Visibility.Visible;
    }
}