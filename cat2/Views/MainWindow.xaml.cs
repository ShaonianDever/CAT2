using System.Threading.Tasks;
using CAT2.ViewModels;
using Microsoft.Win32;

namespace CAT2.Views;

public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
        if (DataContext is MainWindowViewModel viewModel) viewModel.LoadedCommand.Execute(null);
        SystemEvents.UserPreferenceChanged += OnSystemThemeChanged;
    }

    private static async void OnSystemThemeChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        ApplicationThemeManager.Apply(ApplicationThemeManager.GetAppTheme() is ApplicationTheme.Dark
            ? ApplicationTheme.Light
            : ApplicationTheme.Dark);
        SystemEvents.UserPreferenceChanged -= OnSystemThemeChanged;
        await Task.Delay(100);
        SystemEvents.UserPreferenceChanged += OnSystemThemeChanged;
    }
}