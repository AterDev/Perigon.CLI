using CodeGenerator.Models;

namespace StudioMod.Managers;

public class AdvanceManager(CommandDbContext dbContext, IProjectContext projectContext)
{
    private readonly CommandDbContext _dbContext = dbContext;
    private readonly IProjectContext _projectContext = projectContext;

    /// <summary>
    /// 设置配置
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public async Task SetConfigAsync(string key, string value)
    {
        ConfigData? config = _dbContext.Configs.FirstOrDefault(c => c.Key == key);
        if (config != null)
        {
            config.Value = value;
            _dbContext.Configs.Update(config);
        }
        else
        {
            config = new ConfigData
            {
                Key = key,
                Value = value,
            };

            _dbContext.Configs.Add(config);
        }

        await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// 获取配置
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public ConfigData? GetConfig(string key)
    {
        return _dbContext.Configs.FirstOrDefault(c => c.Key == key);
    }

    /// <summary>
    /// 获取实体表结构
    /// </summary>
    /// <returns>markdown format</returns>
    public async Task<string> GetDatabaseStructureAsync()
    {
        var sb = new StringBuilder();
        var entityFiles = Directory.GetFiles(_projectContext.EntityPath!, $"*.cs", SearchOption.AllDirectories)
            .ToList();

        entityFiles = entityFiles.Where(f => !(f.EndsWith(".g.cs")
                || f.EndsWith(".AssemblyAttributes.cs")
                || f.EndsWith(".AssemblyInfo.cs")
                || f.EndsWith("GlobalUsings.cs")
                || f.EndsWith("Const.cs")
                || f.EndsWith("Modules.cs"))
                ).ToList();

        if (entityFiles.Count > 0)
        {
            var entityCompilation = new EntityParseHelper(entityFiles[0]);
            foreach (var entityPath in entityFiles)
            {
                if (entityPath != null)
                {
                    var entityInfo = await entityCompilation.ParseEntityAsync(entityPath);
                    if (entityInfo != null)
                    {
                        string content = ToMarkdown(entityInfo);
                        sb.AppendLine(content);
                    }
                }
            }
        }
        return sb.ToString();
    }

    /// <summary>
    /// 实体转换为 markdown table
    /// </summary>
    /// <param name="entityInfo"></param>
    /// <returns></returns>
    public string ToMarkdown(EntityInfo entityInfo)
    {
        StringBuilder sb = new();
        sb.AppendLine($"## {entityInfo.Name} ({entityInfo.Summary})");
        sb.AppendLine($"|字段名|类型|是否可空|说明|");
        sb.AppendLine($"|---|---|---|---|");
        foreach (PropertyInfo property in entityInfo.PropertyInfos)
        {
            string comment = string.IsNullOrWhiteSpace(property.CommentSummary)
                ? "无"
                : property.CommentSummary;

            sb.AppendLine($"|{property.Name}|{property.Type}|{property.IsNullable}|{comment}|");
        }
        return sb.ToString();
    }
}


public enum ImageSize
{
    /// <summary>
    /// 256*256
    /// </summary>
    Small,
    /// <summary>
    /// 512*512
    /// </summary>
    Middle,
    /// <summary>
    /// 1024*1024
    /// </summary>
    Large
}