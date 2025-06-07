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
        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(50) };
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
                    var tunnelInfo = await Tunnel.GetTunnelData(tunnelName);

                    ListView.Items.Add(new TunnelViewItem
                    {
                        Content = tunnelName,
                        Id = $"#{tunnelInfo.id}",
                        IsTunnelStarted = await Stop.IsTunnelRunning(tunnelInfo.name),
                        Info = $"{tunnelInfo.node}-{tunnelInfo.nport}-{tunnelInfo.type}"
                    });
                }

                ListView.Visibility = Visibility.Visible;
            }
            else
            {
                ListView.Visibility = Visibility.Collapsed;
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

        Stop.StopTunnel(tag, StopTrueHandler, StopFalseHandler);
        return;

        void StopTrueHandler()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Constant.ShowTip("隧道关闭成功", $"隧道 {tag} 已成功关闭。", ControlAppearance.Success, SymbolRegular.Checkmark24);
                toggleButton.IsEnabled = true;
            });
        }

        void StopFalseHandler()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Constant.ShowTip("隧道关闭失败", $"隧道 {tag} 已退出。", ControlAppearance.Danger, SymbolRegular.TagError24);
                toggleButton.IsEnabled = true;
            });
        }
    }

    private void StartTunnel(object sender, RoutedEventArgs routedEventArgs)
    {
        if (sender is not ToggleButton toggleButton) return;
        var tag = (string)toggleButton.Tag;
        if (tag is null) return;
        toggleButton.IsEnabled = false;

        Start.StartTunnel(tag, StartTrueHandler, StartFalseHandler, IniUnKnown);
        return;

        void IniUnKnown()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Constant.ShowTip(
                    "隧道启动数据获取失败",
                    "请检查网络状态，或查看API状态。",
                    ControlAppearance.Danger,
                    SymbolRegular.TagError24);

                toggleButton.IsChecked = false;
            });
        }

        void StartFalseHandler()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Constant.ShowTip(
                    "隧道启动失败",
                    $"隧道 {tag} 启动失败，具体请看日志。",
                    ControlAppearance.Danger,
                    SymbolRegular.TagError24);

                toggleButton.IsChecked = false;
            });
        }

        void StartTrueHandler()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Constant.ShowTip("隧道启动成功",
                    $"隧道 {tag} 已成功启动。",
                    ControlAppearance.Success,
                    SymbolRegular.Checkmark24);
                toggleButton.IsEnabled = true;
            });
        }
    }
}