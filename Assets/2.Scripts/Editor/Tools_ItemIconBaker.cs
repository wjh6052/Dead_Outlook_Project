using UnityEngine;
using UnityEditor;
using System.IO;

// 아이템 아이콘 베이킹 툴
// Tools > Dead Outlook > Item Icon Baker 에서 열기
// 프리팹을 드래그하면 미리보기 → Bake 버튼으로 PNG 저장
public class Tools_ItemIconBaker : EditorWindow
{
    // 설정
    private GameObject _prefab;
    private int        _resolutionX = 256;
    private int        _resolutionY = 256;
    private Vector3    _rotation   = new Vector3(0f, 0f, 0f); // X = 피치, Y = 요, Z = 롤(화면 기울기)
    private float      _distance   = 2f;
    private float      _fov        = 30f;
    private string     _fileName   = "";

    private const string SavePath = "Assets/Resources/UI/Icon";

    private PreviewRenderUtility _preview;
    private Vector2              _scrollPos; // 스크롤 뷰 위치


    [MenuItem("Tools/Dead Outlook/Item Icon Baker")]
    static void Open()
    {
        Tools_ItemIconBaker window = GetWindow<Tools_ItemIconBaker>("Icon Baker");
        window.ResizeToFit();
    }

    // 해상도에 맞춰 창 크기 조정
    private void ResizeToFit()
    {
        float w = Mathf.Max(_resolutionX + 40f, 300f);
        float h = _resolutionY + 340f; // UI 요소 공간 (설정 + 버튼 등)
        minSize = new Vector2(w, 200f);
        maxSize = new Vector2(4096f, 4096f);
        Rect rect = position;
        rect.width  = w;
        rect.height = h;
        position = rect;
    }

    private void OnEnable()
    {
        _preview = new PreviewRenderUtility();
        _preview.camera.clearFlags    = CameraClearFlags.SolidColor;
        _preview.camera.nearClipPlane = 0.01f;
        _preview.camera.farClipPlane  = 100f;
    }

    private void OnDisable()
    {
        _preview?.Cleanup();
        _preview = null;
    }

