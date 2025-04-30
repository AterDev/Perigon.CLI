using FileManagerMod.Models.FileDataDtos;
using Share.Implement;
namespace FileManagerMod.Controllers;

/// <summary>
/// 文件数据
/// </summary>
/// <see cref="Managers.FileDataManager"/>
public class FileDataController(
    Localizer localizer,
    UserContext user,
    ILogger<FileDataController> logger,
    FileDataManager manager
        ) : ClientControllerBase<FileDataManager>(localizer, manager, user, logger)
{

    /// <summary>
    /// 筛选 ✅
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    [HttpPost("filter")]
    public async Task<ActionResult<PageList<FileDataItemDto>>> FilterAsync(FileDataFilterDto filter)
    {
        return await _manager.ToPageAsync(filter);
    }

    /// <summary>
    /// 更新 ✅
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPatch("{id}")]
    public async Task<ActionResult<bool?>> UpdateAsync([FromRoute] Guid id, FileDataUpdateDto dto)
    {
        FileData? current = await _manager.GetCurrentAsync(id);
        if (current == null)
        {
            return NotFound(ErrorKeys.NotFoundResource);
        }
        ;
        return await _manager.UpdateAsync(current, dto);
    }

    /// <summary>
    /// 详情 ✅
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<FileDataDetailDto?>> GetDetailAsync([FromRoute] Guid id)
    {
        var res = await _manager.FindAsync<FileDataDetailDto>(d => d.Id == id);
        return (res == null) ? NotFound() : res;
    }

    /// <summary>
    /// 文件内容 ✅
    /// </summary>
    /// <param name="path"></param>
    /// <param name="md5"></param>
    /// <returns></returns>
    [HttpGet("content")]
    [AllowAnonymous]
    public async Task<ActionResult<string>> GetContentAsync(string path, string md5)
    {
        FileData? res = await _manager.GetByMd5Async(path, md5);
        if (res == null)
        {
            return NoContent();
        }

        var contentType = "application/octet-stream;charset=utf-8";
        string encodedFileName = System.Web.HttpUtility.UrlEncode(res.FileName, System.Text.Encoding.UTF8);
        Response.Headers.Append(new KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>("Content-Disposition", $"attachment; filename={encodedFileName}"));
        return new FileContentResult(res.Content, contentType);
    }

    /// <summary>
    /// 删除 ✅
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    // [ApiExplorerSettings(IgnoreApi = true)]
    [HttpDelete("{id}")]
    public async Task<ActionResult<bool?>> DeleteAsync([FromRoute] Guid id)
    {
        // 注意删除权限
        FileData? entity = await _manager.GetCurrentAsync(id);
        return entity == null ? NotFound() : await _manager.DeleteAsync([id], false);
    }
}