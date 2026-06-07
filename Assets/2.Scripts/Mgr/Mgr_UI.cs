using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugManager;

public class Mgr_UI : MonoBehaviour
{
    public bool IsOnUI = false;

    private GameObject Inventory_Panel;

    // 현재 열려있는 인벤토리 등록 — 가방 열기/닫기 시 갱신용
    private Dictionary<InventoryData, UI_Inventory> OpenInventories = new();

    public void RegisterOpenInventory(InventoryData data, UI_Inventory ui) => OpenInventories[data] = ui;
    public void UnregisterOpenInventory(InventoryData data)               => OpenInventories.Remove(data);
    public UI_Inventory GetOpenInventory(InventoryData data)
        => OpenInventories.TryGetValue(data, out var ui) ? ui : null;

    #region Inventory_Panel
    [SerializeField] public UI_Inventory PocketInventory;
    [SerializeField] public UI_EquipSlot BeltEquipSlot;
    [SerializeField] public UI_EquipSlot BagEquipSlot;

    [SerializeField] public UI_Inventory  UI_Inventory_1test; // 사용 가능한 UI_Inventory 풀
    #endregion

    public Canvas MainCanvas { get; private set; } // 메인 UI 캔버스
    private UI_ContextMenu ContextMenuPrefab; // 컨텍스트 메뉴 프리팹


    public static Mgr_UI Inst { get; private set; }

    private void Awake()
    {
        Inst              = this;
        MainCanvas        = GameObject.FindWithTag("MainCanvas").GetComponent<Canvas>();
        Inventory_Panel   = GameObject.Find("Inventory_Panel");

        
    }

    private void Start()
    {
        OnInventory(false);
    }

    private void Update()
    {
        // TODO: 테스트 코드 — 삭제 필요
        if (Keyboard.current.backquoteKey.wasPressedThisFrame)
        {
            Item i = Mgr_ItemData.Inst.GetItem("Test_1");
            i.AddAmount(40);
            
            UI_Inventory_1test.TryAddItem(i.Copy());
            UI_Inventory_1test.TryAddItem(i.Copy());
            //UI_Inventory_1test.TryAddItem(Mgr_ItemData.Inst.GetItem("Test_2"));
            //UI_Inventory_1test.TryAddItem(Mgr_ItemData.Inst.GetItem("Test_3"));
            UI_Inventory_1test.TryAddItem(Mgr_ItemData.Inst.GetItem("Test_Belt_1"));
            UI_Inventory_1test.TryAddItem(Mgr_ItemData.Inst.GetItem("Test_Bag_2"));
            UI_Inventory_1test.TryAddItem(Mgr_ItemData.Inst.GetItem("Test_Bag_3"));
        }

    }

    // 인벤토리 열기 닫기
    public void OnInventory(bool isOn)
    {
        IsOnUI = isOn;
        Inventory_Panel.SetActive(isOn);
        if (IsOnUI == false) return;

        UI_Inventory_1test.gameObject.SetActive(isOn);

        UI_Inventory_1test.UpdateInventory(Entity_Player.Inst.Inventory.Inventory_Test);
    }

    // 드래그중인 아이템 회전
    public void OnRotateItem()
    {
        if (UI_Item_Icon.BeingDraggedIcon == null) return;

        UI_Item_Icon.BeingDraggedIcon.Rotate();
    }


    // 컨텍스트 메뉴 표시 — 프리팹 스폰 후 Show 호출
    public void ShowContextMenu(UI_Item_Icon icon, Vector2 position)
    {
        if (ContextMenuPrefab == null)
            ContextMenuPrefab = Resources.Load<UI_ContextMenu>("Prefab/UI/Inventory/PF_UI_ContextMenu");

        UI_ContextMenu menu = Instantiate(ContextMenuPrefab, MainCanvas.transform);
        menu.gameObject.SetActive(true);
        menu.Show(icon, position);
    }

    

}