    private void OnGUI()
    {
        // 스크롤 뷰로 감싸서 창이 작아도 Bake 버튼에 접근 가능
        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

        GUILayout.Label("Item Icon Baker", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);

        // 프리팹 필드
        EditorGUI.BeginChangeCheck();
        _prefab = (GameObject)EditorGUILayout.ObjectField("프리팹", _prefab, typeof(GameObject), false);
        if (EditorGUI.EndChangeCheck() && _prefab != null)
        {
            _fileName = "Icon_" + _prefab.name;
            AutoFitDistance();
        }

        EditorGUILayout.Space(8);

        // 카메라 설정
        GUILayout.Label("카메라 설정", EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck();
        _resolutionX = EditorGUILayout.IntPopup("해상도 X", _resolutionX,
                       new[] { "64", "128", "256", "512" }, new[] { 64, 128, 256, 512 });
        _resolutionY = EditorGUILayout.IntPopup("해상도 Y", _resolutionY,
                       new[] { "64", "128", "256", "512" }, new[] { 64, 128, 256, 512 });
        if (EditorGUI.EndChangeCheck())
            ResizeToFit();
        _fov        = EditorGUILayout.Slider("FOV", _fov, 5f, 90f);
        _distance   = EditorGUILayout.Slider("거리", _distance, 0.01f, 20f);
        _rotation   = EditorGUILayout.Vector3Field("각도 (미리보기 드래그로 조정)", _rotation);

        if (GUILayout.Button("거리 자동 맞춤", GUILayout.Height(22)))
            AutoFitDistance();

        EditorGUILayout.Space(8);

        // 저장 설정
        GUILayout.Label("저장", EditorStyles.boldLabel);
        _fileName = EditorGUILayout.TextField("파일 이름", _fileName);
        EditorGUILayout.LabelField("경로", $"{SavePath}/{_fileName}.png");

        EditorGUILayout.Space(8);

        // 미리보기 — 해상도 비율에 맞춰 표시
        Rect previewRect = GUILayoutUtility.GetRect(_resolutionX, _resolutionY);
        if (_prefab != null)
            DrawPreview(previewRect);
        else
            EditorGUI.HelpBox(previewRect, "프리팹을 드래그하세요.", MessageType.Info);

        EditorGUILayout.Space(8);

        // Bake 버튼
        GUI.enabled = _prefab != null && !string.IsNullOrEmpty(_fileName);
        if (GUILayout.Button("Bake", GUILayout.Height(40)))
            Bake();
        GUI.enabled = true;

        EditorGUILayout.Space(4);
        EditorGUILayout.EndScrollView();
    }


    // 미리보기 렌더링 + 마우스 드래그/스크롤 입력 처리
    private void DrawPreview(Rect rect)
    {
        Event e = Event.current;

        // 드래그로 회전
        if (e.type == EventType.MouseDrag && rect.Contains(e.mousePosition))
        {
            _rotation.y += e.delta.x * 0.5f;
            _rotation.x -= e.delta.y * 0.5f;
            Repaint();
        }

        // 스크롤로 줌
        if (e.type == EventType.ScrollWheel && rect.Contains(e.mousePosition))
        {
            _distance = Mathf.Clamp(_distance + e.delta.y * 0.05f, 0.01f, 20f);
            e.Use();
            Repaint();
        }

        _preview.BeginPreview(rect, GUIStyle.none);
        SetupCamera(_preview);
        DrawMeshes(_prefab);
        _preview.camera.Render();
        Texture tex = _preview.EndPreview();

        GUI.DrawTexture(rect, tex, ScaleMode.StretchToFill, true);
    }


    // 카메라 위치·방향·라이트 설정
    private void SetupCamera(PreviewRenderUtility p)
    {
        p.camera.backgroundColor = Color.clear;
        p.camera.fieldOfView     = _fov;

        Bounds  bounds = GetMeshBounds(_prefab);
        Vector3 center = bounds.center;

        Quaternion rot = Quaternion.Euler(_rotation.x, _rotation.y, 0);
        p.camera.transform.position = center + rot * (Vector3.back * _distance);
        p.camera.transform.LookAt(center);
        p.camera.transform.Rotate(0f, 0f, _rotation.z, Space.Self); // 롤 — 카메라를 앞 방향 축으로 돌려 화면 속 아이템을 기울임

        p.lights[0].intensity           = 1.4f;
        p.lights[0].transform.rotation = Quaternion.Euler(40f, 40f, 0);
        p.lights[1].intensity           = 0.5f;
    }


    // 프리팹의 MeshFilter 전체를 DrawMesh로 렌더링
    private void DrawMeshes(GameObject prefab)
    {
        foreach (MeshFilter mf in prefab.GetComponentsInChildren<MeshFilter>())
        {
            if (mf.sharedMesh == null) continue;
            MeshRenderer mr = mf.GetComponent<MeshRenderer>();
            if (mr == null) continue;

            Matrix4x4 matrix = mf.transform.localToWorldMatrix;
            for (int i = 0; i < mf.sharedMesh.subMeshCount; i++)
            {
                Material mat = i < mr.sharedMaterials.Length ? mr.sharedMaterials[i] : null;
                if (mat != null)
                    _preview.DrawMesh(mf.sharedMesh, matrix, mat, i);
            }
        }
    }


    // PNG 저장 + Sprite로 임포트 설정
    private void Bake()
    {
        // PreviewRenderUtility는 EndStaticPreview에서 알파를 버림
        // → 검은/흰 배경으로 각각 렌더 후 Natural Matte로 알파 역산
        Texture2D onBlack = RenderStatic(Color.black);
        Texture2D onWhite = RenderStatic(Color.white);
        Texture2D result  = ExtractAlpha(onBlack, onWhite);
        DestroyImmediate(onBlack);
        DestroyImmediate(onWhite);

        // 폴더 없으면 생성
        if (!Directory.Exists(SavePath))
            Directory.CreateDirectory(SavePath);

        string path = Path.Combine(SavePath, _fileName + ".png").Replace("\\", "/");
        File.WriteAllBytes(path, result.EncodeToPNG());
        DestroyImmediate(result);

        // Refresh 후 Sprite 타입으로 임포트 설정
        AssetDatabase.Refresh();
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType         = TextureImporterType.Sprite;
            importer.spriteImportMode    = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled       = false;
            importer.SaveAndReimport();
        }
        else
        {
            Debug.LogWarning($"[Tools_ItemIconBaker] TextureImporter를 찾지 못했습니다: {path}");
        }

        Debug.Log($"[Tools_ItemIconBaker] 저장 완료 → {path}");
        EditorUtility.DisplayDialog("Bake 완료", $"{_fileName}.png 저장 완료!\n{SavePath}", "확인");
    }

