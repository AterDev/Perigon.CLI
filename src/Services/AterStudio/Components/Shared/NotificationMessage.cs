namespace AterStudio.Components.Shared;

public class NotificationMessage
{
    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public DateTimeOffset DateTime { get; set; }

    public bool UnRead { get; set; } = true;
}
