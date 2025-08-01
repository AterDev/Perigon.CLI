using OfficeOpenXml;
using OfficeOpenXml.Export.ToCollection;

namespace Ater.Web.Extension.Services;
/// <summary>
/// excel 操作类
/// </summary>
public class ExcelService
{
    public const string MimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public ExcelService()
    {

    }

    /// <summary>
    /// 快捷导出
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <param name="sheetName"></param>
    /// <param name="hasTitle">是否包含标题</param>
    /// <returns></returns>
    public static async Task<Stream> ExportAsync<T>(IEnumerable<T> data, string sheetName = "sheet1", bool hasTitle = true)
    {
        var stream = new MemoryStream();
        using (var package = new ExcelPackage(stream))
        {
            ExcelWorksheet sheet = package.Workbook.Worksheets.Add(sheetName);
            var list = data.ToList();
            var excelRange = sheet.Cells[1, 1].LoadFromCollection(list, hasTitle);

            if (hasTitle)
            {
                sheet.Cells[1, 1, 1, excelRange.Columns].Style.Font.Bold = true;
            }

            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
            await package.SaveAsync();
        }
        stream.Position = 0;
        return stream;
    }

    /// <summary>
    /// 快捷导入
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="stream"></param>
    /// <param name="sheetName"></param>
    /// <param name="hasTitle">是否包含标题</param>
    /// <returns></returns>
    public static List<T> Import<T>(Stream stream, string? sheetName = null, bool hasTitle = true)
    {
        var data = new List<T>();
        using var package = new ExcelPackage(stream);
        ExcelWorksheet sheet = sheetName == null ? package.Workbook.Worksheets[0] : package.Workbook.Worksheets[sheetName];

        var rows = sheet.Dimension.Rows;
        var columns = sheet.Dimension.Columns;

        var range = sheet.Dimension.Address;

        data = sheet.Cells[range].ToCollection<T>(options =>
        {
            options.HeaderRow = hasTitle ? 0 : 1;
            options.DataStartRow = hasTitle ? 1 : 0;
            options.ConversionFailureStrategy = ToCollectionConversionFailureStrategy.SetDefaultValue;
        });
        return data;
    }
}
