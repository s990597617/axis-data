<script setup lang="ts">
  import { NButton, NEmpty, NProgress, NSpin, NSwitch } from 'naive-ui'

  import { Icon } from '@/shared/icons'

  import MonitorGauge from './components/MonitorGauge.vue'
  import MonitorSparkline from './components/MonitorSparkline.vue'
  import { useMonitorPage } from './composables/useMonitorPage'
  import { healthTone } from './utils/format'

  defineOptions({ name: 'SysMonitor' })

  /* ============ 状态 & 引用 ============ */

  const {
    autoRefresh,
    collectedAtText,
    cpuHistory,
    data,
    diskSummary,
    formatBytes,
    health,
    hostCards,
    ipList,
    load,
    loading,
    memoryHistory,
    memoryUsage,
    overviewStats,
    primaryCpu,
    processCpu,
    refreshing,
    runtimeCards,
    systemCpu,
    toggleAutoRefresh,
    usageColor,
    usageStatus
  } = useMonitorPage()
</script>

<template>
  <div class="monitor-page">
    <section class="monitor-page__block">
      <div class="monitor-page__toolbar">
        <div class="monitor-page__meta">
          <div class="monitor-page__title-row">
            <h2 class="monitor-page__title">服务监控</h2>
            <span class="monitor-page__badge" :class="`monitor-page__badge--${health.tone}`">
              {{ health.label }}
            </span>
          </div>
          <span class="monitor-page__time">最近采集：{{ collectedAtText }} · 每 5 秒自动刷新</span>
        </div>
        <div class="monitor-page__actions">
          <n-switch :value="autoRefresh" @update:value="toggleAutoRefresh">
            <template #checked>自动刷新</template>
            <template #unchecked>自动刷新</template>
          </n-switch>
          <n-button :loading="refreshing || loading" @click="load(true)">
            <template #icon>
              <Icon icon="ant-design:reload-outlined" />
            </template>
            刷新
          </n-button>
        </div>
      </div>

      <n-spin :show="loading && !data">
        <template v-if="data">
          <div class="monitor-page__hero">
            <div class="monitor-page__hero-main">
              <div class="monitor-page__host">
                <div class="monitor-page__host-name">{{ data.os.machineName }}</div>
                <div class="monitor-page__host-os">{{ data.os.osDescription }}</div>
                <div class="monitor-page__chips">
                  <span class="monitor-page__chip">
                    <Icon icon="ant-design:cloud-server-outlined" />
                    {{ data.runtime.environmentName || '-' }}
                  </span>
                  <span class="monitor-page__chip">
                    <Icon icon="ant-design:code-outlined" />
                    {{ data.runtime.frameworkDescription }}
                  </span>
                  <span
                    v-for="ip in ipList"
                    :key="ip"
                    class="monitor-page__chip"
                  >
                    <Icon icon="ant-design:global-outlined" />
                    {{ ip }}
                  </span>
                </div>
              </div>
            </div>

            <div class="monitor-page__hero-stats">
              <div v-for="item in overviewStats" :key="item.key" class="monitor-page__stat">
                <span class="monitor-page__stat-label">
                  <Icon :icon="item.icon" />
                  {{ item.label }}
                </span>
                <span class="monitor-page__stat-value">{{ item.value }}</span>
              </div>
            </div>
          </div>

          <div class="monitor-page__metrics">
            <section class="monitor-page__panel">
              <div class="monitor-page__panel-head">
                <span class="monitor-page__panel-title">CPU</span>
                <span class="monitor-page__panel-extra">
                  {{ data.cpu.processorCount }} 逻辑核心
                </span>
              </div>
              <div class="monitor-page__metric-body">
                <MonitorGauge
                  :value="primaryCpu"
                  label="当前占用"
                  :sublabel="
                    systemCpu == null
                      ? `进程 ${processCpu.toFixed(1)}%`
                      : `系统 ${systemCpu.toFixed(1)}% · 进程 ${processCpu.toFixed(1)}%`
                  "
                />
                <div class="monitor-page__metric-side">
                  <MonitorSparkline :values="cpuHistory" :color="usageColor(primaryCpu)" />
                  <div class="monitor-page__kv">
                    <span class="monitor-page__k">系统 CPU</span>
                    <span class="monitor-page__v">
                      {{ systemCpu == null ? '暂不可用' : `${systemCpu.toFixed(1)}%` }}
                    </span>
                  </div>
                  <div class="monitor-page__kv">
                    <span class="monitor-page__k">进程 CPU</span>
                    <span class="monitor-page__v">{{ processCpu.toFixed(1) }}%</span>
                  </div>
                </div>
              </div>
            </section>

            <section class="monitor-page__panel">
              <div class="monitor-page__panel-head">
                <span class="monitor-page__panel-title">内存</span>
                <span class="monitor-page__panel-extra">
                  {{ formatBytes(data.memory.usedBytes) }} /
                  {{ formatBytes(data.memory.totalBytes) }}
                </span>
              </div>
              <div class="monitor-page__metric-body">
                <MonitorGauge
                  :value="memoryUsage"
                  label="系统内存"
                  :sublabel="`工作集 ${formatBytes(data.memory.processWorkingSetBytes)}`"
                />
                <div class="monitor-page__metric-side">
                  <MonitorSparkline :values="memoryHistory" :color="usageColor(memoryUsage)" />
                  <div class="monitor-page__kv">
                    <span class="monitor-page__k">可用</span>
                    <span class="monitor-page__v">{{ formatBytes(data.memory.availableBytes) }}</span>
                  </div>
                  <div class="monitor-page__kv">
                    <span class="monitor-page__k">托管堆</span>
                    <span class="monitor-page__v">{{ formatBytes(data.memory.managedHeapBytes) }}</span>
                  </div>
                </div>
              </div>
            </section>

            <section class="monitor-page__panel">
              <div class="monitor-page__panel-head">
                <span class="monitor-page__panel-title">磁盘</span>
                <span class="monitor-page__panel-extra">
                  <template v-if="diskSummary">
                    {{ diskSummary.count }} 卷 · 总占用 {{ diskSummary.usage.toFixed(1) }}%
                  </template>
                  <template v-else>无可用卷</template>
                </span>
              </div>
              <div v-if="data.disks.length" class="monitor-page__disks">
                <div v-for="disk in data.disks" :key="disk.name" class="monitor-page__disk">
                  <div class="monitor-page__disk-head">
                    <span class="monitor-page__disk-name">
                      {{ disk.name }}
                      <template v-if="disk.label"> · {{ disk.label }}</template>
                    </span>
                    <span
                      class="monitor-page__disk-usage"
                      :class="`monitor-page__disk-usage--${healthTone(disk.usage)}`"
                    >
                      {{ disk.usage.toFixed(1) }}%
                    </span>
                  </div>
                  <n-progress
                    type="line"
                    :percentage="Number(disk.usage.toFixed(1))"
                    :status="usageStatus(disk.usage)"
                    :color="usageColor(disk.usage)"
                    :height="8"
                    :border-radius="999"
                    :show-indicator="false"
                  />
                  <div class="monitor-page__disk-meta">
                    {{ formatBytes(disk.usedBytes) }} / {{ formatBytes(disk.totalBytes) }} ·
                    {{ disk.driveFormat }} · 可用 {{ formatBytes(disk.freeBytes) }}
                  </div>
                </div>
              </div>
              <n-empty v-else description="未检测到可用磁盘" size="small" />
            </section>
          </div>
        </template>
        <n-empty v-else-if="!loading" description="暂无监控数据" />
      </n-spin>
    </section>

    <section v-if="data" class="monitor-page__block">
      <div class="monitor-page__sections">
        <div>
          <div class="monitor-page__panel-head">
            <span class="monitor-page__panel-title">运行时</span>
          </div>
          <div class="monitor-page__info-grid">
            <div v-for="item in runtimeCards" :key="item.label" class="monitor-page__info-item">
              <span class="monitor-page__info-icon">
                <Icon :icon="item.icon" />
              </span>
              <div class="monitor-page__info-body">
                <span class="monitor-page__info-label">{{ item.label }}</span>
                <span class="monitor-page__info-value">{{ item.value }}</span>
              </div>
            </div>
          </div>
        </div>

        <div>
          <div class="monitor-page__panel-head">
            <span class="monitor-page__panel-title">主机信息</span>
          </div>
          <div class="monitor-page__info-grid">
            <div v-for="item in hostCards" :key="item.label" class="monitor-page__info-item">
              <span class="monitor-page__info-icon">
                <Icon :icon="item.icon" />
              </span>
              <div class="monitor-page__info-body">
                <span class="monitor-page__info-label">{{ item.label }}</span>
                <span class="monitor-page__info-value">{{ item.value }}</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
  </div>
</template>

<style scoped lang="scss">
  @use './styles/index.scss';
</style>
