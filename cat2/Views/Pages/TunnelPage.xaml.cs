using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using Wpf.Ui.Controls;

namespace CAT2.Views.Pages;

public class TunnelViewItem : ListViewItem
{
    public static readonly DependencyProperty IdProperty =
        DependencyProperty.Register(nameof(Id), typeof(string), typeof(TunnelViewItem));

    public string Id
    {
        get => (string)GetValue(IdProperty);
        set => SetValue(IdProperty, value);
    }

    public static readonly DependencyProperty InfoProperty =
        DependencyProperty.Register(nameof(Info), typeof(string), typeof(TunnelViewItem));

    public string Info
    {
        get => (string)GetValue(InfoProperty);
        set => SetValue(InfoProperty, value);
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
        var tunnelNames = await ChmlFrp.SDK.API.Tunnel.GetTunnelNames();
        if (tunnelNames != null)
        {
            if (tunnelNames.Count != 0)
            {
                ListView.Items.Clear();
                foreach (var tunnelName in tunnelNames)
                {
                    var tunnelinfo = await ChmlFrp.SDK.API.Tunnel.GetTunnelData(tunnelName);

                    ListView.Items.Add(new TunnelViewItem
                    {
                        Content = tunnelName,
                        Id = $"#{tunnelinfo.id}",
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
        Console.WriteLine($"{tag} 关闭成功");
    }

    private void StartTunnel(object sender, RoutedEventArgs routedEventArgs)
    {
        if (sender is not ToggleButton toggleButton) return;
        var tag = (string)toggleButton.Tag;
        Console.WriteLine($"{tag} 启动成功");
    }
}