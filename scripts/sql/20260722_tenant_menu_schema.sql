-- 租户菜单 / 独立库开通：补齐已有库缺失字段与表（PostgreSQL，可重复执行）
-- 对应实体：SysMenu.IsPlatform、SysTenant.ProvisionStatus/ProvisionMessage、SysTenantMenu、SysUser.Account 唯一索引

ALTER TABLE sys_menu
    ADD COLUMN IF NOT EXISTS is_platform boolean NOT NULL DEFAULT false;
COMMENT ON COLUMN sys_menu.is_platform IS '是否平台专属';

-- 平台专属：租户管理菜单及按钮（与 SysMenuSeed 一致）
UPDATE sys_menu
SET is_platform = true
WHERE id IN (
              10000006000011080, -- MenuTenant
              10000006000011081, -- ButtonTenantQuery
              10000006000011082,
              10000006000011083,
              10000006000011084,
              10000006000011085  -- ButtonTenantGrant
    )
   OR permission LIKE 'sys:tenant%';

ALTER TABLE sys_tenant
    ADD COLUMN IF NOT EXISTS provision_status integer NOT NULL DEFAULT 1;
COMMENT ON COLUMN sys_tenant.provision_status IS '开通状态';

ALTER TABLE sys_tenant
    ADD COLUMN IF NOT EXISTS provision_message character varying(512) NULL;
COMMENT ON COLUMN sys_tenant.provision_message IS '开通失败原因';

CREATE TABLE IF NOT EXISTS sys_tenant_menu
(
    id           bigint                   NOT NULL,
    tenant_id    bigint                   NOT NULL,
    menu_id      bigint                   NOT NULL,
    CONSTRAINT pk_sys_tenant_menu PRIMARY KEY (id)
);
COMMENT ON TABLE sys_tenant_menu IS '租户菜单关系';
COMMENT ON COLUMN sys_tenant_menu.tenant_id IS '租户Id';
COMMENT ON COLUMN sys_tenant_menu.menu_id IS '菜单Id';

CREATE UNIQUE INDEX IF NOT EXISTS uk_sys_tenant_menu_tenant_menu
    ON sys_tenant_menu (tenant_id, menu_id);
CREATE INDEX IF NOT EXISTS index_sys_tenant_menu_tenant ON sys_tenant_menu (tenant_id);
CREATE INDEX IF NOT EXISTS index_sys_tenant_menu_menu ON sys_tenant_menu (menu_id);

-- 账号全局唯一（若不存在同名重复数据）
DO
$$
    BEGIN
        IF NOT EXISTS (SELECT 1
                       FROM pg_indexes
                       WHERE schemaname = current_schema()
                         AND indexname = 'uk_sys_user_account') THEN
            CREATE UNIQUE INDEX uk_sys_user_account ON sys_user (account);
        END IF;
    END
$$;

-- 默认租户：补齐非平台菜单启用（避免已有环境菜单变空）
INSERT INTO sys_tenant_menu (id, tenant_id, menu_id)
SELECT (10000011000000000 + m.id), 10000001000000001, m.id
FROM sys_menu m
WHERE COALESCE(m.is_platform, false) = false
  AND NOT EXISTS (SELECT 1
                  FROM sys_tenant_menu tm
                  WHERE tm.tenant_id = 10000001000000001
                    AND tm.menu_id = m.id);
