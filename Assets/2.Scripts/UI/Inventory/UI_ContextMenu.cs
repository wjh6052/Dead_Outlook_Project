using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UI_ContextMenu : MonoBehaviour
{
    private Button Btn_Info;  // 정보 보기
    private Button Btn_Opne;   // 열기
    private Button Btn_Use;   // 사용
    private Button Btn_Split; // 분할
    private Button Btn_Drop;  // 버리기

    private UI_ItemIcon TargetItemIcon; // 우클릭된 아이콘

    private void Awake()
    {
        Btn_Info    = gameObject.transform.Find("Btn_Info").GetComponent<Button>();
        Btn_Opne    = gameObject.transform.Find("Btn_Opne").GetComponent<Button>();
        Btn_Use     = gameObject.transform.Find("Btn_Use").GetComponent<Button>();
        Btn_Split   = gameObject.transform.Find("Btn_Split").GetComponent<Button>();
        Btn_Drop    = gameObject.transform.Find("Btn_Drop").GetComponent<Button>();

        Btn_Info.onClick.AddListener(OnInfo);
        Btn_Opne.onClick.AddListener(OnOpne);
        Btn_Use.onClick.AddListener(OnUse);
        Btn_Split.onClick.AddListener(OnSplit);
        Btn_Drop.onClick.AddListener(OnDrop);
    }

    public void Init(UI_ItemIcon itemIcon, Vector2 position)
    {
        TargetItemIcon = itemIcon;
        transform.position = position;

        Item item = itemIcon.ItemData;
        // 컨테이너(가방 등)만 열기 버튼 표시
        Btn_Opne.gameObject.SetActive(item.ItemType == EItemType.Container);
        // 음식 / 의료 아이템만 사용 버튼 표시
        Btn_Use.gameObject.SetActive(item.ItemType == EItemType.Food || item.ItemType == EItemType.Medical);
        // 무기·방어구·컨테이너는 분할 불가
        Btn_Split.gameObject.SetActive(item.ItemType != EItemType.WeaponMelee && item.ItemType != EItemType.WeaponRanged
                                    && item.ItemType != EItemType.Armor && item.ItemType != EItemType.Container);
        // 수량이 1 이하면 분할 버튼 비활성화
        Btn_Split.interactable = item.Amount > 1;
    }

    private void Update()
    {
        if (!Mouse.current.leftButton.wasPressedThisFrame && !Mouse.current.rightButton.wasPressedThisFrame) return;

        // 클릭 위치가 컨텍스트 메뉴 영역 밖이면 파괴
        if (!RectTransformUtility.RectangleContainsScreenPoint(
                GetComponent<RectTransform>(), Mouse.current.position.ReadValue()))
            Destroy(gameObject);
    }



    // 상세 보기 버튼
    private void OnInfo()
    {
        Item item = TargetItemIcon.ItemData;
        UI_W_ItemInfo content = Instantiate(Resources.Load<UI_W_ItemInfo>("Prefab/UI/Window/PF_UI_W_ItemInfo"));
        content.Init(item);
        Mgr_UI.Inst.SpawnWindow(item.Name, content);
        Destroy(gameObject);
    }

    // 열기(가방) 버튼
    private void OnOpne()
    {
        Item_Container bag = TargetItemIcon.ItemData as Item_Container;
        UI_W_ContainerInventory content = Instantiate(Resources.Load<UI_W_ContainerInventory>("Prefab/UI/Window/PF_UI_W_ContainerInventory"));
        content.Init(bag);
        Mgr_UI.Inst.SpawnWindow(bag.Name, content);
        Destroy(gameObject);
    }

    // 사용 버튼
    private void OnUse()
    {
        bool success = Character_Player.Inst.UseItem(TargetItemIcon.ItemData);
        if (success) Mgr_PlayerController.Inst.OnInventory();

        Destroy(gameObject);
    }

    // 분할 버튼
    private void OnSplit()
    {
        UI_W_ItemSplit content = Instantiate(Resources.Load<UI_W_ItemSplit>("Prefab/UI/Window/PF_UI_W_ItemSplit"));
        content.Init(TargetItemIcon);
        Mgr_UI.Inst.SpawnWindow("아이템 분할", content);
        Destroy(gameObject);
    }

    // 버리기 버튼
    private void OnDrop()
    {
        Item item = TargetItemIcon.ItemData;
        Vector3 pos = Character_Player.Inst.transform.position;

        TargetItemIcon.Inventory.RemoveItem(TargetItemIcon);
        Destroy(TargetItemIcon.gameObject);

        Mgr_Item.Inst.SpawnItem(item, pos);
        Destroy(gameObject);
    }
    
}
