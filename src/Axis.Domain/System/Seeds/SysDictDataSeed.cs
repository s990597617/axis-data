using Axis.Domain.Enums;

namespace Axis.Domain.System.Seeds;

/// <summary>
///     字典数据种子
/// </summary>
public sealed class SysDictDataSeed : ISqlSugarEntitySeedData<SysDictData>
{
    public IEnumerable<SysDictData> HasData()
    {
        // ===== common_status（通用状态） =====
        yield return Data(SysSeedIds.DictDataStatusEnable, SysSeedIds.DictTypeStatus, "启用", "1", "success", 1, true);
        yield return Data(SysSeedIds.DictDataStatusDisable, SysSeedIds.DictTypeStatus, "禁用", "0", "danger", 2);

        // ===== gender（性别） =====
        yield return Data(SysSeedIds.DictDataGenderUnknown, SysSeedIds.DictTypeGender, "未知", "0", "info", 1, true);
        yield return Data(SysSeedIds.DictDataGenderMale, SysSeedIds.DictTypeGender, "男", "1", "primary", 2);
        yield return Data(SysSeedIds.DictDataGenderFemale, SysSeedIds.DictTypeGender, "女", "2", "warning", 3);

        // ===== yes_no（是/否） =====
        yield return Data(SysSeedIds.DictDataYesNoNo, SysSeedIds.DictTypeYesNo, "否", "0", "info", 1, true);
        yield return Data(SysSeedIds.DictDataYesNoYes, SysSeedIds.DictTypeYesNo, "是", "1", "success", 2);

        // ===== account_type（账号类型） =====
        yield return Data(SysSeedIds.DictDataAccountUser, SysSeedIds.DictTypeAccountType, "普通账号", "777", "default", 1, true);
        yield return Data(SysSeedIds.DictDataAccountSysAdmin, SysSeedIds.DictTypeAccountType, "系统管理员", "888", "primary", 2);
        yield return Data(SysSeedIds.DictDataAccountSuperAdmin, SysSeedIds.DictTypeAccountType, "超级管理员", "999", "danger", 3);

        // ===== org_type（机构类型） =====
        yield return Data(SysSeedIds.DictDataOrgGroup, SysSeedIds.DictTypeOrgType, "集团", "1", "danger", 1);
        yield return Data(SysSeedIds.DictDataOrgCompany, SysSeedIds.DictTypeOrgType, "公司", "2", "warning", 2);
        yield return Data(SysSeedIds.DictDataOrgDept, SysSeedIds.DictTypeOrgType, "部门", "3", "primary", 3, true);
        yield return Data(SysSeedIds.DictDataOrgTeam, SysSeedIds.DictTypeOrgType, "小组", "4", "info", 4);

        // ===== code_reset_cycle（与 CodeResetCycleEnum 双轨） =====
        yield return Data(SysSeedIds.DictDataResetNever, SysSeedIds.DictTypeCodeResetCycle, "不重置", "1", "info", 1);
        yield return Data(SysSeedIds.DictDataResetDaily, SysSeedIds.DictTypeCodeResetCycle, "按日重置", "2", "primary", 2, true);
        yield return Data(SysSeedIds.DictDataResetMonthly, SysSeedIds.DictTypeCodeResetCycle, "按月重置", "3", "success", 3);
        yield return Data(SysSeedIds.DictDataResetYearly, SysSeedIds.DictTypeCodeResetCycle, "按年重置", "4", "warning", 4);

        // ===== code_check_algo（与 CodeCheckAlgoEnum 双轨） =====
        yield return Data(SysSeedIds.DictDataCheckNone, SysSeedIds.DictTypeCodeCheckAlgo, "无", "1", "info", 1, true);
        yield return Data(SysSeedIds.DictDataCheckMod10, SysSeedIds.DictTypeCodeCheckAlgo, "Mod10", "2", "primary", 2);

        // ===== code_source_type（与 CodeSourceTypeEnum 双轨） =====
        yield return Data(SysSeedIds.DictDataSourceSelf, SysSeedIds.DictTypeCodeSourceType, "本厂生成", "1", "success", 1, true);
        yield return Data(SysSeedIds.DictDataSourceExternal, SysSeedIds.DictTypeCodeSourceType, "外部传入", "2", "warning", 2);

        // ===== mdm_material_unit（物料计量单位，运营可增项） =====
        yield return Data(SysSeedIds.DictDataUnitPcs, SysSeedIds.DictTypeMaterialUnit, "件", "PCS", "primary", 1, true);
        yield return Data(SysSeedIds.DictDataUnitKg, SysSeedIds.DictTypeMaterialUnit, "千克", "KG", "success", 2);
        yield return Data(SysSeedIds.DictDataUnitM, SysSeedIds.DictTypeMaterialUnit, "米", "M", "info", 3);
        yield return Data(SysSeedIds.DictDataUnitSet, SysSeedIds.DictTypeMaterialUnit, "套", "SET", "warning", 4);
        yield return Data(SysSeedIds.DictDataUnitL, SysSeedIds.DictTypeMaterialUnit, "升", "L", "default", 5);

        // ===== mdm_material_source（物料来源，勿与 code_source_type 混淆） =====
        yield return Data(SysSeedIds.DictDataMaterialSourceBuy, SysSeedIds.DictTypeMaterialSource, "外购", "Buy", "primary", 1, true);
        yield return Data(SysSeedIds.DictDataMaterialSourceMake, SysSeedIds.DictTypeMaterialSource, "自制", "Make", "success", 2);
        yield return Data(SysSeedIds.DictDataMaterialSourceOutsource, SysSeedIds.DictTypeMaterialSource, "委外", "Outsource", "warning", 3);
    }

    private static SysDictData Data(long id, long typeId, string label, string value, string tag, int order, bool isDefault = false)
    {
        return new SysDictData
        {
            Id = id,
            DictTypeId = typeId,
            Label = label,
            Value = value,
            TagStyle = tag,
            IsDefault = isDefault,
            OrderNo = order,
            Status = StatusEnum.Enable,
            CreatedBy = SysSeedIds.SeedCreatorId,
            Creator = SysSeedIds.SeedCreator,
            CreatedTime = SysSeedIds.SeedCreatedTime
        };
    }
}