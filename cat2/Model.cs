using System.Windows;
using Wpf.Ui.Controls;

namespace CAT2;

public abstract class Constant
{
    public static readonly MainWindow MainWinClass = (MainWindow)Application.Current.MainWindow;

    public static async void ShowTip(string title, string content, ControlAppearance appearance, SymbolRegular icon)
    {
        MainWinClass.GlobalSnackbar.Title = title;
        MainWinClass.GlobalSnackbar.Content = content;
        MainWinClass.GlobalSnackbar.Appearance = appearance;
        MainWinClass.GlobalSnackbar.Icon = new SymbolIcon(icon)
        {
            FontSize = 32
        };
        await MainWinClass.GlobalSnackbar.ShowAsync();
    }
}