import { ref } from 'vue'

import {
  createNotice,
  deleteNotice,
  getNoticePage,
  publishNotice,
  updateNotice,
  type NoticeDto,
  type SaveNoticeInput
} from '@/modules/system/api/notice'
import { systemPermissionCodes } from '@/modules/system/permissions/codes'
import {
  AxisCrudPage,
  formatDateTime,
  renderStatus,
  statusOptions,
  type AxisCrudResource
} from '@/shared/components/axis-crud'
import type { AxisTableColumn } from '@/shared/components/axis-table'
import { tryRequest } from '@/shared/http'
import { message } from '@/shared/utils/message'

import { noticeTargetTypeOptions, noticeTypeOptions } from '../utils/display'

export function useNoticePage() {
  /* ============ 常量 ============ */

  // 与后端 StatusEnum.Enable 对齐；启用即已发布
  const statusEnable = 1
  const pageRef = ref<InstanceType<typeof AxisCrudPage> | null>(null)

  /* ============ 列表列 ============ */

  const columns: AxisTableColumn<NoticeDto>[] = [
    {
      key: 'title',
      title: '标题',
      search: { field: 'keyword', placeholder: '标题或内容' }
    },
    {
      key: 'noticeType',
      title: '类型',
      width: 90,
      dict: noticeTypeOptions,
      search: true,
      dictRender: true
    },
    {
      key: 'targetType',
      title: '范围',
      width: 100,
      dict: noticeTargetTypeOptions,
      dictRender: true
    },
    {
      key: 'status',
      title: '状态',
      width: 90,
      dict: statusOptions,
      search: true,
      render: row => renderStatus(row.status)
    },
    {
      key: 'publishTime',
      title: '发布时间',
      width: 170,
      render: row => formatDateTime(row.publishTime)
    },
    {
      key: 'createdTime',
      title: '创建时间',
      width: 170,
      render: row => formatDateTime(row.createdTime)
    }
  ]

  /* ============ 交互操作 ============ */

  // 发布 / 下架成功后刷新列表状态
  async function handlePublish(row: NoticeDto, published: boolean) {
    const [, error] = await tryRequest(publishNotice(row.id, published))
    if (error) return
    message.success(published ? '已发布' : '已下架')
    await pageRef.value?.reload()
  }

  /* ============ 资源描述 ============ */

  const resource: AxisCrudResource<NoticeDto> = {
    title: '',
    entityName: '公告',
    permissions: systemPermissionCodes.notice,
    columns,
    formSubtitle: ({ editing, model }) =>
      editing
        ? `编辑「${String(model.title || '-')}」· 调整内容与发布范围`
        : '创建通知或公告，启用即发布并推送',
    actionColumn: { width: 200, maxInline: 3 },
    extraActions: [
      {
        key: 'publish',
        label: '发布',
        icon: 'ant-design:vertical-align-top-outlined',
        type: 'success',
        permissions: [systemPermissionCodes.notice.publish],
        ifShow: row => (row as NoticeDto).status !== statusEnable,
        confirm: row => `确定发布「${(row as NoticeDto).title}」？`,
        onClick: async row => {
          await handlePublish(row as NoticeDto, true)
        }
      },
      {
        key: 'unpublish',
        label: '下架',
        icon: 'ant-design:vertical-align-bottom-outlined',
        type: 'error',
        permissions: [systemPermissionCodes.notice.publish],
        ifShow: row => (row as NoticeDto).status === statusEnable,
        confirm: row => `确定下架「${(row as NoticeDto).title}」？`,
        onClick: async row => {
          await handlePublish(row as NoticeDto, false)
        }
      }
    ],
    formSections: [
      {
        key: 'basic',
        title: '基础信息',
        icon: 'ant-design:notification-outlined',
        columns: 2,
        fields: [
          {
            field: 'title',
            label: '标题',
            required: true,
            defaultValue: '',
            componentProps: { maxlength: 128, placeholder: '请输入标题' }
          },
          {
            field: 'noticeType',
            label: '类型',
            component: 'radio',
            options: noticeTypeOptions,
            required: true,
            defaultValue: 1
          },
          {
            field: 'targetType',
            label: '发布范围',
            component: 'select',
            options: noticeTargetTypeOptions,
            required: true,
            defaultValue: 0
          },
          {
            field: 'targetIds',
            label: '目标Ids',
            help: 'JSON 数组，如 [1,2,3]；全部用户可留空',
            defaultValue: '',
            ifShow: model => Number(model.targetType) !== 0,
            componentProps: { maxlength: 2000, placeholder: '[角色/机构/用户Id]' }
          },
          {
            field: 'status',
            label: '状态',
            component: 'radio',
            options: statusOptions,
            required: true,
            defaultValue: 0,
            help: '启用=立即发布并推送'
          },
          {
            field: 'orderNo',
            label: '排序',
            component: 'inputNumber',
            required: true,
            defaultValue: 100,
            componentProps: { min: 0, max: 9999 }
          },
          {
            field: 'content',
            label: '内容',
            component: 'textarea',
            fullWidth: true,
            required: true,
            defaultValue: '',
            componentProps: { rows: 6, placeholder: '请输入内容' }
          },
          {
            field: 'remark',
            label: '备注',
            component: 'textarea',
            fullWidth: true,
            defaultValue: '',
            componentProps: { maxlength: 256, rows: 2 }
          }
        ]
      }
    ],
    request: getNoticePage,
    create: data => createNotice(data as unknown as SaveNoticeInput),
    update: (id, data) => updateNotice(id, data as unknown as SaveNoticeInput),
    remove: deleteNotice
  }

  /* ============ 对外暴露 ============ */

  return { resource, pageRef }
}
