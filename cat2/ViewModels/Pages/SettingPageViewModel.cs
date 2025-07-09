using System.IO;

namespace CAT2.ViewModels;

public partial class SettingPageViewModel : ObservableObject
{
    [ObservableProperty] private string _assemblyName = Model.AssemblyName;
    [ObservableProperty] private string _copyright = Model.Copyright;
    [ObservableProperty] private string _fileVersion = $"文件版本：{Model.FileVersion}";
    [ObservableProperty] private string _version = Model.Version;

    [RelayCommand]
    private void OpenDataPath()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = DataPath,
            UseShellExecute = true
        });
    }

    [RelayCommand]
    private void ClearCache()
    {
        foreach (var cachefile in Directory.GetFiles(DataPath, "*.log"))
            try
            {
                File.Delete(cachefile);
            }
            catch
            {
                // ignored
            }

        ShowTip(
            "缓存已清理",
            "所有缓存文件已被删除。",
            ControlAppearance.Success,
            SymbolRegular.PresenceAvailable24);
    }
}