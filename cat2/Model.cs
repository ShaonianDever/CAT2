using System.Linq;
using CAT2.Views;

namespace CAT2;

public abstract class Model
{
    public static readonly MainWindow MainClass = (MainWindow)Application.Current.MainWindow;

    public static async void ShowTip(string title, string content, ControlAppearance appearance, SymbolRegular icon)
    {
        var globalSnackbar = new Snackbar(new SnackbarPresenter())
        {
            Margin = new Thickness(45, 0, 20, 40),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Bottom,
            Timeout = TimeSpan.FromMilliseconds(2000),
            IsCloseButtonEnabled = false,
            Title = title,
            Content = content,
            Appearance = appearance,
            Icon = new SymbolIcon(icon)
            {
                FontSize = 32
            }
        };

        foreach (var child in MainClass.MainGrid.Children.OfType<Snackbar>().ToList())
            MainClass.MainGrid.Children.Remove(child);

        MainClass.MainGrid.Children.Add(globalSnackbar);
        await globalSnackbar.ShowAsync();
    }
}