using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_W_ItemSplit : MonoBehaviour, IUI_WindowContent
{
    public UI_Window Window { get; set; }
    [SerializeField] private Vector2 _windowSize = new Vector2(680, 275);
    public Vector2 WindowSize => _windowSize;

    private TextMeshProUGUI ItemName_TMP;
    private Scrollbar       Split_Slider;
    private TMP_InputField  Amount_Input;
    private Button          Btn_Confirm;

    private UI_ItemIcon TargetIcon;
    private int         Amount;
    private int         SplitCount;

    public void Init(UI_ItemIcon icon)
    {
        ItemName_TMP = transform.Find("ItemName_TMP").GetComponent<TextMeshProUGUI>();
        Split_Slider = transform.Find("Slider_Panel/Split_Slider").GetComponent<Scrollbar>();
        Amount_Input = transform.Find("Slider_Panel/Amount_Input").GetComponent<TMP_InputField>();
        Btn_Confirm  = transform.Find("Btn_Confirm").GetComponent<Button>();

        Btn_Confirm.onClick.AddListener(OnConfirm);

        TargetIcon = icon;

        ItemName_TMP.text = icon.ItemData.Name;

        Amount     = icon.ItemData.Amount;
        SplitCount = Amount / 2;

        Amount_Input.text             = SplitCount.ToString();
        Amount_Input.contentType      = TMP_InputField.ContentType.IntegerNumber;
        Split_Slider.numberOfSteps    = Amount - 1;
        Split_Slider.value            = (float)(SplitCount - 1) / (Amount - 2);

        Split_Slider.onValueChanged.AddListener(OnSliderChanged);
        Amount_Input.onEndEdit.AddListener(OnInputChanged);
    }

    // 슬라이더 값(0~1)을 1~(Amount-1) 범위로 변환해 입력창에 반영
    private void OnSliderChanged(float value)
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

        Split_Slider.onValueChanged.RemoveListener(OnSliderChanged);
        Split_Slider.value = (float)(SplitCount - 1) / (max - 1);
        Split_Slider.onValueChanged.AddListener(OnSliderChanged);
    }

    // 확인 버튼
    private void OnConfirm()
    {
        Item splitItem = TargetIcon.ItemData.Copy();
        splitItem.SetAmount(SplitCount);

        if (TargetIcon.Inventory.TryAddItem(splitItem))
        {
            TargetIcon.ItemData.DecreaseAmount(SplitCount);
            TargetIcon.UpdateAmount();
        }

        Window.WindowClose();
    }
}
