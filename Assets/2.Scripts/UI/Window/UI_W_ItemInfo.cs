using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 아이템 상세 정보 창 콘텐츠
// - 인벤토리 컨텍스트 메뉴 -> 상세보기 버튼으로 열림
// - 아이콘, 설명, 아이템 타입별 스탯을 표시
public class UI_W_ItemInfo : MonoBehaviour, IUI_WindowContent
{
    public UI_Window Window { get; set; }
    [SerializeField] private Vector2 _windowSize = new Vector2(800, 700);
    public Vector2 WindowSize => _windowSize;

    // UI 참조
    private Image           Icon_BackImage; // 아이콘 배경 (희귀도 색상 적용)
    private Image           Icon_Image;     // 아이템 아이콘
    private TextMeshProUGUI Description_TMP; // 설명 텍스트

    // 아이템 데이터를 받아 UI 초기화
    public void Init(Item item)
    {
        Icon_BackImage  = transform.Find("Icon_BackImage").GetComponent<Image>();
        Icon_Image      = Icon_BackImage.transform.Find("Icon_Image").GetComponent<Image>();
        Description_TMP = transform.Find("Description_TMP").GetComponent<TextMeshProUGUI>();

        // 공통 — 희귀도 색상, 아이콘
        Icon_BackImage.color = Mgr_UI.Inst.GetRarityColor(item.Rarity);
        Icon_Image.sprite    = item.Icon;

        // 타입별 스탯을 설명 앞에 먼저 출력
        Description_TMP.text = "";
        switch (item.ItemType)
        {
            case EItemType.General:
                break;
            case EItemType.WeaponMelee:
                break;
            case EItemType.WeaponRanged:
                break;
            case EItemType.Armor:
                break;
            case EItemType.Tool:
                break;
            case EItemType.Food:
                break;
            case EItemType.Medical:
                Item_Medical medical = item as Item_Medical;
                Description_TMP.text += $"[회복량 {medical.HealAmount}] [사용시간 {medical.UseTime}] \n";
                break;
            case EItemType.Container:
                break;
            default:
                break;
        }

        // 공통 — 아이템 설명
        Description_TMP.text += item.Description;
    }
}
