using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_PlayerStatus : MonoBehaviour
{
    [SerializeField] private Image HPBar_Image;
    [Tooltip("스태미나 바")]
    [SerializeField] private Image StaminaBar_Image;
    [Tooltip("배고픔 바")]
    [SerializeField] private Image HungerBar_Image;
    [Tooltip("목마름 바")]
    [SerializeField] private Image ThirstBar_Image;
    [Tooltip("소지 무게 텍스트 (현재 / 과적 기준 kg + 과적 상태)")]
    [SerializeField] private TextMeshProUGUI Weight_TMP;

    private void Update()
    {
        if (Character_Player.Inst == null) return;
        Character_Player player = Character_Player.Inst;

        HPBar_Image.fillAmount     = player.HP.Ratio;
        StaminaBar_Image.fillAmount = player.Stamina.Ratio;
        HungerBar_Image.fillAmount = player.Hunger.Ratio;
        ThirstBar_Image.fillAmount = player.Thirst.Ratio;

        // 무게 + 과적 상태
        string state = player.OverweightState switch
        {
            EOverweightState.Overweight       => "과적",
            EOverweightState.SevereOverweight => "초과적",
            _                                 => "보통",
        };
        Weight_TMP.text = $"{player.CarryWeight:0.0} / {player.OverweightLimit:0.0} kg ({state})";
    }
}
