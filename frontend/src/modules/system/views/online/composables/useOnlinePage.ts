import { h, ref } from 'vue'
import { NTag } from 'naive-ui'

import {
  forceLogoutSession,
  forceLogoutUser,
  getOnlineSessionPage,
  type OnlineSessionDto
} from '@/modules/system/api/online'
import { systemPermissionCodes } from '@/modules/system/permissions/codes'
import { formatDateTime } from '@/shared/components/axis-crud'
import type {
  AxisTableAction,
  AxisTableActionColumn,
  AxisTableColumn,
  AxisTableQuery
} from '@/shared/components/axis-table'
import { tryRequest } from '@/shared/http'
import { dialog, message } from '@/shared/utils/message'

export function useOnlinePage() {
  /* ============ 状态 & 引用 ============ */

  const tableRef = ref<AxisTableAction<OnlineSessionDto> | null>(null)
  const permissions = systemPermissionCodes.online

  /* ============ 表格配置 ============ */

  const columns: AxisTableColumn<OnlineSessionDto>[] = [
    {
      key: 'account',
      title: '账号',
      width: 160,
      search: { field: 'keyword', placeholder: '账号 / 姓名 / IP', primary: true },
      render: row => {
        if (!row.isCurrent) return row.account
        return h('span', { class: 'online-page__account' }, [
          row.account,
          ' ',
          h(
            NTag,
            { size: 'tiny', type: 'success', round: true, bordered: false },
            { default: () => '当前' }
          )
        ])
      }
    },
    {
      key: 'realName',
      title: '姓名',
      width: 120,
      render: row => row.realName || '-'
    },
    {
      key: 'clientIp',
      title: 'IP',
      width: 140,
      render: row => row.clientIp || '-'
    },
    {
      key: 'userAgent',
      title: '客户端',
      ellipsis: { tooltip: true },
      render: row => row.userAgent || '-'
    },
    {
      key: 'rememberMe',
      title: '记住登录',
      width: 100,
      render: row =>
        h(
          NTag,
          {
            size: 'tiny',
            type: row.rememberMe ? 'warning' : 'default',
            round: true,
            bordered: false
          },
          { default: () => (row.rememberMe ? '是' : '否') }
        )
    },
    {
      key: 'createdTime',
      title: '登录时间',
      width: 180,
      render: row => formatDateTime(row.createdTime)
    },
    {
      key: 'expiryDate',
      title: '会话过期',
      width: 180,
      render: row => formatDateTime(row.expiryDate)
    }
  ]

  const actionColumn: AxisTableActionColumn<OnlineSessionDto> = {
    width: 168,
    actions: [
      {
        key: 'kickSession',
        label: '下线会话',
        type: 'warning',
        permissions: [permissions.kick],
        onClick: row => handleKickSession(row)
      },
      {
        key: 'kickUser',
        label: '下线用户',
        type: 'error',
        permissions: [permissions.kick],
        show: row => !row.isCurrent,
        onClick: row => handleKickUser(row)
      }
    ]
  }

  /* ============ 交互操作 ============ */

  function request(params: AxisTableQuery) {
    return getOnlineSessionPage(params)
  }

  function handleKickSession(row: OnlineSessionDto) {
    dialog.warning({
      title: '强制下线会话',
      content: `确认强制下线账号「${row.account}」的该会话？`,
      positiveText: '下线',
      negativeText: '取消',
      onPositiveClick: async () => {
        const [, error] = await tryRequest(forceLogoutSession(row.id))
        if (error) return
        message.success('已强制下线该会话')
        tableRef.value?.reload()
      }
    })
  }

  function handleKickUser(row: OnlineSessionDto) {
    dialog.error({
      title: '强制下线用户',
      content: `将踢出账号「${row.account}」的全部在线会话，确认继续？`,
      positiveText: '全部下线',
      negativeText: '取消',
      onPositiveClick: async () => {
        const [, error] = await tryRequest(forceLogoutUser(row.userId))
        if (error) return
        message.success('已强制下线该用户全部会话')
        tableRef.value?.reload()
      }
    })
  }

  return {
    actionColumn,
    columns,
    permissions,
    request,
    tableRef
  }
}
