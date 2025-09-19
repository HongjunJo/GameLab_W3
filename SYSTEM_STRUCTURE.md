# 📋 시스템 구조 요약

## 🗂️ 폴더 구조
```
Assets/Script/
├── Data/                    # ScriptableObject 데이터
│   ├── MineralData.cs      # 자원 데이터 정의
│   └── BuildingRecipe.cs   # 건물 레시피 정의
├── Manager/                 # 싱글톤 매니저들
│   ├── ResourceManager.cs  # 자원 관리
│   ├── PowerManager.cs     # 전력 관리
│   └── GameEvents.cs       # 이벤트 시스템
├── Survival/               # 생존 시스템
│   ├── Health.cs          # 체력 시스템
│   ├── SafeZone.cs        # 안전 지대
│   ├── PlayerStatus.cs    # 플레이어 상태
│   └── HPDrainSystem.cs   # 체력 감소 시스템
├── Interaction/           # 상호작용 시스템
│   ├── IInteractable.cs   # 상호작용 인터페이스
│   ├── Interactor.cs      # 상호작용 수행자
│   └── ResourceSource.cs # 자원 소스
├── Building/              # 건물 시스템
│   ├── Mine.cs           # 광산 (자원 생산)
│   ├── PowerGenerator.cs # 발전기 (전력 생산)
│   └── Flag.cs           # 깃발 (안전지대)
└── UI/                   # UI 시스템 (TextMeshPro)
    ├── ResourceUI.cs     # 자원 표시
    ├── PowerUI.cs        # 전력 표시
    ├── HealthUI.cs       # 체력 표시
    └── InteractionUI.cs  # 상호작용 안내
```

## 🔗 시스템 연결도

### 📊 데이터 계층
- **MineralData**: 자원의 기본 정보 (이름, 색상, 스택 크기)
- **BuildingRecipe**: 건물 건설에 필요한 자원 비용

### 🎛️ 매니저 계층
- **ResourceManager**: 
  - 전역 자원 관리
  - 자원 변경 이벤트 발생
- **PowerManager**: 
  - 전력 생산/소비 관리
  - 전력 변경 이벤트 발생
- **GameEvents**: 
  - 전역 이벤트 시스템
  - UI와 시스템 간 통신

### 🎮 게임플레이 계층
- **Health**: 플레이어 체력 관리
- **SafeZone**: 체력 회복 지역
- **PlayerStatus**: 플레이어 상태 통합 관리
- **HPDrainSystem**: 지속적인 체력 감소

### 🤝 상호작용 계층
- **IInteractable**: 상호작용 가능한 오브젝트 인터페이스
- **Interactor**: 플레이어의 상호작용 처리
- **ResourceSource**: 채집 가능한 자원 오브젝트

### 🏗️ 건물 계층
- **Mine**: 전력으로 자원 생산
- **PowerGenerator**: 자원으로 전력 생산  
- **Flag**: 안전지대 생성

### 🖥️ UI 계층 (TextMeshPro 기반)
- **ResourceUI**: 보유 자원 표시
- **PowerUI**: 전력 상태 표시 (색상으로 상태 구분)
- **HealthUI**: 체력 바와 수치 표시
- **InteractionUI**: 상호작용 가능 알림

## 🎯 핵심 디자인 패턴

### 1️⃣ ScriptableObject 패턴
- **장점**: 에디터에서 쉽게 데이터 수정 가능
- **사용**: MineralData, BuildingRecipe
- **확장성**: 새로운 자원/건물 쉽게 추가

### 2️⃣ Singleton 패턴
- **장점**: 전역 접근, 인스턴스 유일성 보장
- **사용**: ResourceManager, PowerManager
- **주의**: 의존성 최소화 위해 이벤트 시스템 활용

### 3️⃣ Event-Driven 패턴
- **장점**: 시스템 간 느슨한 결합
- **사용**: GameEvents를 통한 UI 업데이트
- **확장성**: 새로운 시스템 쉽게 연결

### 4️⃣ Interface 패턴
- **장점**: 다형성, 코드 재사용성
- **사용**: IInteractable 인터페이스
- **확장성**: 새로운 상호작용 오브젝트 쉽게 추가

### 5️⃣ Component 패턴
- **장점**: 모듈화, 재사용성
- **사용**: Unity MonoBehaviour 시스템
- **확장성**: 조합으로 복잡한 기능 구현

### 6️⃣ Material-Based Visual 패턴
- **장점**: 가벼운 렌더링, 쉬운 상태 표시
- **사용**: 건물과 자원의 시각적 상태 변화
- **확장성**: 새로운 Material로 다양한 상태 표현

## 🔄 주요 데이터 흐름

### 자원 채집 흐름
1. **Interactor** → **ResourceSource** 클릭
2. **ResourceSource** → **ResourceManager**에 자원 추가
3. **ResourceManager** → **GameEvents**로 변경 알림
4. **ResourceUI** → 자원 표시 업데이트

### 건물 건설 흐름
1. **Interactor** → **Building** 상호작용
2. **Building** → **ResourceManager**에서 비용 확인
3. **비용 충족시** → 건물 활성화 + Material 변경
4. **생산 시스템** → 코루틴으로 지속 생산

### 전력 시스템 흐름
1. **PowerGenerator** → **PowerManager**에 전력 공급
2. **Mine** → **PowerManager**에서 전력 소비
3. **PowerManager** → 전력 균형 계산
4. **PowerUI** → 전력 상태 색상으로 표시

### 생존 시스템 흐름
1. **HPDrainSystem** → 주기적으로 체력 감소
2. **SafeZone** → 범위 내에서 체력 회복
3. **Health** → **GameEvents**로 변경 알림
4. **HealthUI** → 체력 바와 색상 업데이트

## 💡 확장 아이디어

### 새로운 자원 타입
- **구리(Copper)**: 고급 건물 제작용
- **석탄(Coal)**: 발전기 연료
- **목재(Wood)**: 기본 건물 재료

### 새로운 건물 타입
- **저장고**: 자원 보관 용량 증가
- **연구소**: 새로운 기술 개발
- **수리소**: 건물 내구도 회복

### 새로운 게임 메커니즘
- **적 몬스터**: 야간에 등장하는 위협
- **날씨 시스템**: 생산량에 영향을 주는 환경
- **기술 트리**: 점진적인 게임 발전

이 구조는 모듈화와 확장성을 고려하여 설계되었으며, 각 시스템이 독립적으로 작동하면서도 효율적으로 연결됩니다. 🚀