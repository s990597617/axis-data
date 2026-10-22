using SqlSugar;
using System.Linq.Dynamic.Core;
using System.Linq.Dynamic.Core.CustomTypeProviders;

namespace Axis.Infrastructure.Components.Database;

/// <summary>
/// 扩展支持 SqlFunc
/// </summary>
public class SqlSugarTypeProvider(bool cacheCustomTypes = true) : DefaultDynamicLinqCustomTypeProvider(ParsingConfig.Default, cacheCustomTypes)
{
    public override HashSet<Type> GetCustomTypes()
    {
        var customTypes = base.GetCustomTypes();
        customTypes.Add(typeof(SqlFunc));
        return customTypes;
    }
}