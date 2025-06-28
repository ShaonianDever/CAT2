namespace CAT2.ViewModels;

public partial class SettingPageViewModel : ObservableObject
{
    [ObservableProperty] private string _assemblyName = Model.AssemblyName;
    [ObservableProperty] private string _copyright = Model.Copyright;
    [ObservableProperty] private string _fileVersion = $"文件版本：{Model.FileVersion}";
    [ObservableProperty] private string _version = Model.Version;
}