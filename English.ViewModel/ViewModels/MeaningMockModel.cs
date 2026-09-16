namespace English.ViewModel.ViewModels;

/// <summary>
/// Model mock dữ liệu nghĩa của từ phục vụ kiểm thử giao diện và binding UI.
/// </summary>
public class MeaningMockModel
{
    public string WordClass { get; set; } = string.Empty;
    public string DefinitionVI { get; set; } = string.Empty;
    public List<string> Examples { get; set; } = new();
}
