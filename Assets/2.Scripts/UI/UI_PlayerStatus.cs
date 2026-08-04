using UnityEngine;
using UnityEngine.UI;

public class UI_PlayerStatus : MonoBehaviour
{
    [SerializeField] private Image HPBar_Image;

    private void Update()
    {
        if (Character_Player.Inst == null) return;
        HPBar_Image.fillAmount = Character_Player.Inst.HP / Character_Player.Inst.MaxHP;
    }
}
