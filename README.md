# 简账 LedgerDesk

> V1 — 离线、无广告、尊重隐私的 Windows 桌面记账软件。

[GitHub 仓库](https://github.com/DELTA-1873/LedgerDesk) · [V1.0.0](https://github.com/DELTA-1873/LedgerDesk/releases/tag/v1.0.0)

## 主要功能

- 收入、支出、借入、借出与投资中账目
- “支出 / 收入 / 资金”三个内置记账页面
- 支出分类图标选择，并用高亮边框、反色图标和“✓ 已选择”标识当前项目
- 卡片式明细账本与可单独打开的账目详情
- 自绘圆角按钮、输入控件、日期选择器和无系统边框窗口
- 绿色、淡红色、白色、米色、蓝色五套可持久化界面风格
- 最近 36 个月可选的单月消费环状图及切换动画
- 最近 6 个月、最近 5 年收入/支出横向对比图
- JSON 备份与恢复、CSV 导出

## 显示与高 DPI

程序声明 Windows Per-Monitor V2 DPI 感知，可在不同缩放比例的显示器之间移动。窗口启动时按主屏幕可用工作区自适应尺寸，并启用 ClearType、Display 文本排版、布局像素对齐和高质量图像缩放，改善 2K/4K 屏幕上的字体清晰度。

## 数据可靠性

账目保存在 `%LOCALAPPDATA%\简账\ledger.json`，主题设置保存在同目录的 `appearance.json`，不会主动上传。

每次新增、编辑、删除或恢复数据后，总览卡片、明细列表和统计图会使用同一份内存数据即时刷新。写入时先完成 JSON 校验，再以临时文件替换主文件，并保留 `ledger.previous.json`。

## 启动 V1

本机程序：`C:\Users\DELTA\LedgerDesk\LedgerDesk-V1\LedgerDesk.exe`

也可以双击：`C:\Users\DELTA\LedgerDesk\启动简账V1.cmd`

## 从源码构建

需要 .NET 8 SDK 与 Windows Desktop Runtime：

```powershell
dotnet build src/LedgerDesk/LedgerDesk.csproj -c Release
```

## 最新更新

- 增加 Per-Monitor V2 高 DPI 适配和主屏幕工作区尺寸适配
- 增加五套主题，选择后即时应用并在下次启动恢复
- 支出和收入分类增加明确的当前选择标识
- 删除确认改为无边框圆角自定义窗口
- 修复账目变化后统计图不同步的问题
- 增加月度/年度横向对比图与 36 个月环状图
- Release 构建：0 个错误、0 个警告
- 本机 EXE/DLL 哈希核对一致，启动检查无运行错误

## 技术实现

- .NET 8 / WPF
- Windows Per-Monitor V2 DPI awareness
- 原生矢量绘图与依赖属性动画
- System.Text.Json 本地持久化
- Lucide Icons（ISC License）

## 许可

项目图标来自 [Lucide Icons](https://github.com/lucide-icons/lucide)，遵循其 ISC 许可。
