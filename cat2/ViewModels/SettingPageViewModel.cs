namespace CAT2.ViewModels;

public partial class SettingPageViewModel : ObservableObject
{
    [ObservableProperty] private string _assemblyName = Model.AssemblyName;
    [ObservableProperty] private string _version = Model.Version;
}