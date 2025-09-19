# 위험도 게이지 시스템 사용 가이드

새로운 **위험도 게이지 시스템**이 기존 HP 시스템을 대체했습니다.

## 🎯 시스템 개요

### ⚠️ 위험도 게이지 시스템
- **HP 대신 위험도**가 증가하는 시스템
- **초당 5씩** 위험도 증가
- **100 이상**이 되면 플레이어 사망
- UI에서는 **100으로 클램프**되어 표시
- 사망 시 **가장 가까운 활성화된 Flag**에서 리스폰

### 🚨 추가 시스템
- **DangerWarningSystem**: 75% 이상 시 경고, 90% 이상 시 치명적 경고
- **GameStateManager**: 게임 상태, 사망 통계, 일시정지 관리

## 🔧 설정 방법

### 1️⃣ 자동 설정 (권장)
1. **Scene에 SystemTransitionManager 추가**:
   ```
   Hierarchy → Create Empty → "SystemManager"
   SystemManager에 SystemTransitionManager 스크립트 추가
   ```
2. **Play 시 자동으로 시스템 전환됨**

### 2️⃣ 수동 설정
1. **Player에 DangerGaugeSystem 추가**:
   - Player GameObject에 `DangerGaugeSystem` 스크립트 추가

2. **UI 교체**:
   - 기존 HealthUI 대신 `DangerUI` 사용
   - Canvas의 Health Slider에 `DangerUI` 스크립트 추가

3. **경고 시스템 추가** (선택사항):
   - Canvas에 `DangerWarningSystem` 추가
   - Warning Overlay (Image)와 Warning Text (TextMeshPro) 연결

4. **게임 상태 관리** (선택사항):
   - 빈 GameObject에 `GameStateManager` 추가

## 📱 UI 구성

### DangerUI 설정
```
Canvas
└── DangerSlider (Slider)
    ├── Background
    ├── Fill Area
    │   └── Fill (Image) ← 여기에 dangerFill 연결
    └── Handle Slide Area
        └── Handle

DangerText (TextMeshPro) ← 별도 Text 오브젝트
```

### UI 컴포넌트 연결
- **Danger Text**: TextMeshPro 컴포넌트
- **Danger Slider**: Slider 컴포넌트 자기 자신
- **Danger Fill**: Slider의 Fill 이미지

## ⚙️ DangerGaugeSystem 설정

### 기본 설정
- **Max Danger**: 100 (기본값)
- **Danger Increase Rate**: 5 (초당 증가량)
- **Use Flag System**: true (Flag 시스템과 연동)

### 사망 설정
- **Death Effect**: 사망 시 재생할 이펙트
- **Death Particle Effect**: 파티클 이펙트
- **Death Effect Duration**: 이펙트 지속 시간 (2초)

### 리스폰 설정
- **Respawn Delay**: 리스폰 대기 시간 (2초)
- **Use Flag System**: Flag 시스템 사용 여부

## 🎨 시각적 효과

### 위험도에 따른 색상 변화
- **0-25%**: 🟢 초록색 (안전)
- **25-50%**: 🟡 노란색 (주의)
- **50-75%**: 🟠 주황색 (위험)
- **75-100%**: 🔴 빨간색 (극도 위험)

### 특수 효과
- **90% 이상**: 빠른 깜빡임 효과
- **100% 도달**: 사망 이펙트 재생

## 🚩 Flag 시스템 연동

### 리스폰 우선순위
1. **가장 가까운 활성화된 Flag**
2. **수동 설정된 Respawn Point**
3. **첫 번째 Flag (메인 Flag)**
4. **원점 (0,0,0)**

### Flag 요구사항
- Flag 오브젝트에 `Flag` 스크립트 필요
- 최소 1개의 활성화된 Flag 필요

## 🎮 게임플레이

### 위험도 증가 조건
- **안전지대(Flag)를 벗어났을 때** 위험도 증가 시작
- **안전지대에 들어오면** 위험도 증가 중단

### 사망 과정
1. 위험도 100 도달
2. 플레이어 제어 비활성화
3. 사망 이펙트 재생 (2초)
4. 가장 가까운 Flag로 리스폰
5. 위험도 0으로 초기화
6. 플레이어 제어 복구

## 🛠️ 개발자 옵션

### SystemTransitionManager 설정
- **Use Danger Gauge System**: 위험도 시스템 사용
- **Disable HP System**: 기존 HP 시스템 비활성화
- **Disable HP Drain System**: HPDrainSystem 비활성화
- **Auto Setup On Start**: 시작 시 자동 설정

### 디버그 명령어
- **DangerGaugeSystem.ForceRespawn()**: 강제 리스폰
- **DangerGaugeSystem.ResetDanger()**: 위험도 초기화
- **DangerGaugeSystem.GetDangerInfo()**: 현재 상태 정보

## 🔄 시스템 전환

### HP → 위험도 시스템
```csharp
// 기존 HP 시스템 비활성화
Health healthComponent = player.GetComponent<Health>();
healthComponent.enabled = false;

// 위험도 시스템 활성화
DangerGaugeSystem dangerSystem = player.AddComponent<DangerGaugeSystem>();
```

### 이벤트 시스템
```csharp
// 위험도 변화 이벤트
GameEvents.OnDangerChanged += UpdateDangerUI;

// 기존 HP 이벤트는 자동으로 비활성화됨
```

## ⚡ 성능 최적화

### 이펙트 충돌 방지
- CharacterMove의 `deadEffect`와 별도로 작동
- DangerGaugeSystem 전용 이펙트 사용
- 중복 재생 방지 로직 내장

### 메모리 관리
- 이펙트 자동 제거 (2초 후)
- 불필요한 컴포넌트 비활성화
- 이벤트 자동 해제

이제 플레이어는 HP가 아닌 **위험도**를 관리해야 하며, 안전지대를 벗어나면 점점 위험해지다가 100에 도달하면 사망하여 가장 가까운 Flag에서 리스폰됩니다! 🎮