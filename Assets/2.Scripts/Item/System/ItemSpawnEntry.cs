using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemSpawnData
{
    public EItemType ItemType;                             // 스폰할 아이템 타입
    public int SpawnCount = 1;                             // 해당 확률로 시도할 횟수
    [Range(0, 100)] public float WeightNoSpawn   = 16.6f; // 미등장 확률
    [Range(0, 100)] public float WeightNormal    = 16.6f; // Normal 희귀도 가중치
    [Range(0, 100)] public float WeightRare      = 16.6f; // Rare 희귀도 가중치
    [Range(0, 100)] public float WeightEpic      = 16.6f; // Epic 희귀도 가중치
    [Range(0, 100)] public float WeightUnique    = 16.6f; // Unique 희귀도 가중치
    [Range(0, 100)] public float WeightLegendary = 16.6f; // Legendary 희귀도 가중치

    // 가중치 기반으로 희귀도 랜덤 선택 — null이면 미등장
    public ERarity? PickRarity()
    {
        float roll = Random.Range(0f, 100f);
        float cumulative = WeightNoSpawn;
        if (roll < cumulative) return null;

        float[] weights = { WeightNormal, WeightRare, WeightEpic, WeightUnique, WeightLegendary };
        for (int i = 0; i < weights.Length; i++)
        {
            cumulative += weights[i];
            if (roll < cumulative) return (ERarity)i;
        }
        return null; // 나머지 구간도 미등장
    }
}

public class ItemSpawnEntry : MonoBehaviour
{
    [SerializeField] private List<ItemSpawnData> SpawnTable;

    // SpawnTable 전체를 순회해 SpawnCount만큼 확률 시도 후 등장한 아이템 목록 반환
    public List<Item> GetItems()
    {
        List<Item> result = new List<Item>();
        foreach (ItemSpawnData data in SpawnTable)
        {
            for (int i = 0; i < data.SpawnCount; i++)
            {
                Item item = Mgr_Item.Inst.GetRandomItem(data);
                if (item != null) result.Add(item);
            }
        }
        return result;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (SpawnTable == null) return;
        foreach (ItemSpawnData data in SpawnTable)
        {
            float excess = (data.WeightNoSpawn + data.WeightNormal + data.WeightRare + data.WeightEpic
                          + data.WeightUnique + data.WeightLegendary) - 100f;
            if (excess <= 0) continue;

            // Normal부터 순서대로 초과분 차감
            float[] weights = { data.WeightNormal, data.WeightRare, data.WeightEpic, data.WeightUnique, data.WeightLegendary };
            for (int i = 0; i < weights.Length && excess > 0; i++)
            {
                float reduce = Mathf.Min(weights[i], excess);
                weights[i] -= reduce;
                excess     -= reduce;
            }
            data.WeightNormal    = weights[0];
            data.WeightRare      = weights[1];
            data.WeightEpic      = weights[2];
            data.WeightUnique    = weights[3];
            data.WeightLegendary = weights[4];
        }
    }
#endif
}
