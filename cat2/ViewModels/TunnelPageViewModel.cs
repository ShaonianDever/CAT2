using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Wpf.Ui.Controls;
using static CAT2.Model;

namespace CAT2.ViewModels;

public partial class TunnelPageViewModel : ObservableObject
{
    [ObservableProperty] private bool _isCreateTunnelFlyoutOpen;

    // 隧道列表
    [ObservableProperty] private ObservableCollection<TunnelItem> _listDataContext;
    [ObservableProperty] private string _localPort;

    // 创建隧道
    [ObservableProperty] private ObservableCollection<NodeItem> _nodeDataContext;
    [ObservableProperty] private NodeItem _nodeName;
    [ObservableProperty] private ObservableCollection<TunnelItem> _offlinelist;
    [ObservableProperty] private string _remotePort;
    [ObservableProperty] private string _tunnelType;

    public TunnelPageViewModel()
    {
        Loading(null, null);
        var timer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(1) };
        timer.Tick += Loading;
        timer.Start();
    }

    private async void Loading(object sender, EventArgs e)
    {
        ListDataContext = [];
        Offlinelist = [];

        var tunnelsData = await Tunnel.GetTunnelsData();
        if (tunnelsData == null)
            ShowTip(
                "加载隧道信息失败",
                "请检查网络连接或稍后重试。",
                ControlAppearance.Danger,
                SymbolRegular.TagError24);
        else if (tunnelsData.Count == 0)
            ShowTip(
                "没有隧道信息",
                "当前没有可用的隧道信息，请注册隧道。",
                ControlAppearance.Danger,
                SymbolRegular.Warning24);
        else
            foreach (var tunnelData in tunnelsData)
            {
                var person = new TunnelItem(this)
                {
                    Name = tunnelData.name,
                    Id = $"#{tunnelData.id}",
                    IsTunnelStarted = await ChmlFrp.SDK.Services.Tunnel.IsTunnelRunning(tunnelData.name),
                    Info = $"{tunnelData.node}-{tunnelData.nport}-{tunnelData.type}",
                    Url = $"{tunnelData.ip}:{tunnelData.dorp}"
                };
                ListDataContext.Add(person);
                if (tunnelData.nodestate != "online") Offlinelist.Add(person);
            }

        // 节点数据
        NodeDataContext = [];
        foreach (var nodeData in await Node.GetNodeData())
        {
            nodeData.udp = nodeData.udp == "true" ? "允许UDP" : "不允许UDP";
            nodeData.web = nodeData.web == "yes" ? "允许建站" : "不允许建站";
            nodeData.nodegroup = nodeData.nodegroup == "vip" ? "VIP节点" : "免费节点";

            NodeDataContext.Add(new NodeItem
            {
                Name = nodeData.name,
                Content = $"{nodeData.name} ({nodeData.nodegroup})",
                Notes = $"{nodeData.notes} {nodeData.udp} {nodeData.web}"
            });
        }
    }

    [RelayCommand]
    private async Task CreateTunnel()
    {
        if (NodeName == null || string.IsNullOrEmpty(NodeName.Name) || string.IsNullOrEmpty(LocalPort) ||
            string.IsNullOrEmpty(RemotePort))
        {
            ShowTip("输入错误",
                "请确保所有字段都已填写。",
                ControlAppearance.Danger,
                SymbolRegular.Warning24);
            return;
        }

        var msg = await Tunnel.CreateTunnel(NodeName.Name, TunnelType, LocalPort, RemotePort);

        if (msg == null)
        {
            ShowTip("隧道创建失败",
                "请检查网络连接或稍后重试。",
                ControlAppearance.Danger,
                SymbolRegular.TagError24);
            return;
        }

        if (msg.Contains("成功"))
        {
            ShowTip("隧道创建成功",
                $"{msg}。",
                ControlAppearance.Success,
                SymbolRegular.Checkmark24);
            RemotePort = string.Empty;
            LocalPort = string.Empty;
            Loading(null, null);
            return;
        }

        ShowTip("隧道创建失败",
            msg,
            ControlAppearance.Danger,
            SymbolRegular.TagError24);
    }

    [RelayCommand]
    private void ShowFlyout()
    {
        IsCreateTunnelFlyoutOpen = true;
    }
}

public partial class TunnelItem(TunnelPageViewModel parentViewModel) : ObservableObject
{
    [ObservableProperty] private string _id;
    [ObservableProperty] private string _info;
    [ObservableProperty] private bool _isEnabled = true;
    [ObservableProperty] private bool _isFlyoutOpen;
    [ObservableProperty] private bool _isTunnelStarted;
    [ObservableProperty] private string _name;
    [ObservableProperty] private string _url;

    [RelayCommand]
    private void Tunnel()
    {
        IsEnabled = false;
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
                    IsEnabled = true;
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
                    IsTunnelStarted = false;
                    IsEnabled = true;
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
                    IsTunnelStarted = false;
                    IsEnabled = true;
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
                    IsTunnelStarted = false;
                    IsEnabled = true;
                });
            }

            void StartTrueHandler()
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ShowTip("隧道启动成功",
                        $"隧道 {Name} 已成功启动，链接已复制到剪切板。",
                        ControlAppearance.Success,
                        SymbolRegular.Checkmark24);
                    IsEnabled = true;
                    Clipboard.SetDataObject(Url);
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
                    IsEnabled = true;
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
                    IsEnabled = true;
                });
            }
        }
    }


    [RelayCommand]
    private void DeleteTunnel()
    {
        ChmlFrp.SDK.API.Tunnel.DeleteTunnel(Name);

        ShowTip("隧道删除成功",
            $"隧道 {Name} 已成功删除。",
            ControlAppearance.Success,
            SymbolRegular.Checkmark24);

        parentViewModel.ListDataContext.Remove(this);
        parentViewModel.Offlinelist.Remove(this);
    }

    [RelayCommand]
    private void OpenFlyout()
    {
        IsFlyoutOpen = true;
    }


    [RelayCommand]
    private void CopyTunnel()
    {
        try
        {
            Clipboard.SetDataObject(Url, true);
        }
        catch
        {
            return;
        }

        ShowTip("链接已复制",
            $"隧道 {Name} 的链接已复制到剪切板。",
            ControlAppearance.Success,
            SymbolRegular.Checkmark24);
    }
}

public partial class NodeItem : ObservableObject
{
    [ObservableProperty] private string _content;
    [ObservableProperty] private string _name;
    [ObservableProperty] private string _notes;
}