using Axis.Infrastructure.Components.DataMasking;
using Axis.Infrastructure.Dependency.Annotations;
using MiniExcelLibs;

namespace Axis.Infrastructure.Components.Excel;

/// <summary>
/// 基于 MiniExcel 的 Excel 读写实现。
/// </summary>
public sealed class MiniExcelService : IExcelService, IAutoInject
{
    /// <summary>
    /// 将行集合导出为 xlsx 字节（含表头）；自动对带 <see cref="DataMaskAttribute"/> 的字段脱敏。
    /// </summary>
    /// <typeparam name="T">行模型类型。</typeparam>
    /// <param name="rows">数据行；为空时写入一行空对象以生成表头。</param>
    public byte[] Export<T>(IEnumerable<T> rows) where T : class, new()
    {
        var list = rows as IList<T> ?? rows.ToList();
        if (list.Count == 0)
            list = [new T()];

        // 导出前浅拷贝并打码，避免污染调用方持有的业务对象。
        list = DataMaskExportHelper.ApplyForExport(list);

        using var stream = new MemoryStream();
        stream.SaveAs(list);
        return stream.ToArray();
    }

    /// <summary>
    /// 从 xlsx 流按列名映射读取行集合。
    /// </summary>
    /// <typeparam name="T">行模型类型。</typeparam>
    /// <param name="stream">xlsx 文件流。</param>
    public IReadOnlyList<T> Query<T>(Stream stream) where T : class, new()
    {
        if (stream.CanSeek)
            stream.Position = 0;

        return stream.Query<T>().ToList();
    }
}
