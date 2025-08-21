namespace AterStudio.Components.Shared.Models;

/// <summary>
///add or update file information.
/// </summary>
public class UpsertFileDto
{
    public required string RootPath { get; set; }
    public string? FileName { get; set; }
    public required string DirectoryName { get; set; }
    public required string Suffix { get; set; }

    public string FullPath
    {
        get
        {
            return string.IsNullOrEmpty(FileName)
                ? Path.Combine(RootPath, DirectoryName)
                : Path.Combine(RootPath, DirectoryName, FileName + Suffix);
        }
    }
}
