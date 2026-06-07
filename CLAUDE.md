# Claude Code 규칙

## 언어 설정
- 사용자에게 권한을 묻는 질문(파일 생성, 수정, 실행 등)은 반드시 **한글**로 작성한다.
- 예: "SpawnCube.cs 파일을 생성할까요?", "이 파일을 수정해도 될까요?" 등

## 폴더 구조 규칙
- 씬 파일(.unity)은 `Assets/0.Scene/` 폴더에 저장한다.
- 에셋(모델, 텍스처, 사운드 등)은 `Assets/1.Assets/` 폴더에 저장한다.
- 스크립트 파일(.cs)은 `Assets/2.Scripts/` 폴더에 저장한다.
- 애니메이터 컨트롤러(.controller)는 `Assets/3.Animation/` 폴더에 저장한다.

## 스크립트 작성 규칙
- 하나의 스크립트에 모든 기능을 넣지 않는다. 기능별로 스크립트를 분리한다.
- 인스턴스화되는 매니저 스크립트는 앞에 `Mgr_` 접두사를 붙인다.
  - 예: `Mgr_Camera.cs`, `Mgr_Game.cs`, `Mgr_Sound.cs`
- 일반 기능 스크립트는 역할명 그대로 작성한다.
  - 예: `PlayerMovement.cs`, `ZombieAI.cs`, `PlayerStats.cs`
- 열거형(enum)은 앞에 `E` 접두사를 붙인다.
  - 예: `EMoveType`, `EControlType`, `EGameState`
