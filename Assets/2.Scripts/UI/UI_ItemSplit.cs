using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.AdaptivePerformance.Provider.AdaptivePerformanceSubsystemDescriptor;

public class UI_ItemSplit : MonoBehaviour
{
    [SerializeField] private UI_DragWindow   DragWindow;   // 드래그 핸들 패널
    [SerializeField] private TextMeshProUGUI ItemName_TMP; // 아이템 이름
    [SerializeField] private TMP_InputField  Amount_Input;  // 분할 개수 입력
    [SerializeField] private Scrollbar       Split_Scrollbar;  // 개수 조절 슬라이더
    [SerializeField] private Button          Btn_Confirm;  // 확인 버튼
    [SerializeField] private Button          Btn_Cancel;   // 취소 버튼

    private UI_Item_Icon TargetIcon;  // 분할 대상 아이콘
    private int          Amount;     // 분할 대상 아이템 총 개수
    private int          SplitCount; // 현재 선택 중인 분할 개수

    public void Info(UI_Item_Icon inTargetIcon)
    {
        TargetIcon = inTargetIcon;

        DragWindow.Info(GetComponent<RectTransform>());

        // 이름 설정
        ItemName_TMP.text = TargetIcon.ItemData.Name;

        Amount     = TargetIcon.ItemData.Amount;
        SplitCount = Amount / 2;

        // 분할 개수 기본값 — 절반
        Amount_Input.text = SplitCount.ToString();

        // 스크롤바 범위: 1 ~ Amount-1, 기본값 절반
        Split_Scrollbar.numberOfSteps = Amount - 1;
        Split_Scrollbar.value         = (float)(SplitCount - 1) / (Amount - 2);


        // 스크롤바 값 변경 시 입력창 갱신
        Split_Scrollbar.onValueChanged.AddListener(OnScrollbarChanged);

        // 입력창 설정 — 정수만 허용
        Amount_Input.contentType = TMP_InputField.ContentType.IntegerNumber;
        Amount_Input.onEndEdit.AddListener(OnInputChanged);

        // 버튼 설정
        Btn_Confirm.onClick.AddListener(OnConfirm);
        Btn_Cancel.onClick.AddListener(OnCancel);
    }

    // 스크롤바 값(0~1)을 1~(Amount-1) 범위로 변환해 입력창에 반영
    private void OnScrollbarChanged(float value)
    {
        SplitCount        = Mathf.RoundToInt(1 + value * (Amount - 2));
        Amount_Input.text = SplitCount.ToString();
    }

    // 입력창 값 변경 시 최소/최대 범위 조정 후 슬라이더 갱신
    private void OnInputChanged(string input)
    {
        int min = 1;
        int max = Amount - 1;

        if (!int.TryParse(input, out int value)) value = min;

        SplitCount        = Mathf.Clamp(value, min, max);
        Amount_Input.text = SplitCount.ToString();

        // 슬라이더 갱신 — 이벤트 중복 방지를 위해 리스너 일시 제거
        Split_Scrollbar.onValueChanged.RemoveListener(OnScrollbarChanged);
        Split_Scrollbar.value = (float)(value - 1) / (max - 1);
        Split_Scrollbar.onValueChanged.AddListener(OnScrollbarChanged);
    }

    // 수락버튼
    void OnConfirm()
    {
        Item splitItem = TargetIcon.ItemData.Copy();
        splitItem.SetAmount(SplitCount);

        if (TargetIcon.ParentInventory.TryAddItem(splitItem))
        {
            TargetIcon.ItemData.DecreaseAmount(SplitCount);
            TargetIcon.UpdateAmountText();
        }

            
        Destroy(gameObject);
    }

    // 닫기 버튼
    void OnCancel()
    {
        Destroy(this.gameObject);
    }

}
