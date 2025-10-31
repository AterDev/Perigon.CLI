using System.IO.Compression;

namespace Ater.AspNetCore.Utils;

public static class ZipHelper
{
    /// <summary>
    /// 压缩
    /// </summary>
    /// <param name="inputPath">文件或目录</param>
    /// <param name="outputPath">输出zip文件完整路径</param>
    public static void Compress(string inputPath, string outputPath)
    {
        // 判断输入路径是文件还是目录
        var isFile = File.Exists(inputPath);

        // 创建输出文件
        using FileStream outputFile = File.Create(outputPath);
        // 创建一个 ZipArchive
        using var zip = new ZipArchive(outputFile, ZipArchiveMode.Create);
        if (isFile)
        {
            // 如果是文件，添加一个 ZipArchiveEntry
            zip.CreateEntryFromFile(inputPath, Path.GetFileName(inputPath));
        }
        else
        {
            // 如果是目录，遍历所有子文件和子目录
            foreach (
                var entry in Directory.EnumerateFileSystemEntries(
                    inputPath,
                    "*",
                    SearchOption.AllDirectories
                )
            )
            {
                // 获取相对路径
                var relativePath = Path.GetRelativePath(inputPath, entry);

                // 判断是文件还是目录
                var isSubFile = File.Exists(entry);

                if (isSubFile)
                {
                    // 如果是文件，添加一个 ZipArchiveEntry
                    zip.CreateEntryFromFile(entry, relativePath);
                }
                else
                {
                    // 如果是目录，创建一个空的 ZipArchiveEntry
                    zip.CreateEntry(relativePath + Path.DirectorySeparatorChar);
                }
            }
        }
    }

    /// <summary>
    /// 解压
    /// </summary>
    /// <param name="inputPath"></param>
    /// <param name="outputPath"></param>
    public static void Decompress(string inputPath, string outputPath)
    {
        // 打开输入文件
        using FileStream inputFile = File.OpenRead(inputPath);
        // 创建一个 ZipArchive
        using var zip = new ZipArchive(inputFile, ZipArchiveMode.Read);
        // 遍历所有 ZipArchiveEntry
        foreach (ZipArchiveEntry entry in zip.Entries)
        {
            // 获取输出路径
            var entryOutputPath = Path.Combine(outputPath, entry.FullName);

            // 判断是文件还是目录
            var isFile = !entryOutputPath.EndsWith(Path.DirectorySeparatorChar);

            if (isFile)
            {
                // 如果是文件，创建输出目录
                var dir = Path.GetDirectoryName(entryOutputPath);
                if (dir != null)
                {
                    Directory.CreateDirectory(dir);
                }
                // 将 ZipArchiveEntry 的内容复制到输出文件
                entry.ExtractToFile(entryOutputPath, true);
            }
            else
            {
                // 如果是目录，创建输出目录
                Directory.CreateDirectory(entryOutputPath);
            }
        }
    }

    /// <summary>
    /// 压缩文件或目录到流
    /// </summary>
    /// <param name="inputPath"></param>
    /// <param name="output"></param>
    public static void CompressToStream(string inputPath, Stream output)
    {
        // 判断输入路径是文件还是目录
        var isFile = File.Exists(inputPath);

        // 创建一个 ZipArchive
        using var zip = new ZipArchive(output, ZipArchiveMode.Create, true);
        if (isFile)
        {
            // 如果是文件，添加一个 ZipArchiveEntry
            zip.CreateEntryFromFile(inputPath, Path.GetFileName(inputPath));
        }
        else
        {
            // 如果是目录，遍历所有子文件和子目录
            foreach (
                var entry in Directory.EnumerateFileSystemEntries(
                    inputPath,
                    "*",
                    SearchOption.AllDirectories
                )
            )
            {
                // 获取相对路径
                var relativePath = Path.GetRelativePath(inputPath, entry);

                // 判断是文件还是目录
                var isSubFile = File.Exists(entry);

                if (isSubFile)
                {
                    // 如果是文件，添加一个 ZipArchiveEntry
                    zip.CreateEntryFromFile(entry, relativePath);
                }
                else
                {
                    // 如果是目录，创建一个空的 ZipArchiveEntry
                    zip.CreateEntry(relativePath + Path.DirectorySeparatorChar);
                }
            }
        }
    }

    /// <summary>
    /// 从流解压缩文件或目录
    /// </summary>
    /// <param name="input"></param>
    /// <param name="outputPath"></param>
    public static void DecompressFromStream(Stream input, string outputPath)
    {
        // 创建一个 ZipArchive
        using var zip = new ZipArchive(input, ZipArchiveMode.Read);
        // 遍历所有 ZipArchiveEntry
        foreach (ZipArchiveEntry entry in zip.Entries)
        {
            // 获取输出路径
            var entryOutputPath = Path.Combine(outputPath, entry.FullName);

            // 判断是文件还是目录
            var isFile = !entryOutputPath.EndsWith(Path.DirectorySeparatorChar);

            if (isFile)
            {
                // 如果是文件，创建输出目录
                var dir = Path.GetDirectoryName(entryOutputPath);
                if (dir != null)
                {
                    Directory.CreateDirectory(dir);
                }

                // 将 ZipArchiveEntry 的内容复制到输出文件
                entry.ExtractToFile(entryOutputPath, true);
            }
            else
            {
                // 如果是目录，创建输出目录
                Directory.CreateDirectory(entryOutputPath);
            }
        }
    }
}
