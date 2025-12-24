Inventory assets and sprites

Place all inventory-related sprites and UI art inside:

  Assets/Inventory/Sprites/

Suggested filenames:
  - weapon_sword.png       (weapon icon example)
  - gear_armor.png         (equipment icon example)
  - consumable_potion.png  (consumable icon example)
  - empty.png              (empty slot placeholder)
  - ui_panel_bg.png        (inventory base panel background)
  - slot_frame.png         (grid slot frame)

Create ScriptableObject ItemData assets in:
  Assets/Inventory/Items/
  (Right-click -> Create -> Inventory -> ItemData)
  Set `itemName`, `itemType`, and `icon` (choose sprites above).

UI setup notes:
- `Canvas/InventoryRoot` should be the `inventoryRoot` GameObject referenced by `InventoryUI`.
- Left portrait: assign Image to `portraitImage`.
- Create 4 EquipSlot GameObjects (2x2 layout) and assign to `weaponSlot`, `gearSlot`, `consumableSlotA`, `consumableSlotB` on `InventoryUI`.
- Create a `slotPrefab` that contains an `InventorySlot` component, an Image for the icon and a Button (hook the button in the prefab to the InventorySlot button field).

Toggle inventory with the `I` key (runtime).
 
Quick run instructions (demo scene):

1. Create a new empty Scene in Unity (File → New Scene).
2. Create an empty GameObject in the Scene and name it `InventoryDemoBootstrap`.
3. Add the `InventoryDemoBootstrap` component (script) to that GameObject.
4. Press Play — the script will create a Canvas and the inventory UI at runtime. Press `I` to toggle the inventory.

Notes about sprites:
- The demo bootstrap generates simple colored runtime sprites if no art is present.
- For production, add real sprites to `Assets/Inventory/Sprites/` and create `ItemData` assets under `Assets/Inventory/Items/` then drag them into the `InventoryUI.inventoryItems` list in the inspector.

Creating a persistent prefab (optional):
- To make a reusable `slotPrefab` in-editor:
  1. In the Hierarchy, create a UI → Button, name it `SlotPrefab`.
  2. Inside it, keep the Button's Image as background and add a child Image for the icon.
  3. Add the `InventorySlot` component to the root `SlotPrefab` GameObject and wire the `icon` and `button` fields.
  4. Drag the `SlotPrefab` from Hierarchy into `Assets/Inventory/Prefabs/SlotPrefab.prefab`.
