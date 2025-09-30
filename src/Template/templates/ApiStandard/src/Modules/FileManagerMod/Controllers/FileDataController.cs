using FileManagerMod.Models.FileDataDtos;
using Microsoft.AspNetCore.Http;

namespace FileManagerMod.Controllers;

/// <summary>
/// 文件数据
/// </summary>
/// <see cref="FileDataManager"/>
public class FileDataController(
    Localizer localizer,
    UserContext user,
    ILogger<FileDataController> logger,
    FileDataManager manager,
    FolderManager folderManager
) : RestControllerBase<FileDataManager>(localizer, manager, user, logger)
{
    private readonly FolderManager _folderManager = folderManager;

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
    /// 上传 ✅
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<int>> AddAsync(Guid? folderId, List<IFormFile> files)
    {
        Folder? folder = null;
        if (folderId != null)
        {
            folder = await _folderManager.GetCurrentAsync(folderId.Value);
            if (folder == null)
            {
                return NotFound("错误的目录");
            }
        }
        List<FileData> data = await _manager.CreateNewEntityAsync(files, folder);
        return await _manager.AddFilesAsync(data);
    }

    /// <summary>
    /// 上传文件
    /// </summary>
    /// <param name="path"></param>
    /// <param name="file"></param>
    /// <returns></returns>
    [RequestSizeLimit(1024 * 500)]
    [HttpPost("upload")]
    public async Task<ActionResult<string>> UploadAsync(string path, IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName);
        FileData fileData = await _manager.AddFileAsync(file.OpenReadStream(), path, file.FileName);
        return fileData != null ? fileData.Md5 : Problem("上传失败");
    }

    /// <summary>
    /// 详情 ✅
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<FileDataDetailDto?>> GetDetailAsync([FromRoute] Guid id)
    {
        var res = await _manager.FindAsync<FileDataDetailDto>(d => d.Id == id);
        return (res == null) ? NotFound() : res;
    }

    /// <summary>
    /// ⚠删除 ✅
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    // [ApiExplorerSettings(IgnoreApi = true)]
    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteAsync([FromRoute] Guid id)
    {
        // 注意删除权限
        var entity = await _manager.GetCurrentAsync(id);
        if (entity == null)
        {
            return NotFound();
        }
        ;
        return entity == null ? NotFound() : await _manager.DeleteAsync([id], false);
    }
}
