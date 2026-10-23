using UnityEngine;

// 최대값과 현재값을 함께 관리하는 범용 스탯 구조체 (HP · 스태미나 등)
// 구조체이므로 속성({ get; set; })이 아닌 필드로 선언해야 Add · Fill이 실제 값에 반영됨
[System.Serializable]
public struct FStat
{
    public float Max;     // 최대값
    public float Current; // 현재값

    public bool  IsEmpty => Current <= 0f; // 0 이하인지
    public float Ratio   => Current / Max; // 0~1 비율 (UI 바 표시용)

    public void Add(float amount) => Current = Mathf.Clamp(Current + amount, 0f, Max); // 증감 (음수면 감소, 0~Max 제한)
    public void Fill()            => Current = Max;                                    // 최대로 채우기
}
