using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 아이템 사용 집중 중 화면 중앙에 표시되는 원형 타이머 UI
// - FocusCoroutine에서 매 프레임 UpdateTimer()를 호출해 갱신
public class UI_FocusTimer : MonoBehaviour
{
    private Image           Fill_Image; // 방사형 채움 이미지 (Fill Type: Radial 360)
    private TextMeshProUGUI Timer_TMP;  // 남은 시간 텍스트 (초 카운트다운)

    public void Init(float useTime)
    {
        Fill_Image = transform.Find("Fill_Image").GetComponent<Image>();
        Timer_TMP  = transform.Find("Timer_TMP").GetComponent<TextMeshProUGUI>();

        Fill_Image.fillAmount = 1f;
        Timer_TMP.text        = Mathf.CeilToInt(useTime).ToString();
    }

    // FocusCoroutine에서 매 프레임 호출 — 남은 시간과 전체 시간을 받아 UI 갱신
    public void UpdateTimer(float remaining, float total)
    {
        Fill_Image.fillAmount = remaining / total;
        Timer_TMP.text        = remaining.ToString("F1");
    }
}
