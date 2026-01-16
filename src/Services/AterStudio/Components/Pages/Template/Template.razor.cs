using CodeGenerator.Helper;
using Entity;
using Microsoft.AspNetCore.Components;

namespace AterStudio.Components.Pages.Template;

public partial class Template
{
    [Inject]
    private GenStepManager GenStepManager { get; set; } = null!;

    private LocalFileHelper FileHelper { get; set; } = default!;
    private List<DataFile> Directories { get; set; } = [];
    private List<DataFile> Files { get; set; } = [];
    private string? SelectedDirectory { get; set; }
    private DataFile? SelectedFile { get; set; }
    private IEnumerable<DataFile> SelectedFiles { get; set; } = [];

    private bool DialogHidden { get; set; } = true;
    private string? NewDirectoryName { get; set; }
    private string RelativePath { get; set; } = string.Empty;

    private string RootPath { get; set; } = string.Empty;

    private bool IsLoading { get; set; } = true;
    private FluentTextField? _addDirField;

    protected override void OnInitialized()
    {
        CheckProject();
        RelativePath = ConstVal.TemplateDir;
        RootPath = Path.Combine(ProjectContext.SolutionPath!, ConstVal.TemplateDir);
        FileHelper = new LocalFileHelper(RootPath);
        LoadDirectories();

        IsLoading = false;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!DialogHidden && _addDirField is not null)
        {
            _addDirField.FocusAsync();
        }
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
        RelativePath = Path.Combine(ConstVal.TemplateDir, SelectedDirectory);
        LoadFiles();
    }

    private void LoadFiles()
    {
        if (!string.IsNullOrEmpty(SelectedDirectory))
        {
            Files = FileHelper.GetFiles(SelectedDirectory, ".razor");
        }
        else
        {
            Files.Clear();
        }
        SelectedFile = null;
        SelectedFiles = [];
    }

    private void OnSelectedFilesChanged(IEnumerable<DataFile> files)
    {
        SelectedFiles = files ?? [];
    }

    private async Task OpenGenerateStepsDialogAsync()
    {
        if (!SelectedFiles.Any())
        {
            ToastService.ShowError(Lang(Localizer.MustSelectOption));
            return;
        }

        var dialog = await DialogService.ShowDialogAsync<GenerateStepsDialog>(
            new GenerateStepsDialog.Model(),
            new DialogParameters { Width = "460px", Modal = true }
        );

        var result = await dialog.Result;
        if (result.Cancelled)
            return;

        var model = (GenerateStepsDialog.Model?)result.Data;
        if (model is null)
            return;

        if (string.IsNullOrWhiteSpace(ProjectContext.SolutionPath))
        {
            ToastService.ShowError(Lang(Localizer.DirectoryNotFound));
            return;
        }

        if (ProjectContext.SolutionId is null)
        {
            ToastService.ShowError(Lang(Localizer.DirectoryNotFound));
            return;
        }

        var res = await GenStepManager.UpsertStepsFromTemplatesAsync(
            SelectedFiles,
            SelectedDirectory,
            ProjectContext.SolutionId.Value,
            model.RelativeOutputDir
        );

        ToastService.ShowSuccess(Lang(Localizer.Success));
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
                LangWithArguments(Localizer.ConfirmDeleteMessage, Lang(Localizer.Directory))
            );

            var result = await dialog.Result;
            if (!result.Cancelled)
            {
                LocalFileHelper.DeleteDirectory(
                    Path.Combine(FileHelper.RootPath, SelectedDirectory)
                );
                LoadDirectories();
            }
        }
        else
        {
            ToastService.ShowError(Lang(Localizer.MustSelectOption));
        }
    }

    private async Task OpenHelpDialogAsync()
    {
        var dialog = await DialogService.ShowDialogAsync<HelpDialog>(
            new DialogParameters { Width = "auto", Modal = false }
        );
    }

    private async Task OpenAddFileDialogAsync()
    {
        var data = new UpsertFileDto
        {
            DirectoryName = SelectedDirectory!,
            RootPath = RootPath,
            Suffix = ".razor",
        };
        var dialog = await DialogService.ShowDialogAsync<UpsertFileDialog>(
            data,
            new DialogParameters { Width = "800px", Modal = false }
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
            LangWithArguments(Localizer.ConfirmDeleteMessage, Lang(Localizer.File))
        );

        var result = await dialog.Result;
        if (result.Cancelled)
            return;

        if (file != null)
        {
            LocalFileHelper.DeleteFile(file.FullPath);
            LoadFiles();
        }
    }

    private async Task OpenEditFileDialog(DataFile file)
    {
        var data = new UpsertFileDto
        {
            DirectoryName = SelectedDirectory!,
            FileName = Path.GetFileNameWithoutExtension(file.Name),
            RootPath = RootPath,
            Suffix = ".razor",
        };
        var dialog = await DialogService.ShowDialogAsync<UpsertFileDialog>(
            data,
            new DialogParameters { Width = "800px", Modal = false }
        );
        var result = await dialog.Result;
        if (!result.Cancelled)
        {
            LoadFiles();
            ToastService.ShowSuccess(Lang(Localizer.Edit, Localizer.Success));
        }
    }
}
