<div align="center">

# 云枢 Axis

### 连接前后端，统一业务枢纽

<p>
  <img src="https://img.shields.io/badge/.NET-10-512bd4?style=flat-square" alt=".NET 10" />
  <img src="https://img.shields.io/badge/Vue-3.5-42b883?style=flat-square" alt="Vue 3.5" />
  <img src="https://img.shields.io/badge/FastEndpoints-8-111827?style=flat-square" alt="FastEndpoints" />
  <img src="https://img.shields.io/badge/SqlSugar-ORM-f97316?style=flat-square" alt="SqlSugar" />
  <img src="https://img.shields.io/badge/Naive%20UI-2-18a058?style=flat-square" alt="Naive UI" />
  <img src="https://img.shields.io/badge/License-MIT-10b981?style=flat-square" alt="MIT" />
</p>

<p>
  面向企业级中后台的前后端一体化开发框架<br/>
  统一分层规范、权限体系、接口约定与工程骨架，方便长期迭代
</p>

</div>

---

## 项目介绍

**云枢（Axis）** 是一套可落地的中后台开发骨架：后端按业务模块竖切，前端提供布局、权限、表格与请求约定，前后端共享菜单、权限码与接口规范。

适合管理后台、运营平台、内部业务系统等需要清晰结构与持续维护的项目。业务写法以 [业务开发规范](docs/业务开发规范.md) 为准。

---

## 数据中台 DataHub

本仓库同时承载 **云枢 DataHub** 控制面：在 Axis 分层与权限体系上扩展 `dh_*` 元数据（数据源 / 数据集 / 采集任务），PostgreSQL 仅作控制面库，不做数仓。架构说明见 [docs/数据中台架构.md](docs/数据中台架构.md)。

## 核心能力

### 前后端一体

菜单、路由、权限码与接口组织方式对齐，减少跨层联调成本。

### 清晰分层

`Host` / `Application` / `Domain` / `Domain.Data` / `Infrastructure` / `Infrastructure.Data`，禁止跨层抄近路。

### 企业级权限

菜单、按钮、接口权限码，配合数据权限与多租户。

### 开箱系统模块

用户、角色、机构、岗位、菜单、字典、配置、文件、通知公告、站内信等。

### 运维与集成

操作 / 登录 / 异常日志、在线用户、Quartz、缓存管理、SignalR 推送、OpenAPI 签名身份。

### 横切基础设施

JWT + Refresh、图形验证码、防重放、EasyCaching、乐观锁、邮件 / 短信发送端口、出参脱敏钩子。

---

## 技术栈

<table>
<tr>
<td width="50%" valign="top">

### 后端

- ASP.NET Core（.NET 10）
- FastEndpoints
- SqlSugar + PostgreSQL（默认）
- EasyCaching / Quartz / Serilog
- MailKit / Lazy.Captcha / Knife4j

</td>
<td width="50%" valign="top">

### 前端

- Vue 3.5 / TypeScript / Vite
- Pinia / Vue Router
- Naive UI / UnoCSS
- Axios / SignalR
- pnpm（详见 [前端说明](frontend/README.md)）

</td>
</tr>
</table>

---

## 仓库结构

```text
axis-net/
├── src/
│   ├── Axis.Host.Api              # 宿主、Endpoint、配置
│   ├── Axis.Application           # 用例编排
│   ├── Axis.Domain                # 实体 / 枚举 / 种子
│   ├── Axis.Domain.Data           # 仓储与缓存端口
│   ├── Axis.Infrastructure        # 框架横切
│   └── Axis.Infrastructure.Data   # 仓储与缓存实现
├── frontend/                      # Vue 管理端
└── docs/                          # 开发规范等
```

---

## 快速开始

### 1. 启动后端

按需修改 [`src/Axis.Host.Api/Config/`](src/Axis.Host.Api/Config/)，至少确认数据库连接 `08-Db.json`：

```bash
dotnet run --project src/Axis.Host.Api
```

### 2. 启动前端

```bash
cd frontend
pnpm install --frozen-lockfile
pnpm dev
```

环境变量与代理见 `frontend/.env*`。登录、验证码与系统管理页依赖后端接口。

更完整的前端说明、环境要求与页面预览见 [frontend/README.md](frontend/README.md)。

---

## 文档

- [业务开发规范](docs/业务开发规范.md) — 分层、命名、校验、FastEndpoints、SqlSugar、租户等强制约定
- [轻量模块脚手架](docs/脚手架.md) — 管理端「代码生成」或 CLI `axis new module`；支持读表/实体字段、权限合并、菜单种子
- [前端 README](frontend/README.md) — 前端能力、命令与预览截图

---

<div align="center">

**云枢 Axis** · 连接前后端，统一业务枢纽

MIT License

</div>
