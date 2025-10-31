namespace Ater.AspNetCore.Models;

public enum UserActionType
{
    /// <summary>
    /// 其它
    /// </summary>
    [Description("其它")]
    Else,

    /// <summary>
    /// 登录
    /// </summary>
    [Description("登录")]
    Login,

    /// <summary>
    /// 添加
    /// </summary>
    [Description("添加")]
    Add,

    /// <summary>
    /// 更新
    /// </summary>
    [Description("更新")]
    Update,

    /// <summary>
    /// 删除
    /// </summary>
    [Description("删除")]
    Delete,

    /// <summary>
    /// 审查
    /// </summary>
    [Description("审核")]
    Audit,

    /// <summary>
    /// 导入
    /// </summary>
    [Description("导入")]
    Import,

    /// <summary>
    /// 导出
    /// </summary>
    [Description("导出")]
    Export,
}
