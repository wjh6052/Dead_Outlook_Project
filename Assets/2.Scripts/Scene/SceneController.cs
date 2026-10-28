using System.Collections;
using UnityEngine;

// 씬 컨트롤러 공통 부모 — 씬 로드 시 담당 매니저에 등록
public abstract class SceneController : MonoBehaviour
{
    // 준비 → 등록
    protected virtual IEnumerator Start()
    {
        yield return C_Prepare(); // 1. 준비 작업
        Register();               // 2. 매니저에 등록
    }

    protected virtual void OnDestroy() => Unregister(); // 씬 언로드 시 해제

    protected virtual IEnumerator C_Prepare() { yield break; } // 준비 작업 (필요한 씬만 덮어쓰기)
    protected abstract void Register();      // 매니저에 등록 (구현 필수) — C_Prepare가 끝난 후 호출
    protected virtual  void Unregister() { } // 매니저에서 해제 (필요한 씬만 덮어쓰기)
}
