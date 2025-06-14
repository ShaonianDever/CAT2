using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ChmlFrp.SDK;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Wpf.Ui.Appearance;
using static Wpf.Ui.Appearance.ApplicationThemeManager;

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
    [ObservableProperty] private string _password;
    [ObservableProperty] private string _username;

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

public partial class UserinfoPageViewModel : ObservableObject
{
    private readonly string _tempUserImage = Path.GetTempFileName();
    [ObservableProperty] private string _bandwidth;
    [ObservableProperty] private Visibility _cardVisibility;
    [ObservableProperty] private BitmapImage _currentImage;
    [ObservableProperty] private string _email;
    [ObservableProperty] private string _group;
    [ObservableProperty] private string _integral;
    [ObservableProperty] private string _name;
    [ObservableProperty] private string _regtime;
    [ObservableProperty] private Visibility _ringVisibility;
    [ObservableProperty] private string _tunnelCount;

    public UserinfoPageViewModel()
    {
        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
        timer.Tick += Loading;
        timer.Start();
        Loading(null, null);
    }

    private async void Loading(object sender, EventArgs e)
    {
        RingVisibility = Visibility.Visible;
        CardVisibility = Visibility.Collapsed;

        var userInfo = await User.GetUserInfo();
        if (userInfo == null)
        {
            ShowTip(
                "加载用户信息失败",
                "请检查网络连接或稍后重试。",
                ControlAppearance.Danger,
                SymbolRegular.TagError24);
            RingVisibility = Visibility.Collapsed;
            return;
        }

        if (await Http.GetFile(userInfo.userimg, _tempUserImage))
        {
            CurrentImage = new BitmapImage();
            CurrentImage.BeginInit();
            CurrentImage.CacheOption = BitmapCacheOption.OnLoad;
            CurrentImage.UriSource = new Uri(_tempUserImage);
            CurrentImage.EndInit();
        }

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
    private async Task OnSignOut()
    {
        Sign.Signout();
        ShowTip(
            "已退出登录",
            "请重新登录以继续使用。",
            ControlAppearance.Info,
            SymbolRegular.SignOut24);
        await Task.Delay(1000);
        Process.Start(Assembly.GetExecutingAssembly().Location);
        MainClass.Close();
    }
}

public partial class TunnelPageViewModel : ObservableObject
{
    [ObservableProperty] private Visibility _cardVisibility;
    [ObservableProperty] private ObservableCollection<Person> _listDataContext;
    [ObservableProperty] private Visibility _ringVisibility;

    public TunnelPageViewModel()
    {
        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
        timer.Tick += Timer_Tick;
        timer.Start();
        Timer_Tick(null, null);
    }

    private void Timer_Tick(object sender, EventArgs e)
    {
        _ = Loading();
    }

    [RelayCommand]
    private async Task Loading()
    {
        RingVisibility = Visibility.Visible;
        CardVisibility = Visibility.Collapsed;

        var tunnelNames = await Tunnel.GetTunnelNames();
        if (tunnelNames == null)
        {
            ShowTip(
                "加载隧道信息失败",
                "请检查网络连接或稍后重试。",
                ControlAppearance.Danger,
                SymbolRegular.TagError24);
        }
        else
        {
            ListDataContext = [];

            foreach (var tunnelName in tunnelNames)
            {
                var tunnelInfo = await Tunnel.GetTunnelData(tunnelName);
                ListDataContext.Add(new Person
                {
                    Name = tunnelName,
                    Id = $"#{tunnelInfo.id}",
                    IsTunnelStarted = await ChmlFrp.SDK.Services.Tunnel.IsTunnelRunning(tunnelInfo.name),
                    Info = $"{tunnelInfo.node}-{tunnelInfo.nport}-{tunnelInfo.type}"
                });
            }
        }

        RingVisibility = Visibility.Collapsed;
        CardVisibility = Visibility.Visible;
    }

    public partial class Person
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string Info { get; set; }
        public bool IsTunnelStarted { get; set; }

        [RelayCommand]
        private void Tunnel()
        {
            if (IsTunnelStarted)
            {
                ChmlFrp.SDK.Services.Tunnel.StartTunnel(Name, StartTrueHandler, StartFalseHandler, IniUnKnown,
                    FrpcNotExists, TunnelRunning);

                void TunnelRunning()
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ShowTip(
                            "隧道已在运行",
                            $"隧道 {Name} 已在运行中。",
                            ControlAppearance.Danger,
                            SymbolRegular.Warning24);
                    });
                }

                void FrpcNotExists()
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ShowTip(
                            "FRPC 暂未安装",
                            "请等待一会，或重新启动。（软件会自动安装）",
                            ControlAppearance.Danger,
                            SymbolRegular.TagError24);
                    });
                }

                void IniUnKnown()
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ShowTip(
                            "隧道启动数据获取失败",
                            "请检查网络状态，或查看API状态。",
                            ControlAppearance.Danger,
                            SymbolRegular.TagError24);
                    });
                }

                void StartFalseHandler()
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ShowTip(
                            "隧道启动失败",
                            $"隧道 {Name} 启动失败，具体请看日志。",
                            ControlAppearance.Danger,
                            SymbolRegular.TagError24);
                    });
                }

                void StartTrueHandler()
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ShowTip("隧道启动成功",
                            $"隧道 {Name} 已成功启动。",
                            ControlAppearance.Success,
                            SymbolRegular.Checkmark24);
                    });
                }
            }
            else
            {
                ChmlFrp.SDK.Services.Tunnel.StopTunnel(Name, StopTrueHandler, StopFalseHandler);

                void StopTrueHandler()
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ShowTip("隧道关闭成功",
                            $"隧道 {Name} 已成功关闭。",
                            ControlAppearance.Success,
                            SymbolRegular.Checkmark24);
                    });
                }

                void StopFalseHandler()
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ShowTip("隧道关闭失败",
                            $"隧道 {Name} 已退出。",
                            ControlAppearance.Danger,
                            SymbolRegular.TagError24);
                    });
                }
            }
        }
    }
}

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty] private bool _isDarkTheme;

    public MainWindowViewModel()
    {
        _ = Loaded();

        SystemEvents.UserPreferenceChanged += (_, _) =>
        {
            var theme = GetSystemTheme() == SystemTheme.Light;
            Apply(theme ? ApplicationTheme.Light : ApplicationTheme.Dark);
            IsDarkTheme = !theme;
        };
    }

    [RelayCommand]
    private void ThemesChanged()
    {
        var theme = GetAppTheme() == ApplicationTheme.Light;
        Apply(theme ? ApplicationTheme.Dark : ApplicationTheme.Light);
        IsDarkTheme = theme;
    }

    [RelayCommand]
    private async Task Loaded()
    {
        if (GetSystemTheme() == SystemTheme.Dark) ThemesChanged();
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
        UpdateApp();
    }

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
}