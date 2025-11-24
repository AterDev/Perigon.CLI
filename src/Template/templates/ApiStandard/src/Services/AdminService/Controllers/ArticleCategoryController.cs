using Ater.AspNetCore.Models;
using CMSMod.Managers;
using CMSMod.Models.ArticleCategoryDtos;
using Entity.CMSMod;

namespace AdminService.Controllers;

public class ArticleCategoryController(
    Localizer localizer,
    IUserContext user,
    ILogger<SystemConfigController> logger,
    ArticleCategoryManager manager
    ) : RestControllerBase<ArticleCategoryManager>(localizer, manager, user, logger)
{

    /// <summary>
    /// 获取配置列表 ✅
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    [HttpGet("list")]
    public async Task<PageList<ArticleCategoryItemDto>> ListAsync(
        ArticleCategoryFilterDto filter
    )
    {
        return await _manager.FilterAsync(filter);
    }

    /// <summary>
    /// add ✅
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<ArticleCategory>> AddAsync(ArticleCategoryAddDto dto)
    {
        var entity = await _manager.AddAsync(dto);
        return CreatedAtAction(nameof(DetailAsync), new { id = entity.Id }, entity);
    }

    /// <summary>
    /// 更新 ✅
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPatch("{id}")]
    public async Task<bool> UpdateAsync([FromRoute] Guid id, ArticleCategoryUpdateDto dto)
    {
        return await _manager.EditAsync(id, dto) == 1;
    }

    /// <summary>
    /// 详情 ✅
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ArticleCategoryDetailDto?> DetailAsync([FromRoute] Guid id)
    {
        return await _manager.GetAsync(id);
    }

    /// <summary>
    /// ⚠删除 ✅
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteAsync([FromRoute] Guid id)
    {
        return await _manager.DeleteAsync([id], false);
    }
}
