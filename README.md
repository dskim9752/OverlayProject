# OverlayProject
Unity OverlayProject





 생성된 파일 구조

  Scripts/Overlay/
  ├── Common/
  │   ├── IOverlayController.cs      # 플랫폼 공통 인터페이스
  │   ├── OverlayManager.cs          # 통합 관리자 (싱글톤)
  │   └── OverlaySettings.cs         # ScriptableObject 설정
  ├── Windows/
  │   ├── WindowsAPI.cs              # user32.dll, dwmapi.dll P/Invoke
  │   └── TransparentWindowController.cs  # Windows 오버레이 구현
  └── Mac/
      ├── MacOverlayController.cs    # Mac 플레이스홀더 + 구현 계획
      └── MacOverlayPlugin.mm.txt    # Native Plugin 예제 코드

  Windows 구현 기능

  | 기능            | 설명                              |
  |-----------------|-----------------------------------|
  | 투명 창         | 배경 색상(검정)을 투명하게 렌더링 |
  | 클릭 통과       | 마우스 이벤트가 아래 창으로 전달  |
  | 항상 위에       | 다른 창 위에 표시                 |
  | 작업표시줄 숨김 | 트레이 앱처럼 동작                |
  | 드래그 이동     | Alt+클릭으로 창 이동              |
  | 위치 기억       | 세션 간 위치 저장/복원            |

  핫키 (기본값)

  - F1 - 클릭 통과 토글
  - F2 - 항상 위에 토글
  - F3 - 오버레이/일반 창 토글
  - Alt+드래그 - 창 이동

  Unity 설정 필수사항

  Player Settings:
  1. Resolution and Presentation > Use DXGI Flip Model Swapchain = OFF
  2. Resolution and Presentation > Fullscreen Mode = Windowed
  3. Main Camera > Clear Flags = Solid Color, Background = Black (0,0,0,0)

  사용법

  1. 빈 GameObject 생성
  2. OverlayManager 컴포넌트 추가
  3. Inspector에서 설정 조정
  4. 빌드 후 실행

  Mac 구현 계획

  MacOverlayController.cs와 MacOverlayPlugin.mm.txt에 상세 계획이 있습니다:
  - NSWindow API 사용
  - Native .bundle 플러그인 필요
  - Objective-C++ 코드 예제 포함

───────────────────────────────────────────────