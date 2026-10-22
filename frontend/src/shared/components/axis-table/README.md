# AxisTable 使用约定

## 推荐的搜索写法

搜索条件优先写在列配置里。普通输入框使用 `true`，存在 `dict` 时会自动生成下拉框，日期和自定义控件可以使用字符串简写。

```ts
const columns: AxisTableColumn<User>[] = [
  {
    key: 'account',
    title: '账号',
    search: {
      field: 'keyword',
      placeholder: '账号 / 姓名',
      primary: true
    }
  },
  {
    key: 'status',
    title: '状态',
    search: { primary: true },
    dict: statusOptions
  },
  {
    key: 'department',
    title: '部门',
    search: { advanced: true },
    dict: departmentOptions
  },
  {
    key: 'createdAt',
    title: '创建日期',
    search: {
      component: 'dateRange',
      advanced: true,
      span: 2
    }
  }
]
```

只有不属于表格列的搜索条件，才建议单独使用 `searchSchemas`。字段级自定义控件使用 `#search-字段名` 插槽。

## 自适应搜索布局

搜索区根据 AxisTable 自身的可用宽度计算列数，页面不需要维护媒体查询，也不需要计算“一行放几个”。查询、重置和高级筛选按钮位于独立操作区，不占用搜索列。

```vue
<AxisTable
  :columns="columns"
  :request="getUserPage"
  :search-config="{
    minItemWidth: 220,
    maxColumns: 4,
    collapsedRows: 1,
    labelWidth: 'auto'
  }"
/>
```

配置含义：

- `minItemWidth`：单个搜索项允许的最小宽度，默认 `220`。
- `maxColumns`：容器足够宽时最多展示的列数，默认 `4`。
- `collapsedRows`：收起状态最多展示的行数，默认 `1`。
- `labelWidth`：统一标签宽度；数字按 px 处理，默认 `auto`。

旧的 `searchCols`、`searchCollapsedRows` 仍然兼容，但新页面建议统一改用 `searchConfig`。

## 常用与高级筛选

- `primary: true`：常用条件，收起时优先保留；即使超过一行容量也不会被截断。
- `advanced: true`：高级条件，固定放入点击“高级筛选”后出现的下拉浮层。
- 未标记的条件：按照 `order` 和声明顺序填充收起区域的剩余位置。
- 常用区域放不下的普通条件也会自动进入高级筛选，不需要页面手动计算数量。
- 高级条件生效时，“高级筛选”按钮会显示生效数量；浮层底部提供“恢复默认”和“查询”。

同一个条件不要同时声明 `primary` 和 `advanced`。

## 特殊字段跨度

大多数搜索项不需要配置宽度。日期范围、自定义组合控件等特殊字段可以跨列：

```ts
{
  field: 'createdAtRange',
  label: '创建时间',
  component: 'dateRange',
  span: 2
}
```

使用 `fullWidth: true` 可以让字段独占整行。原有 `width` 仍可用于控制字段内部控件宽度，但优先推荐 `span`，避免固定像素宽度破坏响应式布局。

## 远程数据

`request` 会收到分页参数和当前搜索参数。组件会保留最近一次搜索条件，翻页、切换每页数量和刷新时不会丢失。

```vue
<AxisTable :columns="columns" :request="getUserPage" />
```

## 静态数据

静态数据无法推断 `keyword` 等业务字段的匹配规则，应通过 `localFilter` 明确提供本地过滤逻辑。

```vue
<AxisTable
  :columns="columns"
  :data="users"
  :local-filter="(rows, params) => filterUsers(rows, params)"
/>
```
