using AterStudio.Components.Shared;
using CodeGenerator.Helper;
using Entity;

namespace AterStudio.Components.Pages.Prompt;

public partial class Prompt
{
    private LocalFileHelper FileHelper { get; set; } = default!;
    private List<DataFile> Directories { get; set; } = [];
    private List<DataFile> Files { get; set; } = [];
    private string? SelectedDirectory { get; set; }
    private DataFile? SelectedFile { get; set; }

    private bool DialogHidden { get; set; } = true;
    private string? NewDirectoryName { get; set; }
    private string RelativePath { get; set; } = string.Empty;
    private string RootPath { get; set; } = string.Empty;
    private bool IsLoading { get; set; } = true;

    protected override void OnInitialized()
    {
        CheckProject();
        RelativePath = PathConst.PromptPath;
        RootPath = Path.Combine(ProjectContext.SolutionPath!, PathConst.PromptPath);
        FileHelper = new(RootPath);
        LoadDirectories();
        IsLoading = false;
    }

    private void LoadDirectories()
    {
        Directories = FileHelper.GetDirectories();
        if (Directories.Count > 0)
        {
            SelectDirectory(Directories[0].Name);
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
        RelativePath = Path.Combine(PathConst.PromptPath, SelectedDirectory);
        LoadFiles();
    }

    private void LoadFiles()
    {
        if (!string.IsNullOrEmpty(SelectedDirectory))
        {
            Files = FileHelper.GetFiles(SelectedDirectory, ".prompt.md");
        }
        else
        {
            Files.Clear();
        }
        SelectedFile = null;
    }

    private void OpenAddDirectoryDialog()
    {
        DialogHidden = false;
    }

    private void AddDirectory()
    {
        if (NewDirectoryName != null)
        {
            FileHelper.AddDirectory(NewDirectoryName);
            DialogHidden = true;
            LoadDirectories();
            NewDirectoryName = null;
        }
    }

    private async Task DeleteDirectoryAsync()
    {
        if (!string.IsNullOrEmpty(SelectedDirectory))
        {
            var dialog = await DialogService.ShowConfirmationAsync(
                Lang(Localizer.Delete, Localizer.Directory),
                Lang(Localizer.Confirm),
                Lang(Localizer.Cancel),
                Lang(Localizer.ConfirmDeleteMessage)
            );

            var result = await dialog.Result;
            if (!result.Cancelled)
            {
                FileHelper.DeleteDirectory(Path.Combine(FileHelper.RootPath, SelectedDirectory));
                LoadDirectories();
            }
        }
        else
        {
            ToastService.ShowError(Lang(Localizer.MustSelectOption));
        }
    }

    private async Task OpenAddFileDialogAsync()
    {
        var data = new UpsertFileDto
        {
            DirectoryName = SelectedDirectory!,
            RootPath = RootPath,
            Suffix = ".prompt.md",
        };
        var dialog = await DialogService.ShowDialogAsync<UpsertFileDialog>(
            data,
            new DialogParameters { Width = "auto", Modal = false }
        );
        var result = await dialog.Result;
        if (!result.Cancelled)
        {
            ToastService.ShowSuccess(Lang(Localizer.Add, Localizer.Success));
            LoadFiles();
        }
    }

    private async Task DeleteFile(DataFile file)
    {
        var dialog = await DialogService.ShowConfirmationAsync(
            Lang(Localizer.Delete, Localizer.File),
            Lang(Localizer.Confirm),
            Lang(Localizer.Cancel),
            Lang(Localizer.ConfirmDeleteMessage)
        );

        var result = await dialog.Result;
        if (result.Cancelled)
            return;

        if (file != null)
        {
            FileHelper.DeleteFile(file.FullPath);
            LoadFiles();
        }
    }

    private async Task OpenEditFileDialog(DataFile file)
    {
        var data = new UpsertFileDto
        {
            DirectoryName = SelectedDirectory!,
            FileName = file.Name.Replace(".prompt.md", ""),
            RootPath = RootPath,
            Suffix = ".prompt.md",
        };
        var dialog = await DialogService.ShowDialogAsync<UpsertFileDialog>(
            data,
            new DialogParameters { Width = "auto", Modal = false }
        );
        var result = await dialog.Result;
        if (!result.Cancelled)
        {
            LoadFiles();
            ToastService.ShowSuccess(Lang(Localizer.Edit, Localizer.Success));
        }
    }
}
