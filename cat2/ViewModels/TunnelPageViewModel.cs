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
    [ObservableProperty] private ObservableCollection<Person> _listDataContext;
    [ObservableProperty] private ObservableCollection<Person> _viplist;
    [ObservableProperty] private ObservableCollection<Person> _offlinelist;
    [ObservableProperty] private Visibility _ringVisibility;
    [ObservableProperty] private Visibility _cardVisibility;

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
            Model.ShowTip(
                "加载隧道信息失败",
                "请检查网络连接或稍后重试。",
                ControlAppearance.Danger,
                SymbolRegular.TagError24);
        }
        else
        {
            ListDataContext = [];
            Viplist = [];

            foreach (var tunnelName in tunnelNames)
            {
                var tunnelInfo = await Tunnel.GetTunnelData(tunnelName);

                var person = new Person
                {
                    Name = tunnelName,
                    Id = $"#{tunnelInfo.id}",
                    IsTunnelStarted = await ChmlFrp.SDK.Services.Tunnel.IsTunnelRunning(tunnelInfo.name),
                    Info = $"{tunnelInfo.node}-{tunnelInfo.nport}-{tunnelInfo.type}"
                };
                ListDataContext.Add(person);
                if (tunnelInfo.ip.Contains("vip")) Viplist.Add(person);
                if (tunnelInfo.nodestate != "online") Offlinelist.Add(person);
            }
        }

        RingVisibility = Visibility.Collapsed;
        CardVisibility = Visibility.Visible;
    }

    public partial class Person : ObservableObject
    {
        [ObservableProperty] private string _name;
        [ObservableProperty] private string _id;
        [ObservableProperty] private string _info;
        [ObservableProperty] private bool _isTunnelStarted;
        [ObservableProperty] private bool _isEnabled = true;

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
                            $"隧道 {Name} 已成功启动。",
                            ControlAppearance.Success,
                            SymbolRegular.Checkmark24);
                        IsEnabled = true;
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
    }
}