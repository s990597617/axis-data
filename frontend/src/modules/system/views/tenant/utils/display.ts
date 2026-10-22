// 租户隔离类型（与后端 TenantType 对齐；Schema=2 已停用，不再提供选择）
export const tenantTypeOptions = [
  { label: '字段隔离', value: 1 },
  { label: '独立数据库', value: 3 }
]
