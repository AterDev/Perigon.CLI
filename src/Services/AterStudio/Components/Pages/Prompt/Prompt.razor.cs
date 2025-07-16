using CodeGenerator.Helper;
using Microsoft.AspNetCore.Components;

namespace AterStudio.Components.Pages.Prompt;

public partial class Prompt
{
    [Inject]
    private PromptManager PromptManager { get; set; } = default!;

    private List<DataFile> Directories { get; set; } = new();
    private List<DataFile> Files { get; set; } = new();
    private string? SelectedDirectory { get; set; }
    private DataFile? SelectedFile { get; set; }

    private FluentDialog _dialog = default!;
    private bool DialogHidden { get; set; } = true;
    private string DialogTitle { get; set; } = string.Empty;
    private string EditContent { get; set; } = string.Empty;
    private EditContext? editContext;

    protected override void OnInitialized()
    {
        LoadDirectories();
    }

    private void LoadDirectories()
    {
        Directories = PromptManager.FileHelper.GetDirectories();
        if (Directories.Count > 0)
        {
            SelectedDirectory = Directories[0].Name;
            LoadFiles();
        }
        else
        {
            SelectedDirectory = null;
            Files.Clear();
        }
    }

    private void SelectDirectory(string dirName)
    {
        SelectedDirectory = dirName;
        LoadFiles();
    }

    private void LoadFiles()
    {
        if (!string.IsNullOrEmpty(SelectedDirectory))
        {
            Files = PromptManager.GetPromptFiles(SelectedDirectory);
        }
        else
        {
            Files.Clear();
        }
        SelectedFile = null;
    }

    private void OpenAddDirectoryDialog()
    {
        // TODO: 弹窗新建目录
    }

    private async Task DeleteDirectoryAsync()
    {
        if (!string.IsNullOrEmpty(SelectedDirectory))
        {
            PromptManager.FileHelper.DeleteDirectory(
                System.IO.Path.Combine(PromptManager.FileHelper.RootPath, SelectedDirectory)
            );
            LoadDirectories();
        }
    }

    private void OpenAddFileDialog()
    {
        // TODO: 弹窗新建文件
    }

    private async Task DeleteFileAsync()
    {
        if (SelectedFile != null)
        {
            PromptManager.FileHelper.DeleteFile(SelectedFile.FullPath);
            LoadFiles();
        }
    }

    private async Task EditFileAsync(DataFile file)
    {
        DialogTitle = $"编辑: {file.Name}";
        EditContent = PromptManager.FileHelper.GetFileContent(file.FullPath);
        editContext = new EditContext(this);
        SelectedFile = file;
        DialogHidden = false;
    }

    private async Task SaveFileAsync()
    {
        if (SelectedFile != null)
        {
            PromptManager.FileHelper.UpdateFileContent(SelectedFile.FullPath, EditContent);
            DialogHidden = true;
            LoadFiles();
        }
    }
}
