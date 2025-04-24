using FileManagerMod.Models.FileDataDtos;
using Framework.Common.Models;
using Framework.Common.Utils;
using Microsoft.AspNetCore.Http;
using SharedModule.Implement;

namespace FileManagerMod.Managers;
/// <summary>
/// 文件数据
/// </summary>
public class FileDataManager(
    DataAccessContext<FileData> dataContext,
    ILogger<FileDataManager> logger) : ManagerBase<FileData>(dataContext, logger)
{

    /// <summary>
    /// 添加新文件
    /// </summary>
    /// <param name="files"></param>
    /// <param name="folder"></param>
    /// <returns></returns>
    public async Task<List<FileData>> CreateNewEntityAsync(List<IFormFile> files, Folder? folder)
    {
        var res = new List<FileData>();
        if (files.Count != 0)
        {
            foreach (IFormFile file in files)
            {
                // read file stream to bytes
                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                var fileBytes = ms.ToArray();

                var md5 = HashCrypto.Md5FileHash(file.OpenReadStream());
                var exist = Query.Any(q => q.Md5 == md5);
                if (exist)
                {
                    continue;
                }

                var fileData = new FileData()
                {
                    FileName = file.FileName,
                    Extension = Path.GetExtension(file.FileName),
                    Md5 = md5,
                    Content = fileBytes
                };
                if (folder != null)
                {
                    fileData.Folder = folder;
                }

                if (!res.Any(q => q.Md5 == fileData.Md5))
                {
                    res.Add(fileData);
                }
            }
        }
        return res;
    }

    /// <summary>
    /// 添加新文件
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="folder"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public async Task<FileData> AddFileAsync(Stream stream, string folder, string fileName)
    {
        var md5 = HashCrypto.Md5FileHash(stream);
        stream.Seek(0, SeekOrigin.Begin);
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        var fileBytes = ms.ToArray();

        FileData? file = await Query.Where(q => q.Md5 == md5).FirstOrDefaultAsync();
        if (file != null)
        {
            return file;
        }
        var fileData = new FileData()
        {
            FileName = fileName,
            Extension = Path.GetExtension(fileName),
            Md5 = md5,
            Content = fileBytes
        };
        Folder? folderData = await CommandContext.Folders
            .Where(q => q.Name == folder)
            .FirstOrDefaultAsync();

        folderData ??= new Folder()
        {
            Name = folder,
            Path = folder
        };
        fileData.Folder = folderData;
        Command.Add(fileData);
        await SaveChangesAsync();
        return fileData;
    }

    /// <summary>
    /// 获取文件内容
    /// </summary>
    /// <param name="path"></param>
    /// <param name="md5"></param>
    /// <returns></returns>
    public async Task<FileData?> GetByMd5Async(string path, string md5)
    {
        FileData? fileContent = await Query.Where(q => q.Md5 == md5)
            .Where(q => q.Folder!.Name == path)
            .SingleOrDefaultAsync();
        return fileContent;
    }

    public async Task<int> AddFilesAsync(List<FileData> files)
    {
        await Command.AddRangeAsync(files);
        return await SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(FileData entity, FileDataUpdateDto dto)
    {
        entity.Merge(dto);
        return await UpdateAsync(entity);
    }

    public async Task<PageList<FileDataItemDto>> ToPageAsync(FileDataFilterDto filter)
    {
        Queryable = Queryable
            .WhereNotNull(filter.Md5, q => q.Md5 == filter.Md5)
            .WhereNotNull(filter.FileName, q => q.FileName.Contains(filter.FileName!))
            .Where(q => q.Folder!.Id == filter.FolderId);

        if (filter.FileType != null)
        {
            var extensions = new string[] { };
            switch (filter.FileType)
            {
                case FileType.Image:
                    extensions = [".jpg", ".png"];
                    break;
                case FileType.Text:
                    extensions = [".txt", ".json", ".md"];
                    break;
                case FileType.Compressed:
                    extensions = [".zip"];
                    break;
                case FileType.Docs:
                    extensions = [".docx", ".xlsx", ".pptx"];
                    break;
                default:
                    break;
            }
            Queryable = Queryable.Where(q => extensions.Contains(q.Extension));
        }

        return await ToPageAsync<FileDataFilterDto, FileDataItemDto>(filter);
    }

    /// <summary>
    /// 当前用户所拥有的对象
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<FileData?> GetOwnedAsync(Guid id)
    {
        IQueryable<FileData> query = Command.Where(q => q.Id == id);
        // 获取用户所属的对象
        // query = query.Where(q => q.User.Id == _userContext.UserId);
        return await query.FirstOrDefaultAsync();
    }

}
