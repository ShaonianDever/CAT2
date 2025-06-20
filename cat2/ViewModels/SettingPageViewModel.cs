using CommunityToolkit.Mvvm.ComponentModel;

namespace CAT2.ViewModels;

public partial class SettingPageViewModel : ObservableObject
{
    [ObservableProperty] private string _version = Model.Version;
    [ObservableProperty] private string _assemblyName = Model.AssemblyName;
}