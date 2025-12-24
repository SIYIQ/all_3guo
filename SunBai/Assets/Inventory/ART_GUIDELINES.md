美术替换与 UI 交付指南（背包 / 格子 / 立绘）

目标
- 使美术可以仅通过替换 Sprite 资产和 UITheme（如有）来更新背包界面，无需改动代码。

资源与命名约定（示例）
- 图标尺寸（Item icons）: 64x64 或 128x128，PNG，透明背景，命名示例：`inv_icon_sword.png`
- 格子背景（Slot background，推荐 9-slice）: 128x128，命名示例：`inv_slot_bg.png`
- 格子框（Slot frame）: 128x128，命名示例：`inv_slot_frame.png`
- 空格占位图（empty）: 64x64，命名示例：`inv_empty.png`
- 面板背景（panel）: 1024x512 或更高，命名示例：`inv_panel_bg.png`
- 立绘（portrait）: 512x512 或 1024x1024，根据需求，命名示例：`portrait_default.png`

9-slice 指南（Slot panel / 背景）
- 使用 Unity Sprite Editor 设置边距（Border）来启用 9-slice。
- 推荐边距：上下左右各 12-18px（根据图像实际内边距微调）。
- 测试项目：在不同分辨率下把面板缩放到 0.5x/1x/2x，确保图像边角保真。

最佳实践
- 使用统一的 UITheme ScriptableObject（如果项目后来增加），包含：`slotBackground`, `slotFrame`, `emptyIcon`, `panelBackground`, `defaultPortrait`。
- 所有格子使用同一 `SlotPrefab`，由 `GridLayoutGroup` 控制位置与间距，避免手动定位。
- SlotPrefab 的根节点必须是 RectTransform，Pivot = (0.5,0.5)，Scale 默认 (1,1,1)。
- Icon Image 的 `Image Type` 使用 `Simple`，不要在运行时修改 `localScale` 或 `rect`。
- 使用透明 PNG，并确保 atlas/压缩设置不会破坏图像的像素（UI 图集采用 Sprite (2D and UI) import settings）。

Inspector 配置步骤（给美术）
1. 将新素材拖入 `Assets/Art/UI/Inventory/`。
2. 打开 `SlotPrefab`，将 `Icon` 子节点的 `Source Image` 替换为新的图标（测试多个图标）。
3. 在场景中选中 `InventoryRoot`（或 UI Root），打开 `InventoryUI` 组件：
   - 把 `slotPrefab` 指向 `SlotPrefab`（已包含 Icon/Image）。
   - 把 `emptySlotSprite` 指定为 `inv_empty.png`。
   - 在 `GridLayoutGroup` 中调节 `Cell Size` 使格子对齐（推荐同 128x128 或根据设计更改）。
4. 将 `portraitImage` 的 `Source Image` 设置为 `portrait_default.png`（若支持角色立绘，请让工程师把 bridge 接口指向角色的立绘 sprite）。

测试清单（给 QA / 美术）
- 在不同分辨率（800x600, 1280x720, 1920x1080）打开背包面板，检查格子是否对齐与缩放正常。
- 检查图标在格子内是否居中，不被裁切（RectTransform padding）。
- 检查 9-slice 背景在不同尺寸下角部与边缘是否正确显示。
- 检查 UI 在打开/关闭动画中过度是否平滑（若使用动画）。

常见问题与排查
- 格子叠在一起或不在同一平面：检查是否有多个 Canvas 或 SlotPrefab 根节点有非 1 缩放。确保所有 UI 都在同一 Canvas 且 `Canvas.sortingOrder` 合理。
- 图标模糊或被压缩：检查 Sprite 的 Import Mode（应为 Sprite (2D and UI)），Compression 设为 None 或 Low；Pixels Per Unit 一致（通常为 100）。
- 替换后尺寸不对：调整 `GridLayoutGroup.Cell Size` 或 SlotPrefab 的 RectTransform 大小以匹配新素材。

附：推荐交付包
- 最终贴图放入 `Assets/Art/UI/Inventory/`，并附带一个 `UITheme` 资产（若存在）。
- 提交说明文档（README）指明每张图的用途、推荐大小与 9-slice 边距。

如需，我可以把 `InventoryUI` 增强为读取 `UITheme` 的代码模板，或把当前 `SlotPrefab` 的推荐 RectTransform 设置写为更精确的数值样例。


