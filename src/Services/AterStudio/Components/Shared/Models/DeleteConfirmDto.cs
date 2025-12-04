namespace AterStudio.Components.Shared.Models;

/// <summary>
/// Delete confirmation dialog data.
/// </summary>
public class DeleteConfirmDto
{
    public required string Title { get; set; }
    public required string Message { get; set; }
    public Func<Task>? OnConfirm { get; set; }
}