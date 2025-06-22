using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using ChmlFrp.SDK.API;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wpf.Ui.Controls;

namespace CAT2.ViewModels;

public partial class TunnelPageViewModel : ObservableObject
{
    // 隧道列表
    [ObservableProperty] private ObservableCollection<TunnelItem> _listDataContext;
    [ObservableProperty] private ObservableCollection<TunnelItem> _viplist;
    [ObservableProperty] private ObservableCollection<TunnelItem> _offlinelist;

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
        Viplist = [];

        var tunnelsData = await Tunnel.GetTunnelsData();
        if (tunnelsData == null)
        {
            Model.ShowTip(
                "加载隧道信息失败",
                "请检查网络连接或稍后重试。",
                ControlAppearance.Danger,
                SymbolRegular.TagError24);
        }
        else if (tunnelsData.Count == 0)
        {
            Model.ShowTip(
                "没有隧道信息",
                "当前没有可用的隧道信息，请注册隧道。",
                ControlAppearance.Danger,
                SymbolRegular.Warning24);
            IsSelected = true;
        }
        else
        {
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
                if (tunnelData.ip.Contains("vip")) Viplist.Add(person);
                if (tunnelData.nodestate != "online") Offlinelist.Add(person);
            }
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

    // 创建隧道
    [ObservableProperty] private ObservableCollection<NodeItem> _nodeDataContext;
    [ObservableProperty] private NodeItem _nodeName;
    [ObservableProperty] private string _tunnelType;
    [ObservableProperty] private string _localPort;
    [ObservableProperty] private string _remotePort;
    [ObservableProperty] private bool _isSelected;
    [ObservableProperty] private Visibility _isNumberBoxVisibility;
    [ObservableProperty] private Visibility _isTextBoxVisibility;

    partial void OnTunnelTypeChanged(string value)
    {
        if (value is "http" or "https")
        {
            IsNumberBoxVisibility = Visibility.Collapsed;
            IsTextBoxVisibility = Visibility.Visible;
        }
        else
        {
            IsNumberBoxVisibility = Visibility.Visible;
            IsTextBoxVisibility = Visibility.Collapsed;
        }
    }

    [RelayCommand]
    private async Task CreateTunnel()
    {
        if (NodeName == null || string.IsNullOrEmpty(LocalPort) || string.IsNullOrEmpty(RemotePort))
        {
            Model.ShowTip("输入错误",
                "请确保所有字段都已填写。",
                ControlAppearance.Danger,
                SymbolRegular.Warning24);
            return;
        }

        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        var result = new char[8];
        for (var i = 0; i < 8; i++)
            result[i] = chars[random.Next(chars.Length)];
        var tunnelName = new string(result);

        var msg = await Tunnel.CreateTunnel(tunnelName, NodeName.Name, TunnelType, LocalPort, RemotePort);

        switch (msg)
        {
            case null:
                Model.ShowTip("隧道创建失败",
                    "请检查网络连接或稍后重试。",
                    ControlAppearance.Danger,
                    SymbolRegular.TagError24);
                return;
            case "创建成功":
                Model.ShowTip("隧道创建成功",
                    $"隧道 {tunnelName} 已成功创建，请稍后查看。",
                    ControlAppearance.Success,
                    SymbolRegular.Checkmark24);
                Loading(null, null);
                return;
            default:
                Model.ShowTip("隧道创建失败",
                    msg,
                    ControlAppearance.Danger,
                    SymbolRegular.TagError24);
                break;
        }
    }
}

public partial class TunnelItem(TunnelPageViewModel parentViewModel) : ObservableObject
{
    [ObservableProperty] private string _name;
    [ObservableProperty] private string _id;
    [ObservableProperty] private string _info;
    [ObservableProperty] private bool _isTunnelStarted;
    [ObservableProperty] private bool _isFlyoutOpen;
    [ObservableProperty] private bool _isEnabled = true;
    public string Url;

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
                    Model.ShowTip(
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
                    Model.ShowTip(
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
                    Model.ShowTip(
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
                    Model.ShowTip(
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
                    Model.ShowTip("隧道启动成功",
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
                    Model.ShowTip("隧道关闭成功",
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
                    Model.ShowTip("隧道关闭失败",
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

        Model.ShowTip("隧道删除成功",
            $"隧道 {Name} 已成功删除。",
            ControlAppearance.Success,
            SymbolRegular.Checkmark24);

        parentViewModel.ListDataContext.Remove(this);
        parentViewModel.Offlinelist.Remove(this);
        parentViewModel.Viplist.Remove(this);
    }

    [RelayCommand]
    private void OpenFlyout()
    {
        IsFlyoutOpen = true;
    }
}

public partial class NodeItem : ObservableObject
{
    [ObservableProperty] private string _name;
    [ObservableProperty] private string _content;
    [ObservableProperty] private string _notes;
}