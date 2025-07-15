using System.Text;
using Microsoft.AspNetCore.Authorization;
using StudioMod.Models.ApiDocInfoDtos;

namespace AterStudio.Controllers;

/// <summary>
/// api文档
/// </summary>
[AllowAnonymous]
public class ApiDocInfoController(
    Localizer localizer,
    ApiDocInfoManager manager,
    IProjectContext project,
    ILogger<ApiDocInfoController> logger
) : BaseController<ApiDocInfoManager>(localizer, manager, project, logger)
{
    /// <summary>
    /// 获取项目文档
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<List<ApiDocInfoItemDto>>> ListAsync()
    {
        var filter = new ApiDocInfoFilterDto() { PageSize = 999, ProjectId = _project.ProjectId };
        var pager = await _manager.FilterAsync(filter);
        return pager.Data;
    }

    /// <summary>
    /// 获取某个文档信息
    /// </summary>
    /// <param name="id"></param>
    /// <param name="isFresh"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiDocContent?>> GetApiDocContentAsync(
        [FromRoute] Guid id,
        bool isFresh = true
    )
    {
        ApiDocContent? res = await _manager.GetContentAsync(id, isFresh);
        return res == null ? Problem(_manager.ErrorMsg) : res;
    }

    /// <summary>
    /// 导出markdown文档
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("export/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContentResult))]
    public async Task<ActionResult> ExportAsync([FromRoute] Guid id)
    {
        string content = await _manager.ExportDocAsync(id);
        return File(Encoding.UTF8.GetBytes(content), "application/octet-stream", "api-doc.md");
    }

    /// <summary>
    ///  添加
    /// </summary>
    /// <param name="apiDocInfo"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<Guid?>> AddAsync(ApiDocInfoAddDto apiDocInfo)
    {
        var entity = await _manager.CreateNewEntityAsync(apiDocInfo);
        var res = await _manager.AddAsync(entity);
        return res ? entity.Id : Problem("");
    }

    /// <summary>
    ///  更新
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<bool>> UpdateAsync([FromRoute] Guid id, ApiDocInfoUpdateDto dto)
    {
        var entity = await _manager.GetCurrentAsync(id);
        return entity == null ? NotFound("未找到该对象") : await _manager.UpdateAsync(entity, dto);
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult<bool?>> DeleteAsync([FromRoute] Guid id)
    {
        var entity = await _manager.FindAsync(id);
        return entity == null ? NotFound("未找到该对象") : await _manager.DeleteAsync([id], false);
    }
}
