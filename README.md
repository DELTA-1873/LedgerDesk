# 简账 LedgerDesk

> V1 — 离线、无广告、尊重隐私的 Windows 桌面记账软件。

[GitHub 仓库](https://github.com/DELTA-1873/LedgerDesk) · [V1.0.0](https://github.com/DELTA-1873/LedgerDesk/releases/tag/v1.0.0)

## 主要功能

- 收入、支出、借入、借出与投资中账目
- “支出 / 收入 / 资金”三个内置记账页面，支出分类支持图标选择
- 卡片式明细账本与可单独打开的账目详情
- 自绘圆角按钮、输入控件、日期选择器和无系统边框窗口
- 可选择最近 36 个月的单月消费环状图
- 月份切换时从环形左上区域展开的绘制动画
- 最近 6 个月收入/支出横向对比图
- 最近 5 年收入/支出横向对比图，可在月度和年度模式间切换
- JSON 备份与恢复、CSV 导出

## 数据可靠性

账目保存在 `%LOCALAPPDATA%\简账\ledger.json`，不会主动上传。

每次新增、编辑、删除或恢复数据后，总览卡片、明细列表和统计图会使用同一份内存数据即时刷新。写入时先完成 JSON 序列化校验，再写入临时文件，并在替换主文件前保留 `ledger.previous.json`。如果主文件无法读取，程序会尝试载入上一份有效数据。

## 启动 V1

本机版本：`C:\Users\DELTA\LedgerDesk\LedgerDesk-V1\LedgerDesk.exe`

也可以双击：`C:\Users\DELTA\LedgerDesk\启动简账V1.cmd`

## 从源码构建

需要 .NET 8 SDK 与 Windows Desktop Runtime：

```powershell
dotnet build src/LedgerDesk/LedgerDesk.csproj -c Release
```

## 本轮更新

- 删除确认改为无边框、圆角、自定义危险操作提示窗
- 修复统计图只在启动时读取一次数据、增删改后不同步的问题
- 增加写入校验、临时文件替换、上一版本回退机制
- 环状图月份范围扩展到最近 36 个月，并保留切换动画
- 横向柱状图增加月度/年度两种对比模式
- 已通过 Release 构建：0 个错误、0 个警告
- 已完成本机启动检查，未发现 .NET 运行错误

## 技术实现

- .NET 8 / WPF
- 原生矢量绘图与依赖属性动画
- System.Text.Json 本地持久化
- Lucide Icons（ISC License）

## 许可

项目图标来自 [Lucide Icons](https://github.com/lucide-icons/lucide)，遵循其 ISC 许可。
