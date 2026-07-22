using TMPro;
using UnityEngine;

// 월드 공간에 표시되는 상호작용 UI — Mgr_PlayerController가 포커스 변경 시 호출
public class WUI_Interact : MonoBehaviour
{
    [SerializeField] private GameObject     Interact_Dot;

    private void Awake()
    {
        this.gameObject.SetActive(false);
        Interact_Dot.SetActive(true);
    }

    private void LateUpdate()
    {
        transform.rotation = Mgr_Camera.Inst.CameraController.MainCamera.transform.rotation;
    }

    public void Show(bool show, Interactable target = null)
    {
        this.gameObject.SetActive(show);
        if (show && target != null)
        {
            this.transform.position = target.InteractUIPosition;

            if (target is Interactable_Item itemTarget)
                transform.Find("InteractName_TMP").GetComponent<TextMeshProUGUI>().text = $"{itemTarget.ItemData.Name} {itemTarget.ItemData.Amount}개 줍기";
        }
    }

    
}
