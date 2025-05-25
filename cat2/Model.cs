using System.Windows;
using Wpf.Ui.Controls;

namespace CAT2;

public abstract class Constant
{
    public static readonly MainWindow MainWindow = (MainWindow)Application.Current.MainWindow;

    public static async void ShowTip(string title, string content, ControlAppearance appearance, SymbolRegular icon)
    {
        MainWindow.GlobalSnackbar.Title = title;
        MainWindow.GlobalSnackbar.Content = content;
        MainWindow.GlobalSnackbar.Appearance = appearance;
        MainWindow.GlobalSnackbar.Icon = new SymbolIcon(icon)
        {
            FontSize = 32
        };
        await MainWindow.GlobalSnackbar.ShowAsync();
    }
}