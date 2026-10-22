-- 清理 Demo / 主数据(MDM) 菜单及相关角色菜单（可选，已有库执行一次）
-- 说明：
-- 1. 代码种子仅在 EnableInitSeed 时空库生效；已有库中的 Demo/主数据菜单不会自动消失。
-- 2. 本脚本按 path / permission / name / component 匹配删除，并级联清理角色/租户菜单关联。
-- 3. 执行前请备份；若库表结构有差异请先核对。

BEGIN;

CREATE TEMP TABLE tmp_demo_mdm_menus AS
SELECT id
FROM sys_menu
WHERE
  path LIKE '/demo%'
  OR path LIKE '/mdm%'
  OR COALESCE(permission, '') LIKE 'demo:%'
  OR COALESCE(permission, '') LIKE 'mdm:%'
  OR COALESCE(component, '') LIKE 'modules/demo/%'
  OR COALESCE(component, '') LIKE 'modules/mdm/%'
  OR name IN ('Demo', '主数据')
  OR name LIKE 'Demo%';

DELETE FROM sys_role_menu
WHERE menu_id IN (SELECT id FROM tmp_demo_mdm_menus);

DELETE FROM sys_tenant_menu
WHERE menu_id IN (SELECT id FROM tmp_demo_mdm_menus);

DELETE FROM sys_menu
WHERE id IN (SELECT id FROM tmp_demo_mdm_menus);

DELETE FROM sys_job
WHERE job_code = 'demo:heartbeat'
   OR group_name = 'demo'
   OR name = 'Demo 心跳';

DELETE FROM sys_dict_data
WHERE dict_type_id IN (
  SELECT id FROM sys_dict_type
  WHERE code IN ('mdm_material_unit', 'mdm_material_source')
);

DELETE FROM sys_dict_type
WHERE code IN ('mdm_material_unit', 'mdm_material_source');

DELETE FROM sys_code_rule
WHERE code IN ('MAT', 'PRC', 'RTG');

DROP TABLE tmp_demo_mdm_menus;

COMMIT;
