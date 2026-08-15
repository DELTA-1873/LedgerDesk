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

程序使用 Windows Per-Monitor V2 DPI 感知，可在不同缩放比例的显示器之间移动。窗口启动时按主屏幕工作区自适应尺寸，并启用 ClearType、Display 文本排版、布局像素对齐和高质量图像缩放。

## 数据可靠性

账目保存在 `%LOCALAPPDATA%\简账\ledger.json`，主题设置保存在同目录的 `appearance.json`，不会主动上传。

新增、编辑、删除或恢复数据后，总览、明细列表和统计图使用同一份数据即时刷新。写入时先完成 JSON 校验，再以临时文件替换主文件，并保留 `ledger.previous.json`。

## 启动 V1

本机程序：`C:\Users\DELTA\Documents\LedgerDesk\LedgerDesk-V1\LedgerDesk.exe`

也可以双击：`C:\Users\DELTA\Documents\LedgerDesk\启动简账V1.cmd`

## 从源码构建

需要 .NET 8 SDK 与 Windows Desktop Runtime：

```powershell
dotnet build src/LedgerDesk/LedgerDesk.csproj -c Release
```

## 分类消费统计

- 每笔支出可标记为“生活消费”或“大额支出”，并随账目保存。
- 环状图支持生活消费、大额支出和全部支出三种范围。
- 月度/年度横向图分别显示收入、生活消费和大额支出。
- 旧账目未标记时以 5000 元为兼容分界，之后可通过编辑账目手动调整。

## 首屏统计图

- 统计图已改为主窗口显示前完成挂载，应用启动首帧即可看到环状图和横向对比图。
- 启动顺序为读取账本、生成聚合缓存、创建图表、显示窗口，不再等待窗口激活事件。

## 性能优化

- 统计图改为账目更新时一次预聚合，绘制和动画帧不再重复扫描全部账目。
- 首次启动直接显示完整环状图，不播放入场动画；用户切换月份时仍保留动画。

## 最新更新

- 项目本地目录已迁移到 `C:\Users\DELTA\Documents\LedgerDesk`，启动脚本继续使用相对路径。

- 修复首次打开明细账本时筛选统计显示“—”、已有账目未立即出现的问题
- 窗口加载完成时自动填充全部账目，每次进入明细页时再次同步当前筛选
- 增加 Per-Monitor V2 高 DPI 与主屏幕工作区适配
- 增加绿色、淡红色、白色、米色、蓝色五套主题
- 支出和收入分类增加明确的当前选择标识
- 修复账目变化后统计图不同步的问题
- 增加月度/年度横向对比图与 36 个月环状图
- Release 构建：0 个错误、0 个警告；本机启动检查正常

## 技术实现

- .NET 8 / WPF
- Windows Per-Monitor V2 DPI awareness
- 原生矢量绘图与依赖属性动画
- System.Text.Json 本地持久化
- Lucide Icons（ISC License）

## 许可

项目图标来自 [Lucide Icons](https://github.com/lucide-icons/lucide)，遵循其 ISC 许可。
