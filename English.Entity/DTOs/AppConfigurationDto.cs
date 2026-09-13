namespace English.Entity.DTOs;

public class AppConfigurationDto
{
    public string ConfigKey { get; set; } = string.Empty;
    public string ConfigValue { get; set; } = string.Empty;
    public string? ValueType { get; set; }
    public string? Description { get; set; }
}
