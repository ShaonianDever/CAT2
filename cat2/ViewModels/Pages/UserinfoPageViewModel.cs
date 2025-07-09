using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using CAT2.SDK.API;
using CAT2.SDK;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wpf.Ui.Controls;

namespace CAT2.ViewModels;

public partial class UserinfoPageViewModel : ObservableObject
{
    [ObservableProperty] private string _bandwidth;
    [ObservableProperty] private BitmapImage _currentImage;
    [ObservableProperty] private string _email;
    [ObservableProperty] private string _group;
    [ObservableProperty] private string _integral;
    [ObservableProperty] private bool _isFlyoutOpen;
    [ObservableProperty] private string _name;
    [ObservableProperty] private string _regtime;
    [ObservableProperty] private string _tunnelCount;

    public UserinfoPageViewModel()
    {
        Loading(null, null);
        var timer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(1) };
        timer.Tick += Loading;
        timer.Start();
    }

    [RelayCommand]
    private void OpenFlyout()
    {
        IsFlyoutOpen = true;
    }

    private async void Loading(object sender, EventArgs e)
    {
        try
        {
            var userInfo = await User.GetUserInfo();
            if (userInfo == null)
            {
                Paths.WritingLog("加载用户信息失败：UserInfo 为空");
                ShowTip(
                    "加载用户信息失败",
                    "请检查网络连接或稍后重试。",
                    ControlAppearance.Danger,
                    SymbolRegular.TagError24);
                return;
            }

            Name = userInfo.username;
            Email = userInfo.email;
            Group = $"用户组：{userInfo.usergroup}";
            Integral = $"积分：{userInfo.integral}";
            Regtime = $"注册时间：{userInfo.regtime}";
            TunnelCount = $"隧道使用：{userInfo.tunnelCount}/{userInfo.tunnel}";
            Bandwidth = $"带宽限制：国内{userInfo.bandwidth}m | 国外{userInfo.bandwidth * 4}m";

            Paths.WritingLog("加载用户信息成功");

            await LoadUserAvatar(userInfo.userimg);
        }
        catch (Exception ex)
        {
            Paths.WritingLog($"加载用户信息异常：{ex.Message}\n{ex.StackTrace}");
            ShowTip(
                "加载失败",
                ex.Message,
                ControlAppearance.Danger,
                SymbolRegular.TagError24);
        }
    }

    private async Task LoadUserAvatar(string avatarUrl)
    {
        if (string.IsNullOrWhiteSpace(avatarUrl) || !Uri.IsWellFormedUriString(avatarUrl, UriKind.Absolute))
        {
            Paths.WritingLog("头像URL无效，使用默认头像");
            CurrentImage = GetDefaultAvatar();
            return;
        }

        var tempFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.png");

        try
        {
            var downloadSuccess = await Http.GetFile(avatarUrl, tempFilePath);
            if (!downloadSuccess || !File.Exists(tempFilePath))
            {
                Paths.WritingLog("头像下载失败，使用默认头像");
                CurrentImage = GetDefaultAvatar();
                return;
            }

            var bitmap = new BitmapImage();
            using (var stream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                bitmap.BeginInit();
                bitmap.StreamSource = stream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
            }

            CurrentImage = bitmap;
            Paths.WritingLog("头像加载成功");
        }
        catch (Exception ex)
        {
            Paths.WritingLog($"头像加载异常：{ex.Message}，使用默认头像");
            CurrentImage = GetDefaultAvatar();
        }
        finally
        {
            if (File.Exists(tempFilePath))
            {
                try
                {
                    File.Delete(tempFilePath);
                }
                catch (Exception ex)
                {
                    Paths.WritingLog($"临时头像文件删除失败：{ex.Message}");
                }
            }
        }
    }

    private BitmapImage GetDefaultAvatar()
    {
        try
        {
            var defaultAvatar = new BitmapImage();
            defaultAvatar.BeginInit();
            defaultAvatar.UriSource = new Uri("pack://application:,,,/Assets/default-avatar.png");
            defaultAvatar.CacheOption = BitmapCacheOption.OnLoad;
            defaultAvatar.EndInit();
            defaultAvatar.Freeze();
            return defaultAvatar;
        }
        catch (Exception ex)
        {
            Paths.WritingLog($"默认头像加载失败：{ex.Message}");
            return new BitmapImage();
        }
    }

    [RelayCommand]
    private static async Task OnSignOut()
    {
        try
        {
            Sign.Signout();
            Paths.WritingLog("用户已退出登录");
            ShowTip(
                "已退出登录",
                "请重新登录以继续使用。",
                ControlAppearance.Info,
                SymbolRegular.SignOut24);
            
            await Task.Delay(1000);
            
            var currentExePath = Process.GetCurrentProcess().MainModule?.FileName;
            if (!string.IsNullOrEmpty(currentExePath) && File.Exists(currentExePath))
            {
                Paths.WritingLog("正在重启应用程序");
                Process.Start(currentExePath);
                Application.Current.Shutdown();
            }
            else
            {
                Paths.WritingLog("获取应用程序路径失败，无法重启");
                ShowTip("重启失败", "无法获取应用程序路径", ControlAppearance.Danger, SymbolRegular.ErrorCircle24);
            }
        }
        catch (Exception ex)
        {
            Paths.WritingLog($"退出登录异常：{ex.Message}");
            ShowTip("操作失败", ex.Message, ControlAppearance.Danger, SymbolRegular.ErrorCircle24);
        }
    }

    private void ShowTip(string title, string message, ControlAppearance appearance, SymbolRegular symbol)
    {
    }
}
