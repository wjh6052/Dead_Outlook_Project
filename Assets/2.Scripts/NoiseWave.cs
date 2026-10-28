using System.Collections;
using UnityEngine;

// 소음 파동 시각화 — LineRenderer로 원형 링이 퍼지며 페이드 아웃
[RequireComponent(typeof(LineRenderer))]
public class NoiseWave : MonoBehaviour
{
    [SerializeField] private int   Segments    = 64;   // 원을 구성하는 점 개수 (클수록 부드러움)
    [SerializeField] private float LineWidth   = 0.05f; // 선 굵기
    [SerializeField] [Range(0f, 1f)] private float ExpandRatio = 0.6f; // 전체 duration 중 확장에 쓰는 비율 (나머지는 페이드)
    [SerializeField] private float Duration    = 1f;    // 파동 지속 시간 (초)

    private LineRenderer Lr;

    // ============================
    // Unity 기본 함수
    // ============================

    private void Awake()
    {
        Lr = GetComponent<LineRenderer>();
        Lr.positionCount  = Segments + 1;
        Lr.loop           = true;
        Lr.useWorldSpace  = false;
        Lr.startWidth     = LineWidth;
        Lr.endWidth       = LineWidth;
    }

    // ============================
    // 파동
    // ============================

    // radius   : 파동이 퍼질 최대 반지름 (m)
    // duration : 파동 지속 시간 (초) — Duration 변수 사용
    public void Init(float radius)
    {
        StartCoroutine(C_PlayWave(radius, Duration));
    }

    private IEnumerator C_PlayWave(float radius, float duration)
    {
        Color baseColor  = Lr.startColor;
        float expandTime = duration * ExpandRatio;
        float fadeTime   = duration * (1f - ExpandRatio);

        // Phase 1 — 확장: 반지름 0 → radius, alpha 유지
        float t = 0f;
        while (t < expandTime)
        {
            float currentRadius = Mathf.Lerp(0f, radius, t / expandTime);
            UpdateCircle(currentRadius);
            t += Time.deltaTime;
            yield return null;
        }

        // Phase 2 — 페이드: 반지름 radius 유지, alpha 1 → 0
        UpdateCircle(radius);
        t = 0f;
        while (t < fadeTime)
        {
            float alpha = 1f - (t / fadeTime);
            Color c     = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
            Lr.startColor = c;
            Lr.endColor   = c;
            t += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

    private void UpdateCircle(float radius)
    {
        for (int i = 0; i <= Segments; i++)
        {
            float   angle = i * 2f * Mathf.PI / Segments;
            Vector3 pos   = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
            Lr.SetPosition(i, pos);
        }
    }
}
