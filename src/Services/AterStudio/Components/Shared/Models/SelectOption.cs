namespace AterStudio.Components.Shared.Models;

public class SelectOption
{
    public required string DisplayName { get; set; }
    public string? Value { get; set; }
    public List<SelectOption> Children { get; set; } = [];
}

public class SelectDialogData
{
    public required string Titlee { get; set; }
    public List<SelectOption> Options { get; set; } = [];
}
