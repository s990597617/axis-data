using Axis.Domain.Enums;
using Axis.Domain.Mdm;

namespace Axis.Domain.System.Seeds;

/// <summary>
///     字典类型种子
/// </summary>
public sealed class SysDictTypeSeed : ISqlSugarEntitySeedData<SysDictType>
{
    public IEnumerable<SysDictType> HasData()
    {
        yield return Type(SysSeedIds.DictTypeStatus, "通用状态", "common_status", 1, "启用/禁用通用状态");
        yield return Type(SysSeedIds.DictTypeGender, "性别", "gender", 2, "人员性别");
        yield return Type(SysSeedIds.DictTypeYesNo, "是否", "yes_no", 3, "是/否通用选项");
        yield return Type(SysSeedIds.DictTypeAccountType, "账号类型", "account_type", 4, "系统账号类型");
        yield return Type(SysSeedIds.DictTypeOrgType, "机构类型", "org_type", 5, "组织机构层级类型");
        yield return Type(SysSeedIds.DictTypeCodeResetCycle, "编码重置周期", "code_reset_cycle", 6, "编码规则流水重置周期");
        yield return Type(SysSeedIds.DictTypeCodeCheckAlgo, "编码校验位", "code_check_algo", 7, "编码规则校验位算法");
        yield return Type(SysSeedIds.DictTypeCodeSourceType, "编码来源类型", "code_source_type", 8, "本厂生成或外部传入");
        yield return Type(SysSeedIds.DictTypeMaterialUnit, "物料计量单位", MdmDictCodes.MaterialUnit, 9, "物料主数据计量单位");
        yield return Type(SysSeedIds.DictTypeMaterialSource, "物料来源类型", MdmDictCodes.MaterialSource, 10, "外购 / 自制 / 委外，勿与编码来源字典混淆");
    }

    private static SysDictType Type(long id, string name, string code, int order, string remark)
    {
        return new SysDictType
        {
            Id = id,
            Name = name,
            Code = code,
            IsBuiltin = true,
            IsTenant = false,
            OrderNo = order,
            Status = StatusEnum.Enable,
            Remark = remark,
            CreatedBy = SysSeedIds.SeedCreatorId,
            Creator = SysSeedIds.SeedCreator,
            CreatedTime = SysSeedIds.SeedCreatedTime
        };
    }
}