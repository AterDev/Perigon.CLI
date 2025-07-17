namespace CodeGenerator.Helper;

/// <summary>
/// 文件信息
/// </summary>
public class DataFile
{
    public required string Name { get; set; }
    public required string FullPath { get; set; }

    /// <summary>
    ///
    /// </summary>
    public long? Size { get; set; }

    /// <summary>
    /// 最后更新时间
    /// </summary>
    public DateTimeOffset? LatestTime { get; set; }
}

/// <summary>
/// 本地文件助手
/// </summary>
public class LocalFileHelper
{
    /// <summary>
    /// 根路径
    /// </summary>
    public string RootPath { get; set; }

    public LocalFileHelper(string rootPath)
    {
        if (!Directory.Exists(rootPath))
        {
            Directory.CreateDirectory(rootPath);
        }
        RootPath = rootPath;
    }

    public void AddDirectory(string directoryName)
    {
        var dirPath = Path.Combine(RootPath, directoryName);
        if (Directory.Exists(dirPath))
            throw new IOException($"Directory already exists: {dirPath}");
        Directory.CreateDirectory(dirPath);
    }

    public void AddFile(string directoryName, string fileName, string content)
    {
        var dirPath = Path.Combine(RootPath, directoryName);
        if (!Directory.Exists(dirPath))
            throw new DirectoryNotFoundException($"Directory not found: {dirPath}");
        var filePath = Path.Combine(dirPath, fileName);
        if (File.Exists(filePath))
            throw new IOException($"File already exists: {filePath}");
        File.WriteAllText(filePath, content);
    }

    /// <summary>
    /// 获取根路径下所有目录
    /// </summary>
    public List<DataFile> GetDirectories()
    {
        var dirs = Directory.GetDirectories(RootPath);
        var result = new List<DataFile>();
        foreach (var dir in dirs)
        {
            result.Add(new DataFile { Name = Path.GetFileName(dir), FullPath = dir });
        }
        return result;
    }

    /// <summary>
    /// 获取指定目录下所有文件，可按后缀筛选
    /// </summary>
    /// <param name="directoryName">目录名称（非完整路径）</param>
    /// <param name="extension">可选后缀名（如 .txt）</param>
    public List<DataFile> GetFiles(string directoryName, string? extension = null)
    {
        var dirPath = Path.Combine(RootPath, directoryName);
        if (!Directory.Exists(dirPath))
            throw new DirectoryNotFoundException($"Directory not found: {dirPath}");
        var files = Directory.GetFiles(dirPath);
        var result = new List<DataFile>();
        foreach (var file in files)
        {
            if (
                extension == null
                || Path.GetExtension(file).Equals(extension, StringComparison.OrdinalIgnoreCase)
            )
            {
                result.Add(
                    new DataFile
                    {
                        Name = Path.GetFileName(file),
                        FullPath = file,
                        Size = new FileInfo(file).Length,
                        LatestTime = File.GetLastWriteTimeUtc(file),
                    }
                );
            }
        }
        return result;
    }

    /// <summary>
    /// 获取文件内容
    /// </summary>
    public string GetFileContent(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");
        return File.ReadAllText(filePath);
    }

    /// <summary>
    /// 更新文件内容（覆盖）
    /// </summary>
    public void UpdateFileContent(string filePath, string content)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");
        File.WriteAllText(filePath, content);
    }

    /// <summary>
    /// 删除文件
    /// </summary>
    public void DeleteFile(string filePath)
    {
        if (File.Exists(filePath))
            File.Delete(filePath);
    }

    /// <summary>
    /// 删除目录及所有子内容
    /// </summary>
    public void DeleteDirectory(string directoryPath)
    {
        if (Directory.Exists(directoryPath))
            Directory.Delete(directoryPath, true);
    }

    public void RenameFile(string filePath, string newName)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");
        var directory = Path.GetDirectoryName(filePath);
        if (directory == null)
            throw new InvalidOperationException("Unable to determine file directory.");
        var newFilePath = Path.Combine(directory, newName);
        if (File.Exists(newFilePath))
            throw new IOException($"File already exists: {newFilePath}");
        File.Move(filePath, newFilePath);
    }

    /// <summary>
    /// rename a directory
    /// </summary>
    /// <param name="directoryPath"></param>
    /// <param name="newName"></param>
    /// <exception cref="DirectoryNotFoundException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="IOException"></exception>
    public void RenameDirectory(string directoryPath, string newName)
    {
        if (!Directory.Exists(directoryPath))
            throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");

        var parentDir = Path.GetDirectoryName(directoryPath);
        if (parentDir == null)
            throw new InvalidOperationException("Unable to determine parent directory.");
        var newDirPath = Path.Combine(parentDir, newName);
        if (Directory.Exists(newDirPath))
            throw new IOException($"Directory already exists: {newDirPath}");
        Directory.Move(directoryPath, newDirPath);
    }
}
