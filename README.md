# 简账 LedgerDesk

> V1 — 离线、无广告、尊重隐私的 Windows 桌面记账软件。

[GitHub 仓库](https://github.com/DELTA-1873/LedgerDesk)

## V1 功能

- 收入、支出、借入、借出与投资账目
- “支出 / 收入 / 资金”三个内置记账页面
- 支出分类图标快捷选择
- 卡片式明细账本与单条详情窗口
- 类型下拉筛选、收支汇总与未结资金统计
- JSON 备份与恢复、CSV 导出
- 无系统边框窗口、自绘圆角按钮、输入控件与日期选择器
- 最近 24 个月可选的单月消费环状图
- 月份切换环状图动画
- 近 6 个月收入与支出柱状图

## 启动 V1

本机版本可运行 `LedgerDesk-V1/LedgerDesk.exe`。从源码构建后，可执行文件位于 `src/LedgerDesk/bin/Release/net8.0-windows/`。

## 数据与隐私

账目保存在 `%LOCALAPPDATA%\简账\ledger.json`。软件不会主动上传账目数据。

## 从源码构建

需要 .NET 8 SDK 与 Windows Desktop Runtime：

```powershell
dotnet build src/LedgerDesk/LedgerDesk.csproj -c Release
```

## 技术实现

- .NET 8 / WPF
- 原生矢量绘图与依赖属性动画
- System.Text.Json 本地持久化
- Lucide Icons（ISC License）

## V1 状态

- [x] V1 功能与界面完成
- [x] 独立源码零错误、零警告构建
- [x] 旧实验版本清理完成
- [x] GitHub 仓库创建完成
- [x] GitHub CLI 登录完成
- [x] V1 首次提交已准备

## 更新日志

### V1

- 完成基础账目、借贷与投资管理。
- 完成三页面快速记账和图标分类选择。
- 完成卡片式明细、单条详情和数据备份。
- 完成月份可选的动画消费环状图与月度柱状图。
- 清理旧实验版本，正式将本轮定为 V1。

## 许可证

项目图标来自 [Lucide Icons](https://github.com/lucide-icons/lucide)，遵循其 ISC/MIT 许可。