    // 지정 배경색으로 정적 렌더 후 Texture2D 반환
    private Texture2D RenderStatic(Color bgColor)
    {
        _preview.BeginStaticPreview(new Rect(0, 0, _resolutionX, _resolutionY));
        SetupCamera(_preview);
        _preview.camera.backgroundColor = bgColor; // SetupCamera 이후 덮어씀
        DrawMeshes(_prefab);
        _preview.camera.Render();
        return _preview.EndStaticPreview();
    }

    // Natural Matte: 검은/흰 배경 결과로 정확한 알파·RGB 계산
    private Texture2D ExtractAlpha(Texture2D onBlack, Texture2D onWhite)
    {
        Color[] b      = onBlack.GetPixels();
        Color[] w      = onWhite.GetPixels();
        Color[] output = new Color[b.Length];

        for (int i = 0; i < b.Length; i++)
        {
            // alpha = 1 - 채널별 (흰배경 - 검은배경) 평균
            float alpha = 1f - Mathf.Clamp01((w[i].r - b[i].r + w[i].g - b[i].g + w[i].b - b[i].b) / 3f);

            // 알파가 있으면 프리멀티플라이 제거, 없으면 투명
            output[i] = alpha > 0.001f
                ? new Color(b[i].r / alpha, b[i].g / alpha, b[i].b / alpha, alpha)
                : Color.clear;
        }

        Texture2D result = new Texture2D(_resolutionX, _resolutionY, TextureFormat.RGBA32, false);
        result.SetPixels(output);
        result.Apply();
        return result;
    }


    // 모델 AABB 계산 (프리팹 계층의 MeshFilter 기준)
    private Bounds GetMeshBounds(GameObject prefab)
    {
        bool   first = true;
        Bounds b     = new Bounds();

        foreach (MeshFilter mf in prefab.GetComponentsInChildren<MeshFilter>())
        {
            if (mf.sharedMesh == null) continue;

            Bounds world = TransformBounds(mf.sharedMesh.bounds, mf.transform.localToWorldMatrix);
            if (first) { b = world; first = false; }
            else        b.Encapsulate(world);
        }

        return first ? new Bounds(Vector3.zero, Vector3.one) : b;
    }

    // 로컬 Bounds → 월드 Bounds 변환 (8개 꼭짓점 변환 후 재계산)
    private Bounds TransformBounds(Bounds local, Matrix4x4 m)
    {
        Vector3 c = local.center;
        Vector3 e = local.extents;

        Vector3[] corners = new Vector3[8];
        for (int i = 0; i < 8; i++)
        {
            corners[i] = m.MultiplyPoint3x4(c + new Vector3(
                (i & 1) == 0 ? -e.x : e.x,
                (i & 2) == 0 ? -e.y : e.y,
                (i & 4) == 0 ? -e.z : e.z));
        }

        Bounds result = new Bounds(corners[0], Vector3.zero);
        for (int i = 1; i < 8; i++) result.Encapsulate(corners[i]);
        return result;
    }

    // 모델 크기에 맞게 카메라 거리 자동 계산
    private void AutoFitDistance()
    {
        if (_prefab == null) return;
        Bounds b    = GetMeshBounds(_prefab);
        float  size = b.size.magnitude;
        _distance = size / (2f * Mathf.Tan(_fov * 0.5f * Mathf.Deg2Rad)) * 1.2f;
        Repaint();
    }
}
