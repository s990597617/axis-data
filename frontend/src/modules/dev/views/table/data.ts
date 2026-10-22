import type { AxisTableRow } from '@/shared/components/axis-table'

export interface DemoUser extends AxisTableRow {
  id: number
  username: string
  nickname: string
  status: number
  role: string
  department: string
  createdAt: string
  score: number
}

export const statusOptions = [
  { label: '启用', value: 1 },
  { label: '禁用', value: 0 },
  { label: '待审核', value: 2 }
]

export const roleOptions = [
  { label: '管理员', value: 'admin' },
  { label: '运营', value: 'operator' },
  { label: '访客', value: 'guest' }
]

export const departmentOptions = [
  { label: '平台研发部', value: '平台研发部' },
  { label: '业务运营部', value: '业务运营部' },
  { label: '数据治理部', value: '数据治理部' }
]

export const demoUsers: DemoUser[] = [
  {
    id: 1001,
    username: 'axis_admin',
    nickname: '系统管理员',
    status: 1,
    role: 'admin',
    department: '平台研发部',
    createdAt: '2026-01-08',
    score: 98
  },
  {
    id: 1002,
    username: 'miao_operator',
    nickname: '运营专员',
    status: 1,
    role: 'operator',
    department: '业务运营部',
    createdAt: '2026-01-12',
    score: 86
  },
  {
    id: 1003,
    username: 'data_owner',
    nickname: '数据负责人',
    status: 2,
    role: 'operator',
    department: '数据治理部',
    createdAt: '2026-02-03',
    score: 91
  },
  {
    id: 1004,
    username: 'guest_readonly',
    nickname: '只读用户',
    status: 0,
    role: 'guest',
    department: '业务运营部',
    createdAt: '2026-02-18',
    score: 62
  },
  {
    id: 1005,
    username: 'dev_leader',
    nickname: '研发负责人',
    status: 1,
    role: 'admin',
    department: '平台研发部',
    createdAt: '2026-03-06',
    score: 95
  },
  {
    id: 1006,
    username: 'data_viewer',
    nickname: '数据观察员',
    status: 2,
    role: 'guest',
    department: '数据治理部',
    createdAt: '2026-03-20',
    score: 73
  }
]

export function filterDemoUsers(params: Record<string, unknown>) {
  const keyword = String(params.keyword ?? '').trim().toLowerCase()
  const status = params.status
  const role = params.role
  const department = params.department

  return demoUsers.filter(item => {
    const hitKeyword =
      !keyword ||
      item.username.toLowerCase().includes(keyword) ||
      item.nickname.toLowerCase().includes(keyword)
    const hitStatus = status === undefined || status === null || item.status === Number(status)
    const hitRole = !role || item.role === role
    const hitDepartment = !department || item.department === department

    return hitKeyword && hitStatus && hitRole && hitDepartment
  })
}
