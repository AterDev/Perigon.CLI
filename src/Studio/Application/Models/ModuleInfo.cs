namespace StudioMod.Models;
/// <summary>
/// 模块信息
/// </summary>
public class ModuleInfo
{
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
    public static List<ModuleInfo> GetModules() => [
            new() {
                Name = "系统模块",
                Value = System,
                Description = "含角色/用户/菜单/系统日志等基础功能"
            },
            new ModuleInfo
            {
                Name = "客户模块",
                Value = Customer,
                Description = "客户信息管理"
            },
            new ModuleInfo
            {
                Name = "文件管理模块",
                Value = FileManager,
                Description = "文件管理"
            },
            new ModuleInfo
            {
                Name = "订单模块",
                Value = Order,
                Description = "含产品/订单/对账单等功能"
            },
            new ModuleInfo
            {
                Name = "内容管理模块",
                Value = CMS,
                Description = "含博客及分类等基础功能"
            },
            //new ModuleInfo
            //{
            //    Name = "配置模块",
            //    Value = Configuration,
            //    Description = "配置模块"
            //}
        ];
}
