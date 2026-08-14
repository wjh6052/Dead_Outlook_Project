using TMPro;
using UnityEngine;
using UnityEngine.UI;

// EscapeZone 체류 중 표시되는 귀환 타이머 UI
// - EscapeZone에서 매 프레임 UpdateTimer()를 호출해 갱신
public class UI_EscapeTimer : MonoBehaviour
{
    [SerializeField] private Image           Fill_Image; // 방사형 채움 이미지 (Fill Type: Radial 360)
    [SerializeField] private TextMeshProUGUI Timer_TMP;  // 남은 시간 텍스트

    public void Init(bool isOn)
    {
        gameObject.SetActive(isOn);
    }

    // EscapeZone에서 매 프레임 호출 — FTimer를 받아 UI 갱신
    public void UpdateTimer(FTimer timer)
    {
        Fill_Image.fillAmount = timer.Current / timer.Max;
        Timer_TMP.text        = (timer.Max - timer.Current).ToString("F1");
    }
}
