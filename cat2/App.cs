using ChmlFrp.SDK;

namespace CAT2.ViewModels;

public partial class App
{
    public App()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            if (args.ExceptionObject is not Exception ex) return;
            Paths.WritingLog($"Error: \n{ex.Message}");
            Process.Start(new ProcessStartInfo
            {
                FileName = Paths.LogFilePath,
                UseShellExecute = true
            });
        };

        Paths.Init("CAT2");
    }
}