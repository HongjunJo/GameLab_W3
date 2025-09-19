# 📱 UI 시스템 간단 가이드

## 🎯 UI 컴포넌트 개요

이 게임의 UI는 **TextMeshPro** 기반으로 만들어진 간단하고 효율적인 시스템입니다.

## 🔧 UI 컴포넌트 설정

### 1️⃣ ResourceUI (자원 표시)
**용도**: 플레이어가 소유한 자원을 실시간으로 표시

**설정 방법**:
1. Canvas에 **TextMeshPro - Text (UI)** 추가
2. `ResourceUI` 스크립트 추가
3. **Resource Text** 필드에 TextMeshPro 드래그

**표시 내용**:
```
Resources:
철 광석: 25
구리: 10
```

---

### 2️⃣ PowerUI (전력 표시)
**용도**: 현재 전력 상태를 색상과 함께 표시

**설정 방법**:
1. Canvas에 **TextMeshPro - Text (UI)** 추가
2. `PowerUI` 스크립트 추가
3. **Power Text** 필드에 TextMeshPro 드래그

**색상 코딩**:
- 🟢 **초록색**: 전력 충분 (70% 이상)
- 🟡 **노란색**: 전력 부족 (30-70%)
- 🔴 **빨간색**: 전력 위험 (30% 미만)

**표시 내용**: `Power: 8/10`

---

### 3️⃣ HealthUI (체력 표시)
**용도**: 플레이어 체력을 텍스트와 슬라이더로 표시

**설정 방법**:
1. Canvas에 **Slider** 추가
2. Slider에 `HealthUI` 스크립트 추가
3. 필드 설정:
   - **Health Text**: TextMeshPro 컴포넌트
   - **Health Slider**: Slider 자기 자신
   - **Health Fill**: Slider의 Fill Area > Fill 이미지

**색상 변화**:
- 🟢 **초록색**: 건강 (60% 이상)
- 🟡 **노란색**: 주의 (30-60%)
- 🔴 **빨간색**: 위험 (30% 미만)

---

### 4️⃣ InteractionUI (상호작용 안내)
**용도**: 상호작용 가능한 오브젝트 근처에서 안내 메시지 표시

**설정 방법**:
1. Canvas에 **Panel** 추가
2. Panel 아래에 **TextMeshPro - Text (UI)** 추가
3. Panel에 `InteractionUI` 스크립트 추가
4. 필드 설정:
   - **Interaction Text**: TextMeshPro 컴포넌트
   - **Interaction Panel**: Panel 자기 자신

**특징**:
- 자동으로 싱글톤으로 관리됨
- 다른 스크립트에서 `InteractionUI.ShowMessage()`, `InteractionUI.HideMessage()` 호출 가능

---

## 🎨 UI 배치 권장사항

### 화면 레이아웃
```
┌─────────────────────────────────┐
│ Resources: 철광석: 25           │ ← ResourceUI (좌상단)
│                                 │
│                 Power: 8/10     │ ← PowerUI (우상단)
│                                 │
│                                 │
│                                 │
│                                 │
│ Press E to 광산 건설            │ ← InteractionUI (하단 중앙)
│ ███████████████████████ HP:80/100│ ← HealthUI (하단)
└─────────────────────────────────┘
```

### Canvas 설정
- **Render Mode**: Screen Space - Overlay
- **Canvas Scaler**: Scale With Screen Size
- **Reference Resolution**: 1920 x 1080

---

## 🔄 이벤트 시스템 연동

### 자동 업데이트
모든 UI는 **GameEvents**를 통해 자동으로 업데이트됩니다:

- `ResourceUI` ← `GameEvents.OnResourceChanged`
- `PowerUI` ← `GameEvents.OnPowerChanged`
- `HealthUI` ← `GameEvents.OnHealthChanged`
- `InteractionUI` ← Static 메서드로 직접 호출

### 수동 호출 방법
```csharp
// 상호작용 메시지 표시
InteractionUI.ShowMessage("Press E to collect");

// 상호작용 메시지 숨기기
InteractionUI.HideMessage();
```

---

## ⚠️ 주의사항

### TextMeshPro 설정
- **Font Asset**: 한글을 지원하는 폰트 사용 필요
- **Material Preset**: Default 사용 권장

### 성능 최적화
- UI는 변경될 때만 업데이트됨
- 불필요한 UI 컴포넌트는 비활성화

### 색상 일관성
- 초록: 안전/충분
- 노랑: 주의/부족  
- 빨강: 위험/부족

이 UI 시스템은 간단하지만 게임에 필요한 모든 정보를 효과적으로 전달합니다! 🎮