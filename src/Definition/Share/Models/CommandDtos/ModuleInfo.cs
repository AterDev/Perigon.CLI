namespace Share.Models.CommandDtos;

/// <summary>
/// 模块信息
/// </summary>
public class ModuleInfo
{
    public const string User = "UserMod";
    public const string CMS = "CMSMod";
    public const string FileManager = "FileManagerMod";
    public const string Order = "OrderMod";
    public const string Customer = "CustomerMod";
    public const string System = "SystemMod";
    public const string Configuration = "ConfigurationMod";

    /// <summary>
    /// 模块名称
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// 模块标识
    /// </summary>
    public required string Value { get; set; }

    /// <summary>
    /// 模块描述
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// 获取所有模块内容
    /// </summary>
    /// <returns></returns>
    public static List<ModuleInfo> GetModules() =>
        [
            new ModuleInfo
            {
                Name = "Customer",
                Value = Customer,
                Description = Localizer.CustomerModuleDes,
            },
            new ModuleInfo
            {
                Name = "FileManage",
                Value = FileManager,
                Description = Localizer.FileManagerModuleDes,
            },
            new ModuleInfo
            {
                Name = "Order",
                Value = Order,
                Description = Localizer.OrderModuleDes,
            },
            new ModuleInfo
            {
                Name = "CMS",
                Value = CMS,
                Description = Localizer.CMSModuleDes,
            },
            //new ModuleInfo
            //{
            //    Name = "配置模块",
            //    Value = Configuration,
            //    Description = "配置模块"
            //}
        ];
}
