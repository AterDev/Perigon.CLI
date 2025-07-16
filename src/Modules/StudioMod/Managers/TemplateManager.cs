namespace StudioMod.Managers;

public class TemplateManager : ILocalDirectoryManager
{
    public LocalFileHelper FileHelper { get; }

    public TemplateManager() => FileHelper = new LocalFileHelper(ConstVal.TemplateDir);

    /// <summary>
    /// 获取指定目录下的razor文件信息
    /// </summary>
    public List<DataFile> GetRazorFiles(string directoryName) =>
        ((ILocalDirectoryManager)this).GetFiles(directoryName, ".razor");

    public void AddRazorFile(string directoryName, string fileName, string content = "")
    {
        if (!fileName.EndsWith(".razor"))
            fileName += ".razor";
        ((ILocalDirectoryManager)this).AddFile(directoryName, fileName, content);
    }
}
