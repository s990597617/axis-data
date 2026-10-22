namespace Axis.Infrastructure.Components.Excel;

/// <summary>
/// Excel 读写抽象：业务侧只依赖本接口，不直接引用 MiniExcel。
/// </summary>
public interface IExcelService
{
    /// <summary>
    /// 将行集合导出为 xlsx 字节（含表头）。
    /// </summary>
    /// <remarks>
    /// 行模型上带 <c>DataMaskAttribute</c> 的 string 属性会在写出前自动脱敏。
    /// </remarks>
    /// <typeparam name="T">行模型类型；可用 <c>ExcelColumnName</c> 指定中文列名。</typeparam>
    /// <param name="rows">数据行；为空时仍写出表头（写入一行空对象）。</param>
    byte[] Export<T>(IEnumerable<T> rows) where T : class, new();

    /// <summary>
    /// 从 xlsx 流按列名映射读取行集合。
    /// </summary>
    /// <typeparam name="T">行模型类型。</typeparam>
    /// <param name="stream">xlsx 文件流；调用方负责释放。</param>
    IReadOnlyList<T> Query<T>(Stream stream) where T : class, new();
}
