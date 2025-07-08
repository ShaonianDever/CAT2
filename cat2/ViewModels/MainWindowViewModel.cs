using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32;
using Wpf.Ui.Appearance;

namespace CAT2.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty] private string _assemblyName = Model.AssemblyName;
    [ObservableProperty] private bool _isDarkTheme;

    public MainWindowViewModel()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            if (args.ExceptionObject is not Exception ex) return;

            WritingLog(ex.Message.Contains("拒绝访问")
                ? "请以管理员身份运行本程序"
                : $"请将此日志反馈给开发者\n联系方式：\n1.QQ：2976779544\n2.Email：Qusay_Diaz@outlook.com\n3.GitHub：Qianyiaz/CAT2\n版本号：{Model.Version}次版本号：{FileVersion}\n异常信息：\n{ex}");

            Process.Start(new ProcessStartInfo
            {
                FileName = LogFilePath,
                UseShellExecute = true
            });
        };

        MainClass.Loaded += async (_, _) =>
        {
            new Mutex(
                true,
                "CAT2_GlobalMutex",
                out var createdNew);

            if (!createdNew)
            {
                var otherInstance = Process.GetProcessesByName(Model.AssemblyName).FirstOrDefault();
                if (otherInstance != null)
                {
                    otherInstance.WaitForInputIdle();
                    SetForegroundWindow(otherInstance.MainWindowHandle);
                }

                Application.Current.Shutdown();
                return;
            }

            Init("CAT2");

            if (ApplicationThemeManager.GetSystemTheme() == SystemTheme.Dark) ThemesChanged();

            MainClass.RootNavigation.Navigate("登录");
            WritingLog($"验证上次登录：{await Sign.Signin()}");
            if (Sign.IsSignin)
            {
                MainClass.LoginItem.Visibility = Visibility.Collapsed;
                MainClass.UserItem.Visibility = Visibility.Visible;
                MainClass.TunnelItem.Visibility = Visibility.Visible;
                MainClass.RootNavigation.Navigate("用户页");
            }

            MainClass.Topmost = false;
            UpdateApp();
            WritingLog("主窗口加载完成");
        };

        SystemEvents.UserPreferenceChanged += (_, _) =>
        {
            var theme = ApplicationThemeManager.GetSystemTheme() == SystemTheme.Light;
            ApplicationThemeManager.Apply(theme ? ApplicationTheme.Light : ApplicationTheme.Dark);
            IsDarkTheme = !theme;
        };
    }

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [RelayCommand]
    private void ThemesChanged()
    {
        var theme = ApplicationThemeManager.GetAppTheme() == ApplicationTheme.Light;
        ApplicationThemeManager.Apply(theme ? ApplicationTheme.Dark : ApplicationTheme.Light);
        IsDarkTheme = theme;
    }

    [RelayCommand]
    private static void MinimizeThis()
    {
        MainClass.WindowState = WindowState.Minimized;
    }

    [RelayCommand]
    private static void CloseThis()
    {
        WritingLog("主窗口正常退出");
        MainClass.Close();
    }
}