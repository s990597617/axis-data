using Axis.Infrastructure.Domain.Interfaces;

namespace Axis.Infrastructure.Domain
{
    /// <summary>
    /// 数据权限实体
    /// </summary>
    internal class BaseDataEntity : BaseEntity, IOrgId
    {
        /// <summary>
        /// 机构 id
        /// </summary>
        public long OrgId { get; set; }
    }
}
