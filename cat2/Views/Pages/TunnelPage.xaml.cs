using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using ChmlFrp.SDK.Frpc;

namespace CAT2.Views.Pages;

public class TunnelViewItem : ListViewItem
{
    public static readonly DependencyProperty IdProperty =
        DependencyProperty.Register(nameof(Id), typeof(string), typeof(TunnelViewItem));

    public static readonly DependencyProperty InfoProperty =
        DependencyProperty.Register(nameof(Info), typeof(string), typeof(TunnelViewItem));

    public static readonly DependencyProperty IsTunnelStartedProperty =
        DependencyProperty.Register(nameof(IsTunnelStarted), typeof(bool), typeof(TunnelViewItem));

    public string Id
    {
        get => (string)GetValue(IdProperty);
        set => SetValue(IdProperty, value);
    }

    public string Info
    {
        get => (string)GetValue(InfoProperty);
        set => SetValue(InfoProperty, value);
    }

    public bool IsTunnelStarted
    {
        get => (bool)GetValue(IsTunnelStartedProperty);
        set => SetValue(IsTunnelStartedProperty, value);
    }
}

public partial class TunnelPage
{
    public TunnelPage()
    {
        InitializeComponent();
        LoadTunnelInfo(null, null);
        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
        timer.Tick += LoadTunnelInfo;
        timer.Start();
    }

    private async void LoadTunnelInfo(object sender, EventArgs e)
    {
        LoadingRing.Visibility = Visibility.Visible;
        TunnelCard.Visibility = Visibility.Collapsed;
        var tunnelNames = await Tunnel.GetTunnelNames();
        if (tunnelNames != null)
        {
            if (tunnelNames.Count != 0)
            {
                ListView.Items.Clear();
                foreach (var tunnelName in tunnelNames)
                {
                    var tunnelinfo = await Tunnel.GetTunnelData(tunnelName);

                    ListView.Items.Add(new TunnelViewItem
                    {
                        Content = tunnelName,
                        Id = $"#{tunnelinfo.id}",
                        IsTunnelStarted = await Stop.IsTunnelRunning(tunnelinfo.name),
                        Info = $"{tunnelinfo.node}-{tunnelinfo.nport}-{tunnelinfo.type}"
                    });
                }

                ListView.Visibility = Visibility.Visible;
                NoTunnelText.Visibility = Visibility.Collapsed;
            }
            else
            {
                ListView.Visibility = Visibility.Collapsed;
                NoTunnelText.Visibility = Visibility.Visible;
            }
        }
        else
        {
            Constant.ShowTip(
                "加载隧道信息失败",
                "请检查网络连接或稍后重试。",
                ControlAppearance.Danger,
                SymbolRegular.TagError24);
        }

        LoadingRing.Visibility = Visibility.Collapsed;
        TunnelCard.Visibility = Visibility.Visible;
    }

    private void StopTunnel(object sender, RoutedEventArgs routedEventArgs)
    {
        if (sender is not ToggleButton toggleButton) return;
        var tag = (string)toggleButton.Tag;
        if (tag is null) return;
        toggleButton.IsEnabled = false;

        Action stopTrueHandler = null;
        Action stopFalseHandler = null;

        stopTrueHandler = () =>
        {
            Stop.OnStopTrue -= stopTrueHandler;
            Application.Current.Dispatcher.Invoke(() =>
            {
                Constant.ShowTip("隧道关闭成功", $"隧道 {tag} 已成功关闭。", ControlAppearance.Success, SymbolRegular.Checkmark24);
                toggleButton.IsEnabled = true;
            });
        };

        stopFalseHandler = () =>
        {
            Stop.OnStopFalse -= stopFalseHandler;
            Application.Current.Dispatcher.Invoke(() =>
            {
                Constant.ShowTip("隧道关闭失败", $"隧道 {tag} 已退出。", ControlAppearance.Danger, SymbolRegular.TagError24);
                toggleButton.IsEnabled = true;
            });
        };

        Stop.OnStopTrue += stopTrueHandler;
        Stop.OnStopFalse += stopFalseHandler;
        Stop.StopTunnel(tag);
    }

    private void StartTunnel(object sender, RoutedEventArgs routedEventArgs)
    {
        if (sender is not ToggleButton toggleButton) return;
        var tag = (string)toggleButton.Tag;
        if (tag is null) return;
        toggleButton.IsEnabled = false;

        Action startTrueHandler = null;
        Action startFalseHandler = null;
        Action iniUnKnown = null;

        startTrueHandler = () =>
        {
            Start.OnStartTrue -= startTrueHandler;
            Application.Current.Dispatcher.Invoke(() =>
            {
                Constant.ShowTip("隧道启动成功", $"隧道 {tag} 已成功启动。", ControlAppearance.Success, SymbolRegular.Checkmark24);
                toggleButton.IsEnabled = true;
            });
        };

        startFalseHandler = () =>
        {
            Start.OnStartFalse -= startFalseHandler;
            Application.Current.Dispatcher.Invoke(() =>
                Constant.ShowTip("隧道启动失败",
                    $"隧道 {tag} 启动失败，具体请看日志。",
                    ControlAppearance.Danger,
                    SymbolRegular.TagError24));

            Task.Run(async () =>
            {
                await Task.Delay(1000);
                Process.Start(new ProcessStartInfo
                {
                    FileName = Start.FrpclogFilePath,
                    UseShellExecute = true
                });
                Application.Current.Dispatcher.Invoke(() =>
                    toggleButton.IsChecked = false);
            });
        };

        iniUnKnown += () =>
        {
            Start.OnIniUnKnown -= iniUnKnown;
            Application.Current.Dispatcher.Invoke(() =>
                Constant.ShowTip("隧道启动数据获取失败",
                    "请检查网络状态，或查看API状态。",
                    ControlAppearance.Danger,
                    SymbolRegular.TagError24));
        };

        Start.OnStartTrue += startTrueHandler;
        Start.OnStartFalse += startFalseHandler;
        Start.OnIniUnKnown += iniUnKnown;
        Start.StartTunnel(tag);
    }
}