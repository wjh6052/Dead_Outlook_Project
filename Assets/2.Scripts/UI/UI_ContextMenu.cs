using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UI_ContextMenu : MonoBehaviour
{
    [SerializeField] private Button Btn_Info;  // 정보 보기
    [SerializeField] private Button Btn_Use;   // 사용
    [SerializeField] private Button Btn_Split; // 분할
    [SerializeField] private Button Btn_Drop;  // 버리기

    //private UI_Item_Icon TargetIcon; // 우클릭된 아이콘

    private void Update()
    {
        if (!Mouse.current.leftButton.wasPressedThisFrame && !Mouse.current.rightButton.wasPressedThisFrame) return;

        // 클릭 위치가 컨텍스트 메뉴 영역 밖이면 파괴
        if (!RectTransformUtility.RectangleContainsScreenPoint(
                GetComponent<RectTransform>(), Mouse.current.position.ReadValue()))
            Destroy(gameObject);
    }


    //public void Show(UI_Item_Icon inTargetIcon, Vector2 position)
    //{
    //    TargetIcon = inTargetIcon;
    //    transform.position = position;
    //
    //    Btn_Use.onClick.AddListener(OnUse);
    //    Btn_Split.onClick.AddListener(OnSplit);
    //    Btn_Drop.onClick.AddListener(OnDrop);
    //    Btn_Info.onClick.AddListener(OnInfo);
    //
    //    // 해당 보유 아이템 개수가 1개보다 적을때 분할 버튼 비활성화
    //    Btn_Split.interactable = inTargetIcon.ItemData.Amount > 1;
    //
    //}
   

    private void OnInfo()
    {
        Debug.Log($"정보 보기 버튼");
    }
    private void OnUse()
    {
        Debug.Log($"사용 버튼");
    }
    private void OnSplit()
    {
        //UI_ItemSplit splitPrefab = Resources.Load<UI_ItemSplit>("Prefab/UI/Inventory/PF_UI_ItemSplit");
        //UI_ItemSplit splitUI     = Instantiate(splitPrefab, Mgr_UI.Inst.MainCanvas.transform);
        //splitUI.transform.SetAsLastSibling();
        //splitUI.Info(TargetIcon);
        //Destroy(gameObject);
    }
    private void OnDrop()
    {
        Debug.Log($"버리기 버튼");
    }
    
}
