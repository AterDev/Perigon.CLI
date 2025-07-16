namespace StudioMod.Managers;

public interface ILocalDirectoryManager
{
    LocalFileHelper FileHelper { get; }

    List<DataFile> GetDirectoryList() => FileHelper.GetDirectories();

    void AddDirectory(string directoryName)
    {
        var path = Path.Combine(FileHelper.RootPath, directoryName);
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
    }

    void AddFile(string directoryName, string fileName, string content = "")
    {
        var dirPath = Path.Combine(FileHelper.RootPath, directoryName);
        if (!Directory.Exists(dirPath))
            Directory.CreateDirectory(dirPath);
        var filePath = Path.Combine(dirPath, fileName);
        File.WriteAllText(filePath, content);
    }

    void EditDirectoryName(string directoryPath, string newName) =>
        FileHelper.RenameDirectory(directoryPath, newName);

    void DeleteDirectory(string directoryPath) => FileHelper.DeleteDirectory(directoryPath);

    List<DataFile> GetFiles(string directoryName, string? extension = null) =>
        FileHelper.GetFiles(directoryName, extension);

    void EditFileName(string filePath, string newName) => FileHelper.RenameFile(filePath, newName);

    string GetFileContent(string filePath) => FileHelper.GetFileContent(filePath);

    void EditFileContent(string filePath, string content) =>
        FileHelper.UpdateFileContent(filePath, content);

    void DeleteFile(string filePath) => FileHelper.DeleteFile(filePath);
}
