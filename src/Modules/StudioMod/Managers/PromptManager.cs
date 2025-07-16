namespace StudioMod.Managers;

public class PromptManager : ILocalDirectoryManager
{
    public LocalFileHelper FileHelper { get; }

    public PromptManager() => FileHelper = new LocalFileHelper(PathConst.PromptPath);

    /// <summary>
    /// 获取指定目录下的prompt文件信息（如需筛选可自定义扩展）
    /// </summary>
    public List<DataFile> GetPromptFiles(string directoryName) =>
        ((ILocalDirectoryManager)this).GetFiles(directoryName, ".prompt.md");

    public void AddPromptFile(string directoryName, string fileName, string content = "")
    {
        if (!fileName.EndsWith(".prompt.md"))
            fileName += ".prompt.md";
        ((ILocalDirectoryManager)this).AddFile(directoryName, fileName, content);
    }
}
