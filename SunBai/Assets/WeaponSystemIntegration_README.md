# 背包系统与武器系统集成说明

## 概述

本项目成功实现了 SunBai 背包系统与 DuanYiBo 武器系统的无缝集成。通过 WeaponSystemBridge 桥接器，背包中的武器物品现在可以直接装备到玩家角色上，并实时更新武器状态。

## 主要组件

### 1. WeaponSystemBridge.cs
- **位置**: `SunBai/Assets/Scripts/Inventory/WeaponSystemBridge.cs`
- **功能**: 桥接 SunBai 背包系统与 DuanYiBo 武器系统
- **方法**:
  - `EquipWeapon(WeaponData data)`: 装备武器
  - `UnequipWeapon()`: 卸载武器
  - `GetCurrentWeapon()`: 获取当前装备的武器
  - `GetCurrentAttackPower()`: 获取当前攻击力
  - `GetCurrentAttackRange()`: 获取当前攻击范围
  - `GetCurrentAttackCooldown()`: 获取当前攻击冷却时间

### 2. 修改的现有文件

#### ItemData.cs
- **修改**: 添加了 `public WeaponData weaponData;` 字段
- **用途**: 存储武器物品对应的 WeaponData 引用

#### EquipSlot.cs
- **修改**: 在 `SetItem()` 和 `Clear()` 方法中添加了武器装备/卸载逻辑
- **功能**: 当装备武器类型的物品时，自动调用 WeaponSystemBridge 进行装备

#### InventoryUI.cs
- **修改**: 添加了 `UpdateWeaponStats()` 方法
- **功能**: 实时更新武器状态栏（攻击力、范围、冷却时间）

#### 场景设置
- 需要手动在场景中添加 WeaponSystemBridge 和 TestWeaponIntegration 组件

### 3. 测试组件

#### TestWeaponIntegration.cs
- **位置**: `SunBai/Assets/Scripts/Inventory/TestWeaponIntegration.cs`
- **功能**: 在运行时自动设置 Sword 物品的 WeaponData 引用为 DuanYiBo 的 GuanDao

## 使用方法

### 1. 基本设置
1. 确保 DuanYiBo 项目中的脚本可以被 SunBai 项目访问
2. 在 Unity 中打开 SunBai 场景
3. 确保场景中有带 "Player" 标签的玩家对象，且该对象有 PlayerCombat 组件

### 2. 设置武器物品
对于每个武器类型的 ItemData，需要设置其 `weaponData` 字段：
```csharp
// 在代码中设置
ItemData sword = Resources.Load<ItemData>("Inventory/Items/Sword");
sword.weaponData = Resources.Load<WeaponData>("GuanDao");

// 或在 Inspector 中手动设置
```

### 3. 运行测试
1. 在场景中添加 WeaponSystemBridge 和 TestWeaponIntegration 组件
2. 运行游戏，按 I 键打开背包
3. 将 Sword 物品拖拽到武器装备槽中
4. 观察武器状态栏的实时更新

## 工作流程

1. **装备武器**:
   - 用户将武器物品拖拽到武器装备槽
   - EquipSlot 调用 WeaponSystemBridge.EquipWeapon()
   - WeaponSystemBridge 通过反射调用 DuanYiBo 的 PlayerCombat.EquipWeapon()
   - InventoryUI 更新状态栏显示

2. **卸载武器**:
   - 用户右键点击装备槽或拖拽其他物品覆盖
   - EquipSlot 调用 WeaponSystemBridge.UnequipWeapon()
   - WeaponSystemBridge 调用 DuanYiBo 的 PlayerCombat.UnequipWeapon()

3. **状态更新**:
   - WeaponSystemBridge 监听装备/卸载事件
   - InventoryUI 实时更新攻击力、范围、冷却时间显示

## 注意事项

1. **项目引用**: 确保 DuanYiBo 项目的脚本可以被 SunBai 项目访问
2. **玩家对象**: 需要有带 "Player" 标签的对象，且包含 PlayerCombat 组件
3. **WeaponData**: 每个武器物品都需要正确设置对应的 WeaponData 引用
4. **测试环境**: TestWeaponIntegration 脚本会自动设置 Sword 的引用用于测试

## 扩展

如需添加更多武器或装备类型：
1. 在 ItemData 中添加相应的 WeaponData 引用字段
2. 在 EquipSlot 中添加相应类型的处理逻辑
3. 在 InventoryUI 中添加相应的状态栏
