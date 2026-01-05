# Unity Overlay Project

Windows용 투명 오버레이 창을 구현하는 Unity 프로젝트입니다.

## 주요 기능

| 기능 | 설명 |
|------|------|
| 투명 배경 | DWM 합성을 이용한 배경 투명화 |
| 클릭 통과 | 마우스 이벤트가 아래 창으로 전달 |
| 항상 위에 | 다른 창 위에 표시 |
| 작업표시줄 숨김 | 트레이 앱처럼 동작 |
| 드래그 이동 | Alt + 클릭으로 창 이동 |
| 위치 저장 | 세션 간 위치 저장/복원 |

## 핫키

| 키 | 기능 |
|----|------|
| F1 | 클릭 통과 토글 |
| F2 | 항상 위에 토글 |
| F3 | 오버레이/일반 창 토글 |
| Alt + 드래그 | 창 이동 |

## 필수 설정 (중요)

### 1. Graphics API 설정

> **Direct3D11을 사용해야 투명 창이 작동합니다.**

1. **Edit → Project Settings → Player** 열기
2. **Other Settings** 섹션 펼치기
3. **Auto Graphics API for Windows** 체크 해제
4. **Graphics APIs for Windows** 목록에서:
   - **Direct3D11**만 남기기
   - Direct3D12, Vulkan 등 제거

### 2. Player Settings

| 설정 | 값 | 경로 |
|------|-----|------|
| Use DXGI Flip Model Swapchain | OFF | Player → Resolution and Presentation |
| Fullscreen Mode | Windowed | Player → Resolution and Presentation |
| Preserve Framebuffer Alpha | ON | Player → Other Settings |

### 3. Camera 설정

| 설정 | 값 |
|------|-----|
| Clear Flags | Solid Color |
| Background | Black (R:0, G:0, B:0, A:0) |

## 프로젝트 구조

```
Assets/Scripts/Overlay/
├── Common/
│   ├── IOverlayController.cs       # 플랫폼 공통 인터페이스
│   ├── OverlayManager.cs           # 통합 관리자 (싱글톤)
│   └── OverlaySettings.cs          # ScriptableObject 설정
├── Windows/
│   ├── WindowsAPI.cs               # user32.dll, dwmapi.dll P/Invoke
│   └── TransparentWindowController.cs  # Windows 오버레이 구현
└── Mac/
    ├── MacOverlayController.cs     # Mac 플레이스홀더
    └── MacOverlayPlugin.mm.txt     # Native Plugin 예제 코드
```

## 사용법

1. 빈 GameObject 생성
2. `OverlayManager` 컴포넌트 추가
3. Inspector에서 설정 조정
4. **스탠드얼론 빌드 후 실행** (에디터에서는 작동하지 않음)

## 동작 원리

### Windows (DWM 합성 방식)

1. `DwmExtendFrameIntoClientArea`로 창 전체를 DWM 프레임으로 확장
2. Camera 배경색을 검은색 + 알파 0으로 설정
3. DWM이 검은색(0,0,0) + 알파 0 픽셀을 자동으로 투명 처리
4. Unity의 `preserveFramebufferAlpha` 설정으로 알파 채널 보존

### 주의사항

- **Unity 에디터에서는 투명 창이 작동하지 않습니다.** 반드시 빌드 후 테스트하세요.
- Direct3D12, Vulkan에서는 투명 창이 작동하지 않을 수 있습니다.
- Mac 지원은 아직 구현되지 않았습니다.

## 트러블슈팅

| 문제 | 해결 방법 |
|------|----------|
| 배경이 검은색으로 보임 | Graphics API를 Direct3D11로 설정 |
| 창이 화면에 안 보임 | 저장된 위치가 화면 밖일 수 있음. PlayerPrefs 삭제 |
| 에디터에서 안 됨 | 정상. 빌드 후 테스트 필요 |
