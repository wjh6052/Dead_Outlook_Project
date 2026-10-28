using UnityEngine;

// 경과 시간과 최대 시간을 함께 관리하는 범용 타이머 구조체
[System.Serializable]
public struct FTimer
{
    public float Max;     // 최대 시간
    public float Current; // 현재 경과 시간

    public bool IsReached => Current >= Max;  // 최대 시간 도달 여부

    public void Tick()    => Current += Time.deltaTime; // 매 프레임 경과 시간 누적
    public void Reset()   => Current  = 0f;             // 타이머 초기화
}
