using UnityEngine;
using UnityEngine.InputSystem;

public class Test_ : MonoBehaviour
{
    public Character_Zombie Test_Character_Zombie;

    public float NoiseIntensity;
    public float NoiseRadius;


    void Update()
    {
        // T
        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            Mgr_Noise.Inst.MakeNoise(Character_Player.Inst.gameObject, NoiseIntensity, NoiseRadius);
        }

        // Y

        if (Keyboard.current.yKey.wasPressedThisFrame)
        {
            //Test_Character_Zombie?.TakeDamage(0.5f, this.gameObject, this.gameObject);
            // 플레이어에게 배트 지급
            Character_Player.Inst?.InventoryComp.TryAddItem(Mgr_Item.Inst.GetItem("Test_Bet"));
        }
    }
}
