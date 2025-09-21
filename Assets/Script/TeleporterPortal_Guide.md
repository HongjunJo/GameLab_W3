# 🌀 텔레포터 & 포탈 시스템 가이드

## 📋 **시스템 개요**

### 🚀 **SimpleTeleporter (W키 자동 텔레포트)**
- **사용법**: W키 자동 감지
- **조건**: 광산 건설 상태 확인 가능
- **용도**: 특정 조건을 만족했을 때 빠른 이동

### 🌀 **InteractionPortal (E키 상호작용 포탈)**  
- **사용법**: E키 상호작용 필요
- **조건**: 조건 확인 없이 항상 사용 가능
- **용도**: 돌아오기 또는 양방향 이동

---

## 🛠️ **SimpleTeleporter 설정**

### **컴포넌트 추가**
1. Empty GameObject 생성 → 이름: `"MineToExploration"`
2. **SimpleTeleporter** 스크립트 추가
3. **Box Collider 2D** 추가 (Is Trigger ✅)

### **Inspector 설정**
```
📦 텔레포트 설정:
- Teleport Target: 목적지 Transform
- Teleport Key: W
- Interaction Range: 3.0

🎨 페이드 효과:
- Fade Time: 0.5
- Fade Color: Black

💰 비용 (선택사항):
- Required Mineral: (없음)
- Required Amount: 0

⚙️ 조건 확인 (선택사항):
- Required Mine: Mine 오브젝트 드래그 ✅
- Check Mine Active: ✅ 체크
```

### **특징**
- ✅ **광산 건설 확인**: 광산이 건설되고 활성화된 경우에만 사용 가능
- ❌ **광산 미건설**: "광산이 건설되지 않았거나 비활성화 상태입니다" 메시지
- 🔥 **자동 감지**: 범위 안에서 W키만 누르면 즉시 이동

---

## 🌀 **InteractionPortal 설정**

### **컴포넌트 추가**
1. Empty GameObject 생성 → 이름: `"ExplorationToBase"`
2. **InteractionPortal** 스크립트 추가
3. **Box Collider 2D** 추가 (Is Trigger ✅)

### **Inspector 설정**
```
🌀 포탈 설정:
- Teleport Target: 목적지 Transform
- Interaction Key: E
- Interaction Range: 3.0

🎨 페이드 효과:
- Fade Time: 0.5
- Fade Color: Black

💰 비용 (선택사항):
- Required Mineral: (없음)
- Required Amount: 0

📝 포탈 정보:
- Portal Name: "귀환 포탈"
- Destination Name: "메인 베이스"
```

### **특징**
- ✅ **항상 사용 가능**: 조건 확인 없음
- 🔄 **양방향 가능**: 두 개의 포탈로 왕복 이동 구성 가능
- 💬 **상호작용 UI**: E키 상호작용 시스템과 연동
- 📝 **커스텀 메시지**: 포탈 이름과 목적지 이름 설정 가능

---

## 🎮 **권장 사용 시나리오**

### **🏗️ 탐험 지역으로 가기**
```
광산 → 탐험 지역
📍 위치: 광산 근처
🎮 시스템: SimpleTeleporter (W키)
⚙️ 조건: 광산 건설 완료 필요
💡 용도: 광산을 건설한 플레이어만 탐험 가능
```

### **🏠 메인 베이스로 돌아오기**
```
탐험 지역 → 메인 베이스
📍 위치: 탐험 지역
🎮 시스템: InteractionPortal (E키)
⚙️ 조건: 없음 (항상 사용 가능)
💡 용도: 언제든지 안전하게 귀환
```

---

## 🔧 **고급 설정**

### **💰 비용 설정 (두 시스템 공통)**
```csharp
// 철광석 5개가 필요한 포탈
Required Mineral: IronOre
Required Amount: 5
```

### **🎨 페이드 커스터마이징**
```csharp
// 빠른 페이드
Fade Time: 0.2f

// 느린 페이드
Fade Time: 1.0f

// 다른 색상 페이드
Fade Color: White (흰색 페이드)
```

### **⚙️ 조건 커스터마이징 (SimpleTeleporter만)**
```csharp
// 광산 확인 비활성화
Check Mine Active: ❌ 체크 해제

// 다른 광산 참조
Required Mine: 다른 Mine 오브젝트
```

---

## 🐛 **트러블슈팅**

### **❌ "광산이 건설되지 않았거나 비활성화 상태입니다"**
- 광산이 건설되었는지 확인
- Mine 스크립트의 `Is Built`와 `Is Active`가 true인지 확인
- SimpleTeleporter의 `Required Mine`이 올바른 광산을 참조하는지 확인

### **❌ "페이드가 보이지 않음"**
- Canvas가 제대로 생성되고 있는지 Console 로그 확인
- 카메라의 Culling Mask 설정 확인
- UI Layer 설정 확인

### **❌ "이동 후 플레이어 조작 안됨"**
- CharacterMove 컴포넌트가 활성화되었는지 확인
- MovementLimiter.Instance.CharacterCanMove가 true인지 확인
- Console에서 "활성화" 로그 메시지 확인

---

## 📝 **사용 예시**

### **기본 설정 (광산 → 탐험지역)**
1. **SimpleTeleporter** 생성 및 설정
2. **Required Mine**에 광산 오브젝트 드래그
3. **Teleport Target**에 탐험지역 위치 설정
4. 광산 건설 후 W키로 이동 가능

### **돌아오기 설정 (탐험지역 → 베이스)**
1. **InteractionPortal** 생성 및 설정  
2. **Portal Name**: "귀환 포탈"
3. **Destination Name**: "메인 베이스"
4. **Teleport Target**에 베이스 위치 설정
5. E키 상호작용으로 언제든 귀환 가능

---

## 🎯 **핵심 차이점**

| 특징 | SimpleTeleporter | InteractionPortal |
|------|------------------|-------------------|
| **활성화 키** | W (자동 감지) | E (상호작용) |
| **조건 확인** | 광산 상태 확인 가능 | 조건 확인 없음 |
| **UI 연동** | 없음 | 상호작용 UI 연동 |
| **사용 상황** | 조건부 이동 | 항상 사용 가능 |
| **표시 방식** | 범위 진입 시 로그 | 상호작용 텍스트 |

이제 두 가지 시스템을 조합하여 완전한 탐험 시스템을 구축할 수 있습니다!