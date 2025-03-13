using System.Text;
using Microsoft.AspNetCore.Authorization;
using Share.Models.ApiDocInfoDtos;

namespace AterStudio.Controllers;

/// <summary>
/// api文档
/// </summary>
[AllowAnonymous]
public class ApiDocInfoController(
    ApiDocInfoManager _manager,
    IProjectContext project,
    ILogger<ApiDocInfoController> logger
    ) : BaseController<ApiDocInfoManager>(_manager, project, logger)
{

    /// <summary>
    /// 获取项目文档
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<List<ApiDocInfoItemDto>>> ListAsync()
    {
        var filter = new ApiDocInfoFilterDto()
        {
            PageSize = 999,
            ProjectId = _project.ProjectId
        };
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
    public async Task<ActionResult<ApiDocContent?>> GetApiDocContentAsync([FromRoute] Guid id, bool isFresh = true)
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
        return entity == null
            ? NotFound("未找到该对象")
            : await _manager.DeleteAsync([id], false);
    }

    /// <summary>
    /// 生成前端请求
    /// </summary>
    /// <param name="id"></param>
    /// <param name="webPath"></param>
    /// <param name="type"></param>
    /// <param name="swaggerPath"></param>
    /// <returns></returns>
    [HttpGet("generateRequest/{id}")]
    public async Task<ActionResult<bool>> GenerateRequest([FromRoute] Guid id, string webPath, RequestLibType type, string? swaggerPath = null)
    {
        if (_project.Project == null)
        {
            return NotFound("项目不存在");
        }

        var entity = await _manager.GetCurrentAsync(id);
        if (entity == null)
        {
            return NotFound("未找到文档配置");
        }

        await _manager.GenerateRequestAsync(entity, webPath, type, swaggerPath);
        return true;
    }


    /// <summary>
    /// 生成Csharp请求
    /// </summary>
    /// <param name="id"></param>
    /// <param name="webPath"></param>
    /// <param name="swaggerPath"></param>
    /// <returns></returns>
    [HttpGet("generateCsharpRequest/{id}")]
    public async Task<ActionResult<bool>> GenerateCsharpRequest([FromRoute] Guid id, string webPath, string? swaggerPath = null)
    {
        if (_project.Project == null)
        {
            return NotFound("项目不存在");
        }

        var entity = await _manager.GetCurrentAsync(id);
        if (entity == null)
        {
            return NotFound("未找到文档配置");
        }

        await _manager.GenerateCsharpRequestAsync(webPath, swaggerPath);
        return true;
    }
}
