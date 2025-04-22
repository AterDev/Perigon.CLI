namespace Framework.Web.Convention.Abstraction;
/// <summary>
/// 配置文件上下文
/// 不支持动态读取
/// </summary>
public interface ISettingContextBase
{
    /// <summary>
    /// 获取值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    T? GetValue<T>(string key);

}
