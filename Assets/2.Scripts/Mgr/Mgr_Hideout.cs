using UnityEngine;
using UnityEngine.SceneManagement;

// 은신처 데이터 관리 — Base_Scene에 배치
// - 씬 전환과 무관하게 은신처 관련 데이터를 영구 보관
public class Mgr_Hideout : MonoBehaviour
{

    [Header("씬")]
    [SerializeField] private string HideoutSceneName = "Hideout_Scene";

    [field: SerializeField] public bool IsOnHideout { get; private set; } // 현재 은신처에 있는지 여부

    [Header("보관함")]
    [SerializeField] public InventoryData StorageData = new InventoryData(); // 은신처 보관함 데이터

    public static Mgr_Hideout Inst { get; private set; }

    private void Awake()
    {
        Inst = this;
    }

    private void Start()
    {
        // 테스트
        {
            // 테스트 모든 아이템 추가
            foreach (Item item in Mgr_Item.Inst.GetAllItems())
                StorageData.TryAddItem(item.Copy());
        }
        
    }

    // true: Hideout_Scene 로드 / false: Hideout_Scene 언로드
    public void SetHideout(bool isOn)
    {
        IsOnHideout = isOn;

        if (isOn)
        {
            // 이미 로드된 경우 중복 로드 방지
            if (SceneManager.GetSceneByName(HideoutSceneName).isLoaded) return;
            SceneManager.LoadScene(HideoutSceneName, LoadSceneMode.Additive);
        }
        else
        {
            SceneManager.UnloadSceneAsync(HideoutSceneName);
        }
    }
}
