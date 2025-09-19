# 🎮 2D 생존 탐험 게임 시스템

## 📖 게임 소개
이 게임은 3일차 MVP(최소 기능 제품)으로 만들어진5. **Box Collider** 제거:
   - Cube의 **Box Collider** 컴포넌트 **Remove** (Player의 Box Collider 2D와 충돌 방지)

---

### 2️⃣ 카메라 시스템 설정

#### 🎯 카메라 시스템 특징
- **부드러운 플레이어 추적**: 플레이어를 자연스럽게 따라감
- **Look-Ahead 시스템**: 이동 방향으로 미리 보기
- **맵 경계 제한**: 카메라가 맵 밖으로 나가지 않음
- **Input System 연동**: PlayerController와 완벽 통합

#### 🔧 카메라 컨트롤러 설정하기
**Main Camera 오브젝트를 선택한 상태에서:**

1. **Add Component** 버튼 클릭
2. **CameraController** 스크립트 추가

#### 🔧 CameraController 설정
**CameraController 스크립트 설정:**

1. **Target Settings**:
   - **Player Transform**: Player 오브젝트를 드래그 & 드롭
   
2. **Camera Settings**:
   - **Look Ahead Distance**: 4.0 (앞쪽 미리보기 거리)
   - **Smooth Time**: 0.3 (카메라 부드러움 정도, 낮을수록 빠름)

3. **Map Boundaries** (맵 경계 설정):
   - **Map Min Bounds**: (0, 0) - 맵 왼쪽 아래 경계
   - **Map Max Bounds**: (100, 50) - 맵 오른쪽 위 경계
   - 🎯 **게임 맵 크기에 맞게 조정하세요**

4. **Debug Info**:
   - **Show Bounds**: ✅ (Scene 뷰에서 경계선 표시)

#### 🎮 카메라 작동 원리
1. **방향 감지**: PlayerController의 Input System에서 이동 방향 감지
2. **Look-Ahead**: 이동 방향으로 카메라가 약간 앞쪽을 미리 보여줌
3. **부드러운 이동**: SmoothDamp를 사용해 자연스럽고 프레임 독립적인 이동
4. **경계 제한**: 설정한 맵 경계를 벗어나지 않도록 제한

#### 🔧 런타임 카메라 제어 (코드에서)
```csharp
// 맵 경계 변경
cameraController.SetMapBounds(new Vector2(0, 0), new Vector2(200, 100));

// 카메라 설정 조정
cameraController.SetCameraSettings(lookAhead: 6f, smoothTime: 0.2f);

// 플레이어 위치로 즉시 이동 (씬 전환 시)
cameraController.SnapToPlayer();
```

---

### 3️⃣ 자원 시스템 설정 탐험 게임입니다. 플레이어는 자원을 채집하고, 건물을 건설하며, 생존하는 것이 목표입니다.

## 🎯 주요 기능
- **자원 채집**: 맵에 있는 자원을 클릭하여 수집
- **건물 건설**: 광산, 발전기, 깃발 설치
- **생존 시스템**: 체력 관리와 안전 지대 시스템
- **전력 관리**: 발전기로 전력 생산하고 광산에 공급

**🎨 비주얼 시스템**: 이 게임은 **2D 횡스크롤 스타일**이지만, **3D 프리미티브 오브젝트**(큐브, 구체, 실린더, 캡슐)를 사용하여 **즉시 보이고 쉽게 구분할 수 있는** 시각적 시스템을 사용합니다. Material 색상 변경으로 다양한 상태를 표현합니다.

---

## 🛠️ 게임 오브젝트 설정 가이드

### 1️⃣ 플레이어 캐릭터 설정

#### 🔧 기본 플레이어 오브젝트 만들기
1. **Hierarchy 창**에서 우클릭 → **Create Empty** 선택
2. 이름을 **"Player"**로 변경
3. **Position**을 (0, 0, 0)으로 설정

#### 🔧 플레이어 컴포넌트 추가하기
**Player 오브젝트를 선택한 상태에서:**

1. **Add Component** 버튼 클릭
2. **Box Collider 2D** 추가 (플레이어 충돌 감지용)
3. **Rigidbody 2D** 추가 (물리 시뮬레이션용)
   - **Gravity Scale**: 1로 설정 (2D 횡스크롤 게임이므로 중력 적용)
4. **PlayerController** 스크립트 추가 (Input System 기반 이동 및 점프)
5. **Interactor** 스크립트 추가 (상호작용 기능)
6. **Health** 스크립트 추가 (체력 시스템)
7. **PlayerStatus** 스크립트 추가 (플레이어 상태 관리)

#### 🔧 PlayerController 설정
**PlayerController 스크립트 설정:**

1. **Movement Settings**:
   - **Move Speed**: 5.0 (이동 속도)
   - **Jump Force**: 10.0 (점프 힘)

2. **Ground Check** (땅 감지 설정):
   - **Ground Check Point**: 자동으로 생성됨 (수동 설정 가능)
   - **Ground Check Radius**: 0.2 (땅 감지 범위)
   - **Ground Layer Mask**: Default 선택 (땅으로 사용할 레이어)

3. **Visual Settings**:
   - **Player Z Position**: -1.0 (플레이어가 건물보다 앞에 보이도록)

4. **Player 태그 설정** (카메라용):
   - **Player 오브젝트 선택** → **Tag** → **Player** 선택

💡 **Z축 위치 자동 유지**: 플레이어가 건물과 겹쳐도 항상 앞에 보이도록 Z축 위치가 자동으로 유지됩니다.

#### 🎮 **플레이어 조작법**
- **A, D 키** 또는 **←, → 화살표**: 좌우 이동
- **Space 키**: 점프 (땅에 있을 때만)
- **마우스 클릭**: 자원 채집 및 건물 상호작용

#### 🔧 플레이어 비주얼 만들기 (간단한 3D 큐브 방식)
**Player 오브젝트 아래에 3D 큐브 추가:**

1. Player 오브젝트 우클릭 → **3D Object** → **Cube** 선택
2. 이름을 **"PlayerVisual"**로 변경
3. **Transform** 설정:
   - **Position**: (0, 0, 0)
   - **Scale**: (0.8, 0.8, 0.8) - 플레이어 크기에 맞게 조정
4. **Mesh Renderer** 색상 변경 (Unity 6000.x 버전):
   - **Materials** → **Element 0** 드롭다운 클릭
   - **Create New Material** 선택 (또는 기존 Material 드래그)
   - 새로 생성된 Material을 클릭하여 선택
   - **Inspector 하단**에서 **Surface Inputs** 섹션 찾기
   - **Base Map** 옆의 **색상 사각형** (기본값: 흰색) 클릭
   - **Color Picker**에서 **파란색**으로 설정
5. **Box Collider** 제거:
   - Cube의 **Box Collider** 컴포넌트 **Remove** (Player의 Box Collider 2D와 충돌 방지)

---

### 2️⃣ 자원 오브젝트 설정

#### 🔧 자원 데이터 만들기
1. **Assets** 폴더에서 우클릭 → **Create** → **Game Data** → **Mineral Data**
2. 이름을 **"IronOre"** (철광석)로 설정
3. **Inspector**에서 설정:
   - **Mineral Name**: "철 광석"
   - **Base Value**: 10
   - **Mineral Color**: 회색
   - **Icon**: (스프라이트는 사용하지 않으므로 비워둠)

#### 🔧 자원 오브젝트 만들기
1. **Hierarchy**에서 **Create Empty** → 이름을 **"IronResource"**로 설정
2. **Transform Position 설정**: 원하는 위치에 배치
   - 예시: **(5, 0, 0)** - 플레이어 오른쪽 5칸
   - 여러 개 만들 경우: **(3, 0, 0)**, **(7, 0, 0)**, **(12, 0, 0)** 등
3. **컴포넌트 추가**:
   - **Box Collider 2D** 추가
   - **Is Trigger** 체크 ✅
   - **ResourceSource** 스크립트 추가
4. **ResourceSource 스크립트 설정**:
   - **Mineral Data**: 위에서 만든 IronOre 드래그
   - **Amount**: 5 (채집할 수 있는 양)

#### 📍 **자원 배치 가이드 (맵 디자인)**
**권장 맵 레이아웃:**
```
[플레이어] ─── [자원들] ─── [광산위치] ─── [발전기위치] ─── [깃발위치]
   (0,0)      (3~12,0)      (15,0)         (20,0)         (25,0)
```

**자원 오브젝트 배치 예시:**
- **IronResource1**: Position **(3, 0, 0)**
- **IronResource2**: Position **(7, 0, 0)**  
- **IronResource3**: Position **(12, 0, 0)**
- **IronResource4**: Position **(16, 0, 0)**

**배치 팁:**
- 🎯 **플레이어 주변에 몰아두기**: 초반 자원 수집 쉽게
- 🚶 **이동 경로상에 배치**: 자연스러운 수집 흐름
- 📏 **2~5칸 간격**: 너무 밀집되지 않게

#### 🔧 자원 비주얼 만들기 (간단한 3D 구체 방식)
**IronResource 오브젝트 아래에:**

1. 우클릭 → **3D Object** → **Sphere** 선택
2. 이름을 **"ResourceVisual"**로 변경
3. **Transform** 설정:
   - **Position**: (0, 0, 0)
   - **Scale**: (0.5, 0.5, 0.5) - 자원 크기
4. **Mesh Renderer** 색상 변경 (Unity 6000.x 버전):
   - **Materials** → **Element 0** 드롭다운 클릭
   - **Create New Material** 선택 (또는 기존 Material 드래그)
   - 새로 생성된 Material을 클릭하여 선택
   - **Inspector 하단**에서 **Surface Inputs** 섹션 찾기
   - **Base Map** 옆의 **색상 사각형** (기본값: 흰색) 클릭
   - **Color Picker**에서 **회색** (철광석)으로 설정
5. **Sphere Collider** 제거:
   - **Sphere Collider** 컴포넌트 **Remove** (IronResource의 Box Collider 2D 사용)

---

### 3️⃣ 광산 건물 설정

#### 🔧 광산 오브젝트 만들기
1. **Create Empty** → 이름을 **"Mine"**으로 설정
2. **Transform Position 설정**: 자원 근처에 배치
   - 예시: **(15, 0, 0)** - 자원들 오른쪽에 배치
3. **컴포넌트 추가**:
   - **Box Collider 2D** (Is Trigger ✅)
   - **Mine** 스크립트 추가

#### 🔧 Mine 스크립트 설정
**Inspector에서 Mine 컴포넌트:**

1. **Recipe**: 
   - **Assets**에서 우클릭 → **Create** → **Game Data** → **Building Recipe**
   - 이름을 **"MineRecipe"**로 설정
   - **Recipe Name**: "광산"
   - **Resource Costs**: 철광석 20개 설정 (Size를 1로 늘리고 Element 0에 IronOre와 20 입력)
2. **Production**:
   - **Produced Mineral**: IronOre 선택
   - **Production Amount**: 2
   - **Production Time**: 5.0 (5초마다 생산)
   - **Electricity Cost**: 1

#### 🔧 광산 비주얼 만들기 (간단한 3D 큐브 방식)
**Mine 오브젝트 아래에:**

1. 우클릭 → **3D Object** → **Cube** 선택
2. 이름을 **"MineVisual"**로 변경
3. **Transform** 설정:
   - **Position**: (0, 0, 0)
   - **Scale**: (1.5, 1.5, 1.5) - 광산 크기
4. **Material 2개 만들기** (Unity 6000.x 버전):
   - **Assets**에서 우클릭 → **Create** → **Material**
   - **"MineInactive"** Material 생성 → **Surface Inputs** → **Base Map** 색상을 **빨간색**으로 설정
   - **"MineActive"** Material 생성 → **Surface Inputs** → **Base Map** 색상을 **초록색**으로 설정
5. **Mine 스크립트에서**:
   - **Object Renderer**: MineVisual의 **Mesh Renderer** 컴포넌트 드래그
   - **Inactive Material**: MineInactive 드래그
   - **Active Material**: MineActive 드래그
6. **Box Collider** 제거:
   - MineVisual의 **Box Collider** 컴포넌트 **Remove**

---

### 4️⃣ 발전기 설정

#### 🔧 발전기 오브젝트 만들기
1. **Create Empty** → **"PowerGenerator"**
2. **Transform Position 설정**: 광산 근처에 배치
   - 예시: **(20, 0, 0)** - 광산 오른쪽 5칸
3. **컴포넌트**:
   - **Box Collider 2D** (Is Trigger ✅)
   - **PowerGenerator** 스크립트

#### 🔧 PowerGenerator 설정
1. **Upgrade Recipes**: 배열 크기를 3으로 설정하고 각각 다른 레벨의 레시피 생성
   - **Level 1 Recipe**: 철광석 30개
   - **Level 2 Recipe**: 철광석 50개  
   - **Level 3 Recipe**: 철광석 100개
2. **Max Level**: 3 (최대 업그레이드 레벨)
3. **Material 설정**:
   - **Level Materials**: 배열 크기 4로 설정
   - **Element 0**: GeneratorLevel0 (회색)
   - **Element 1**: GeneratorLevel1 (주황색)
   - **Element 2**: GeneratorLevel2 (노란색)  
   - **Element 3**: GeneratorLevel3 (밝은 노란색)

#### 🔧 발전기 비주얼 만들기 (간단한 3D 실린더 방식)
**PowerGenerator 오브젝트 아래에:**

1. 우클릭 → **3D Object** → **Cylinder** 선택
2. 이름을 **"GeneratorVisual"**로 변경
3. **Transform** 설정:
   - **Position**: (0, 0, 0)
   - **Scale**: (1.2, 1.2, 1.2) - 발전기 크기
4. **Material 4개 만들기** (레벨별, Unity 6000.x 버전):
   - **Assets**에서 우클릭 → **Create** → **Material**로 4개 생성
   - **"GeneratorLevel0"**: **Surface Inputs** → **Base Map** 색상을 **회색**으로 설정
   - **"GeneratorLevel1"**: **Surface Inputs** → **Base Map** 색상을 **주황색**으로 설정
   - **"GeneratorLevel2"**: **Surface Inputs** → **Base Map** 색상을 **노란색**으로 설정
   - **"GeneratorLevel3"**: **Surface Inputs** → **Base Map** 색상을 **밝은 노란색**으로 설정
5. **PowerGenerator 스크립트에서**:
   - **Object Renderer**: GeneratorVisual의 **Mesh Renderer** 컴포넌트 드래그
   - **Level Materials**: 배열 크기 4로 설정하고 위 Material들 순서대로 드래그
6. **Capsule Collider** 제거:
   - GeneratorVisual의 **Capsule Collider** 컴포넌트 **Remove**

---

### 5️⃣ 깃발 (안전지대) 설정

#### 🔧 깃발 오브젝트 만들기
1. **Create Empty** → **"Flag"**
2. **Transform Position 설정**: 맵 끝이나 안전한 위치에 배치
   - 시작 깃발: **(0, 0, 0)** - 플레이어 시작 위치
   - 중간 깃발: **(25, 0, 0)** - 발전기 오른쪽 5칸
   - 끝 깃발: **(35, 0, 0)** - 맵 끝
3. **컴포넌트**:
   - **Box Collider 2D** (Is Trigger ✅)
   - **Flag** 스크립트
   - **SafeZone** 스크립트

#### 🔧 Flag & SafeZone 설정

**Flag 컴포넌트 설정:**
1. **Activation Recipe**: 
   - **Assets**에서 우클릭 → **Create** → **Game Data** → **Building Recipe**
   - 이름을 **"FlagRecipe"**로 설정
   - **Recipe Name**: "깃발"
   - **Resource Costs**: 철광석 15개 설정
2. **Flag Settings**:
   - **Is Active**: false (건설 후 활성화됨)
   - **Is Main Flag**: true (시작 지점 깃발인 경우만 체크)
3. **Safe Zone**:
   - **Safe Zone Radius**: 3.0 (안전지대 범위)
4. **Progression** (게임 진행 시스템):
   - **Next Flag**: 다음 단계 깃발 오브젝트 드래그 (선택사항)
   - **Player Spawn Point**: 플레이어 리스폰 위치 Transform 드래그 (선택사항)
5. **Visual**:
   - **Flag Light**: 깃발 조명 효과 GameObject (선택사항)

**SafeZone 컴포넌트 설정:**
1. **Safe Zone Settings**:
   - **Is Active**: true (안전지대 활성화 여부)
   - **Safe Zone Size**: (6.0, 6.0) (사각형 안전지대 크기 - Width, Height)
2. **Visual Settings**:
   - **Safe Zone Color**: Green (범위 표시 색상)
   - **Show Gizmo**: true (에디터에서 범위 표시)
   - **Show Runtime Visual**: true ✨ **NEW!** (게임 실행 중 범위 표시)
   - **Safe Zone Material**: 범위 라인 렌더링용 머티리얼 (선택사항)
   - **Enable Merging**: true 🆕 **NEW!** (SafeZone 합치기 활성화)
   - **Merge Check Interval**: 1.0 (합치기 체크 간격, 초 단위)
3. **Effects** (선택사항):
   - **Enter Effect**: 안전지대 입장 시 이펙트
   - **Exit Effect**: 안전지대 퇴장 시 이펙트

💡 **Box Collider 2D 사용**: Circle Collider 대신 Box Collider 2D를 사용하여 명확한 사각형 범위를 제공합니다.

🎮 **실시간 범위 표시**: **Show Runtime Visual**을 활성화하면 게임 플레이 중에도 SafeZone의 범위를 시각적으로 확인할 수 있습니다!
- ✅ **활성화된 안전지대**: 초록색 테두리로 표시
- ❌ **비활성화된 안전지대**: 🚫 **범위 완전히 숨김** (더 이상 빨간색으로 표시하지 않음)
- 🔧 **자동 LineRenderer**: SafeZone이 자동으로 LineRenderer 컴포넌트를 추가하여 범위를 그립니다

🔗 **SafeZone 스마트 합치기**: **Enable Merging**을 활성화하면 겹치는 SafeZone들이 하나의 큰 안전지대처럼 표시됩니다!
- 🎯 **스마트 감지**: 주기적으로 겹치는 SafeZone을 자동 감지
- 🖼️ **통합 시각화**: 겹치는 영역들을 하나의 큰 사각형으로 표시
- 💫 **동적 업데이트**: SafeZone 활성화/비활성화 시 실시간으로 합쳐진 영역 업데이트
- 🎨 **차별화된 표시**: 합쳐진 영역은 더 두껍고 투명한 라인으로 표시
- 🔒 **활성화 필터링**: 비활성화된 SafeZone은 합치기에서 자동 제외

#### 🔧 깃발 비주얼 만들기 (간단한 3D 캡슐 방식)
**Flag 오브젝트 아래에:**

1. 우클릭 → **3D Object** → **Capsule** 선택
2. 이름을 **"FlagVisual"**로 변경
3. **Transform** 설정:
   - **Position**: (0, 0, 0)
   - **Scale**: (0.5, 1.5, 0.5) - 깃발 모양 (얇고 높게)
4. **Material 2개 만들기** (Unity 6000.x 버전):
   - **Assets**에서 우클릭 → **Create** → **Material**로 2개 생성
   - **"FlagInactive"**: **Surface Inputs** → **Base Map** 색상을 **회색**으로 설정
   - **"FlagActive"**: **Surface Inputs** → **Base Map** 색상을 **파란색**으로 설정
5. **Flag 스크립트에서**:
   - **Object Renderer**: FlagVisual의 **Mesh Renderer** 컴포넌트 드래그
   - **Inactive Material**: FlagInactive 드래그
   - **Active Material**: FlagActive 드래그
6. **Capsule Collider** 제거:
   - FlagVisual의 **Capsule Collider** 컴포넌트 **Remove**

#### 🔧 깃발 조명 효과 만들기 (선택사항)
**Flag 오브젝트 아래에:**

1. 우클릭 → **Light** → **2D Light** 선택
2. 이름을 **"FlagLight"**로 변경
3. **Light 2D** 컴포넌트 설정:
   - **Color**: 파란색
   - **Intensity**: 0.5
   - **Radius**: 2.0
4. 초기에는 비활성화 상태로 설정
7. **Flag 스크립트에서**:
   - **Activation Recipe**: FlagRecipe 드래그
   - **Object Renderer**: FlagVisual의 **Sprite Renderer** 컴포넌트 드래그
   - **Inactive Material**: FlagInactive 드래그
   - **Active Material**: FlagActive 드래그
   - **Flag Light**: FlagLight GameObject 드래그 (선택사항)
   - **Safe Zone**: SafeZone 컴포넌트 (자동으로 연결됨)

#### 🔧 깃발 시스템 추가 설정 (선택사항)

**게임 진행 시스템 (다단계 맵용):**
1. **Next Flag 설정**:
   - 다음 스테이지의 Flag 오브젝트를 **Next Flag** 필드에 드래그
   - 현재 깃발 활성화 시 다음 깃발로의 진행 가능
2. **Player Spawn Point 설정**:
   - 빈 GameObject 생성, 이름을 **"SpawnPoint"**로 설정
   - 플레이어가 리스폰될 위치에 배치
   - Flag의 **Player Spawn Point** 필드에 드래그

**시작 깃발 설정 (첫 번째 깃발):**
1. **Is Main Flag**: ✅ 체크
2. **Is Active**: ✅ 체크 (처음부터 활성화)
3. **Activation Recipe**: None (건설 없이 활성화)

**안전지대 고급 설정:**
1. **Circle Collider 2D** 자동 추가됨:
   - **Is Trigger**: ✅ (자동 설정)
   - **Radius**: Safe Zone Radius와 동일 (자동 설정)
2. **HP 회복 시스템**:
   - PlayerStatus 컴포넌트가 안전지대 상태를 감지
   - 안전지대 내에서 자동 체력 회복
   - 안전지대 밖에서 체력 감소

**깃발 기능 요약:**
- 🏠 **안전지대**: 체력 회복 및 안전한 휴식 공간
- 🎯 **진행 관리**: 다음 지역으로의 이동 체크포인트
- 💾 **리스폰 지점**: 플레이어 사망 시 되돌아올 위치
- 🔄 **게임 상태**: 활성화를 통한 게임 진행도 추적

---

### 6️⃣ UI 시스템 설정

#### 🔧 캔버스 만들기
1. **Hierarchy** 우클릭 → **UI** → **Canvas**
2. **Canvas Scaler** 설정:
   - **UI Scale Mode**: Scale With Screen Size
   - **Reference Resolution**: 1920 x 1080

#### 🔧 자원 UI 만들기
1. **Canvas 아래에** 우클릭 → **UI** → **Text - TextMeshPro**
2. 이름을 **"ResourceDisplay"**로 변경
3. **ResourceUI** 스크립트 추가
4. **ResourceUI 설정**:
   - **Resource Text**: 방금 만든 TextMeshPro 드래그

#### 🔧 체력 UI 만들기
1. **UI** → **Slider** 추가, 이름을 **"HealthBar"**
2. **HealthUI** 스크립트 추가
3. **설정**:
   - **Health Slider**: Slider 드래그
   - **Health Text**: Slider 아래 TextMeshPro 추가해서 드래그

#### 🔧 전력 UI 만들기
1. **UI** → **Text - TextMeshPro**, 이름을 **"PowerDisplay"**
2. **PowerUI** 스크립트 추가
3. **Power Text**에 TextMeshPro 드래그

#### 🔧 상호작용 UI 만들기
1. **UI** → **Panel**, 이름을 **"InteractionPanel"**
2. Panel 아래에 **Text - TextMeshPro** 추가
3. **InteractionUI** 스크립트를 Panel에 추가
4. **설정**:
   - **Interaction Text**: TextMeshPro 드래그
   - **Interaction Panel**: Panel 자기 자신 드래그

---

### 7️⃣ 매니저 시스템 설정
매니저 시스템은 게임의 핵심 데이터를 관리하고 모든 시스템 간의 소통을 담당합니다. 3개의 주요 매니저가 있습니다.

#### 🔧 게임 매니저 오브젝트 만들기
1. **Hierarchy**에서 우클릭 → **Create Empty** 선택
2. 이름을 **"GameManager"**로 변경
3. **Transform Position**을 (0, 0, 0)으로 설정
4. **다음 3개의 스크립트를 모두 추가합니다**:

#### 🔧 ResourceManager 설정 (자원 관리)
**GameManager 오브젝트에 ResourceManager 스크립트 추가 후:**

**ResourceManager의 역할:**
- 🗃️ **자원 저장소**: 플레이어가 수집한 모든 자원을 중앙 관리
- 🔄 **자원 거래**: 건물 건설, 업그레이드 시 자원 차감/추가
- 📊 **실시간 추적**: 현재 보유 자원량을 UI에 실시간 표시
- 🌐 **전역 접근**: 게임 내 모든 시스템에서 자원 정보 조회 가능

**Inspector 설정:**
1. **Debug - Current Resources**: 
   - 현재 보유 자원을 확인할 수 있는 디버그 정보
   - 게임 실행 중 실시간으로 업데이트됨
   - 직접 수정하지 말고 확인용으로만 사용

**ResourceManager는 싱글톤 패턴**으로 설계되어 별도 설정이 필요하지 않습니다.

#### 🔧 PowerManager 설정 (전력 관리)
**GameManager 오브젝트에 PowerManager 스크립트 추가 후:**

**PowerManager의 역할:**
- ⚡ **전력 저장**: 발전기가 생산한 전력을 저장
- 🏭 **전력 분배**: 광산 등 전력이 필요한 건물에 공급
- 📈 **용량 관리**: 최대 전력량과 현재 전력량 추적
- 🔋 **효율성 모니터링**: 전력 생산/소비 균형 관리

**Inspector 설정:**
1. **Power Settings**:
   - **Max Power**: 100 (최대 전력 저장량)
   - **Current Power**: 100 (시작 시 전력량 - 자동으로 Max Power와 동일하게 설정됨)
2. **Debug Info**:
   - **Power Usage History**: 전력 사용 기록 (자동 업데이트)

**전력 시스템 작동 방식:**
- 🔌 **발전기**: 레벨에 따라 전력 생산량 증가
- 🏭 **광산**: 작동 시 지속적으로 전력 소모
- ⚠️ **전력 부족**: 전력이 0이 되면 광산 작동 중단

#### 🔧 HPDrainSystem 설정 (체력 감소 시스템)
**GameManager 오브젝트에 HPDrainSystem 스크립트 추가 후:**

**HPDrainSystem의 역할:**
- 💔 **생존 압박**: 안전지대 밖에서 지속적인 체력 감소로 생존 긴장감 조성
- 🛡️ **안전지대 효과**: 깃발 주변에서는 체력 감소 중단
- ⏱️ **유예 시간**: 안전지대를 벗어나도 즉시 데미지 받지 않음
- 🎮 **게임 밸런스**: 탐험과 안전의 균형을 통한 전략적 플레이

**Inspector 설정:**

1. **Target Player**:
   - **Player Object**: Player 오브젝트를 드래그 & 드롭
   - ⚠️ **중요**: Player 오브젝트에 Health와 PlayerStatus 컴포넌트가 반드시 있어야 함
   - 💡 **자동 검색**: Player Object가 비어있으면 "Player" 태그를 가진 오브젝트를 자동으로 찾음

2. **Drain Settings**:
   - **Drain Amount**: 5.0 (한 번에 감소하는 체력량)
   - **Drain Interval**: 1.0 (체력 감소 간격, 초 단위)
   - **Enable Drain**: ✅ 체크 (체력 감소 시스템 활성화)

3. **Grace Period**:
   - **Grace Period**: 3.0 (안전지대를 벗어난 후 데미지 시작까지의 유예시간)

4. **Debug Info** (자동 업데이트):
   - **Time Since Left Safe Zone**: 안전지대를 벗어난 후 경과 시간
   - **Is Draining**: 현재 체력이 감소 중인지 표시

**체력 감소 시스템 작동 방식:**
1. 🏠 **안전지대 내**: 체력 감소 중단, 안전한 상태
2. 🚪 **안전지대 이탈**: 3초 유예 시간 시작
3. ⏰ **유예 시간 경과**: 1초마다 5씩 체력 감소 시작
4. 🔄 **안전지대 복귀**: 즉시 체력 감소 중단

#### 🔧 매니저 시스템 검증하기

**게임 실행 후 확인사항:**

1. **ResourceManager 작동 확인**:
   - 자원 채집 시 Debug-Current Resources에서 수량 증가 확인
   - 건물 건설 시 필요 자원 차감 확인

2. **PowerManager 작동 확인**:
   - 발전기 건설 시 Max Power 증가 확인
   - 광산 가동 시 Current Power 감소 확인

3. **HPDrainSystem 작동 확인**:
   - 안전지대(깃발) 밖에서 체력 감소 확인
   - 안전지대 진입 시 체력 감소 중단 확인
   - Debug Info에서 실시간 상태 확인

#### 🛠️ 매니저 시스템 문제 해결

**흔한 오류와 해결방법:**

1. **"Missing Reference" 오류**:
   - HPDrainSystem의 Player Object가 올바르게 드래그되었는지 확인
   - Player에 Health, PlayerStatus 컴포넌트가 있는지 확인

2. **자원이 저장되지 않는 문제**:
   - ResourceManager가 GameManager 오브젝트에 있는지 확인

#### 🔧 최신 업데이트 사항 (v2.0)

**SafeZone 시스템 개선:**
- ✅ **Box Collider 2D 사용**: Circle Collider 대신 명확한 사각형 범위 제공
- ✅ **비활성화 깃발 문제 해결**: 비활성화된 깃발에서 HP 드레인 타이머 초기화 문제 수정
- ✅ **더 정확한 안전지대 감지**: 깃발 범위 내에서 정확한 안전지대 판정

**플레이어 시스템 개선:**
- ✅ **Z축 위치 자동 관리**: 플레이어가 건물보다 항상 앞에 표시되도록 자동 위치 조정
- ✅ **시각적 일관성**: 건물과 겹쳐도 플레이어가 뒤로 밀려나지 않음

**변경된 설정값:**
- SafeZone Size: (6.0, 6.0) - 사각형 크기
- Player Z Position: -1.0 - 항상 앞에 표시
   - ResourceSource의 Mineral Data가 설정되었는지 확인

3. **전력 시스템이 작동하지 않는 문제**:
   - PowerManager가 씬에 하나만 있는지 확인
   - 발전기와 광산의 레시피가 올바르게 설정되었는지 확인

**매니저 시스템은 게임의 두뇌 역할**을 하므로, 모든 설정이 정확해야 게임이 원활하게 작동합니다! 🧠✨

#### 🚨 플레이어 문제 해결

**플레이어가 떨어지는 문제:**

1. **Ground Layer 설정 확인**:
   - PlayerController의 **Ground Layer Mask**가 올바르게 설정되었는지 확인
   - 땅으로 사용하는 오브젝트들이 동일한 Layer에 있는지 확인

2. **Rigidbody2D 설정 확인**:
   - **Gravity Scale**: 1 (너무 높으면 떨어짐)
   - **Mass**: 1 (기본값)
   - **Linear Drag**: 0 (기본값)
   - **Angular Drag**: 0.05 (기본값)

3. **Collider 설정 확인**:
   - Player의 **Box Collider 2D**가 적절한 크기인지 확인
   - **Is Trigger**: false (체크 해제 상태)

4. **Ground Check 위치 확인**:
   - **Ground Check Point**가 플레이어 발 아래에 위치하는지 확인
   - **Ground Check Radius**: 0.2 (너무 작으면 땅 감지 실패)

5. **Z축 위치 문제**:
   - **Player Z Position**: -1.0으로 설정
   - **Sorting Order**: 10으로 설정하여 다른 오브젝트보다 앞에 표시

💡 **디버깅 팁**: Console 창에서 "Player falling!" 메시지가 나오면 위 설정들을 다시 확인하세요.

---

### 8️⃣ 카메라 시스템 설정 (Hollow Knight 스타일)

#### 🔧 메인 카메라 설정
1. **Main Camera 오브젝트 선택** (Hierarchy에서 Main Camera 클릭)
2. **Transform Position 설정**: (0, 0, -10)
3. **Camera 컴포넌트 설정**:
   - **Projection**: Orthographic ✅
   - **Size**: 5 (화면에 보이는 범위, 취향에 따라 조정)
   - **Clipping Planes**: Near 0.3, Far 1000

#### 🔧 CameraController 스크립트 추가
**Main Camera 오브젝트에 CameraController 스크립트 추가 후:**

**CameraController의 역할:**
- 🎯 **부드러운 추적**: 플레이어를 자연스럽게 따라가는 카메라
- 👀 **Look-Ahead**: 플레이어 이동 방향을 미리 보여주는 시야
- 🗺️ **맵 경계 제한**: 카메라가 맵 밖으로 나가지 않도록 제한
- 🎮 **Hollow Knight 스타일**: 부드럽고 예측 가능한 카메라 워크

#### 🔧 CameraController 설정

**Inspector에서 CameraController 컴포넌트:**

1. **Target**:
   - **Player Transform**: Player 오브젝트를 드래그
   - ⚠️ **자동 감지**: Player 태그가 있으면 자동으로 찾아서 설정됨

2. **Camera Settings**:
   - **Camera Follow Speed**: 5.0 (카메라 따라가는 속도, 높을수록 빠름)
   - **Look Ahead Distance**: 4.0 (플레이어 진행 방향으로 얼마나 앞을 볼지)

3. **Map Boundaries** (맵 크기에 맞게 설정):
   - **Map Min Bounds**: (-10, -5) - 맵 왼쪽 아래 모서리
   - **Map Max Bounds**: (50, 10) - 맵 오른쪽 위 모서리

4. **Debug Info** (자동 업데이트):
   - **Last Move Direction**: 플레이어 마지막 이동 방향 (-1: 왼쪽, 1: 오른쪽)
   - **Show Bounds**: ✅ 체크 (에디터에서 맵 경계선 표시)

#### 🎮 **카메라 작동 방식**

1. **🎯 부드러운 추적**:
   - 플레이어 위치로 즉시 이동하지 않고 부드럽게 따라감
   - Camera Follow Speed 값으로 추적 속도 조절

2. **👀 Look-Ahead 시스템**:
   - 플레이어가 오른쪽으로 이동 → 카메라가 오른쪽을 더 많이 보여줌
   - 플레이어가 왼쪽으로 이동 → 카메라가 왼쪽을 더 많이 보여줌
   - 플레이어가 멈춰도 마지막 이동 방향 유지

3. **🗺️ 맵 경계 제한**:
   - 카메라가 맵 밖의 빈 공간을 보여주지 않음
   - Map Min/Max Bounds로 카메라 이동 범위 제한

#### 🔧 **맵 경계 설정 가이드**

**우리 게임의 맵 레이아웃:**
```
시작깃발(0,0) → 자원들(3~16,0) → 광산(15,0) → 발전기(20,0) → 중간깃발(25,0) → 끝깃발(35,0)
```

**권장 카메라 경계 설정:**
- **Map Min Bounds**: (-5, -3) - 시작 지점 왼쪽 여유분
- **Map Max Bounds**: (40, 8) - 끝 지점 오른쪽 + 위아래 여유분

#### 🎨 **에디터 디버그 기능**

**Scene 뷰에서 확인 가능:**
- 🔴 **빨간 박스**: 맵 경계 (Map Bounds)
- 🟡 **노란 박스**: 현재 카메라 뷰포트
- 🟢 **초록 구체**: Look-Ahead 목표 위치

#### 🔧 **플레이어 태그 설정** (필수)

**Player 오브젝트 태그 설정:**
1. **Player 오브젝트 선택**
2. **Inspector 상단**에서 **Tag** 드롭다운 클릭
3. **Player** 선택 (없으면 Add Tag로 생성)
4. 이렇게 하면 CameraController가 자동으로 플레이어를 찾음

#### 🛠️ **카메라 시스템 문제 해결**

**흔한 오류와 해결방법:**

1. **카메라가 플레이어를 따라가지 않음**:
   - Player Transform이 드래그되었는지 확인
   - Player 태그가 설정되었는지 확인

2. **카메라가 너무 빠르거나 느림**:
   - Camera Follow Speed 값 조정 (3~8 사이 권장)

3. **Look-Ahead가 너무 과하거나 부족함**:
   - Look Ahead Distance 값 조정 (2~6 사이 권장)

4. **카메라가 맵 밖을 보여줌**:
   - Map Min/Max Bounds가 올바르게 설정되었는지 확인
   - Scene 뷰에서 빨간 박스(맵 경계) 확인

#### 🎯 **카메라 설정 요약**

**기본 설정값:**
- **Camera Follow Speed**: 5.0 (부드러운 추적)
- **Look Ahead Distance**: 4.0 (적당한 예측 시야)
- **Map Min Bounds**: (-5, -3) (시작 지점 여유분)
- **Map Max Bounds**: (40, 8) (끝 지점 여유분)

**카메라 워크 특징:**
- 🎮 **Hollow Knight 스타일**: 부드럽고 예측 가능한 움직임
- 📱 **프레임률 독립적**: 어떤 FPS에서도 동일한 속도
- 🔧 **실시간 조정**: 게임 실행 중에도 Inspector에서 값 조정 가능

---

## 🗺️ **완전한 맵 배치 가이드**

### 🎯 **전체 맵 레이아웃**
```
[시작깃발] [플레이어] [자원1] [자원2] [자원3] [광산] [발전기] [중간깃발] [끝깃발]
   (0,0)     (0,0)     (3,0)   (7,0)  (12,0)  (15,0)  (20,0)    (25,0)    (35,0)
```

### 📍 **단계별 배치 순서**

1. **🚩 시작 깃발 배치**: Position **(0, 0, 0)**
   - **Is Main Flag**: ✅ 체크
   - **Is Active**: ✅ 체크

2. **🧑 플레이어 배치**: Position **(0, 0, 0)** (깃발과 같은 위치)

3. **🪨 자원들 배치**: 
   - **IronResource1**: **(3, 0, 0)**
   - **IronResource2**: **(7, 0, 0)**
   - **IronResource3**: **(12, 0, 0)**
   - **IronResource4**: **(16, 0, 0)**

4. **⛏️ 광산 배치**: Position **(15, 0, 0)**
   - **Is Built**: ❌ 체크 해제 (건설 전 상태)

5. **⚡ 발전기 배치**: Position **(20, 0, 0)**
   - **Is Built**: ✅ 체크 (이미 건설됨)

6. **🚩 중간 깃발 배치**: Position **(25, 0, 0)**
   - **Is Active**: ❌ 체크 해제 (활성화 전 상태)

7. **🚩 끝 깃발 배치**: Position **(35, 0, 0)** (선택사항)

### 🎮 **게임 플레이 흐름**
1. **시작**: 시작 깃발 안전지대에서 시작
2. **자원 수집**: 오른쪽으로 이동하며 철광석 수집
3. **발전기 업그레이드**: 전력 생산량 증가
4. **광산 건설**: 자동 자원 생산 시작
5. **중간 깃발 활성화**: 새로운 안전지대 확보
6. **다음 구역 진행**: 더 많은 자원과 도전

### 💡 **배치 팁**
- **Y축은 0으로 고정**: 2D 횡스크롤이므로 높이 변화 없음
- **X축 간격 3~5**: 너무 밀집되지 않게
- **카메라 범위 고려**: 너무 멀리 떨어뜨리지 않기
- **진행 방향**: 왼쪽에서 오른쪽으로 자연스럽게

---

## 🎮 게임플레이 가이드

### 기본 조작법
- **A/D 키** 또는 **←/→ 화살표**: 플레이어 좌우 이동
- **Space 키**: 점프 (땅에 있을 때만 가능)
- **마우스 클릭**: 자원 채집 및 건물과 상호작용
- **카메라**: 플레이어를 자동으로 부드럽게 추적

### 게임 진행 순서
1. **자원 채집**: 맵의 철광석을 클릭해서 수집
2. **발전기 건설**: 전력 생산을 위해 먼저 건설
3. **광산 건설**: 자동으로 자원을 생산
4. **깃발 건설**: 안전지대에서 체력 회복

### 생존 팁
- 체력이 떨어지면 깃발 근처로 이동
- 전력이 부족하면 광산이 작동하지 않음
- 자원을 효율적으로 관리하세요

---

## ⚠️ 주의사항

### 필수 패키지
- **TextMeshPro**: Window → TextMeshPro → Import TMP Essential Resources

### 스크립트 오류 해결
1. **Missing Reference 오류**: Inspector에서 드래그 앤 드롭이 제대로 되었는지 확인
2. **NullReference 오류**: 매니저 오브젝트들이 씬에 있는지 확인
3. **Renderer 오류**: 각 건물의 **Object Renderer**에 **Mesh Renderer** 컴포넌트가 드래그되었는지 확인
4. **Collider 중복**: 3D 프리미티브의 3D Collider는 반드시 제거 (2D Collider만 사용)

### 비주얼 시스템 장점
- ✅ **즉시 보임**: 3D 프리미티브는 바로 보이고 Material 색상 변경 쉬움
- ✅ **디버깅 용이**: 왜 안 보이는지 고민할 필요 없음
- ✅ **빠른 제작**: Sprite 제작이나 복잡한 설정 불필요
- ✅ **명확한 구분**: 모양과 색상으로 오브젝트 종류 쉽게 구분

### 성능 최적화
- 자원 오브젝트는 10개 이하로 제한
- UI는 필요할 때만 업데이트
- 코루틴을 과도하게 사용하지 않기

---

## 🔧 확장 가능한 시스템

### 새로운 자원 추가하기
1. **MineralData** 새로 생성
2. **ResourceSource**에 적용
3. **광산 레시피**에 출력 자원으로 설정

### 새로운 건물 추가하기
1. **BuildingRecipe** 생성
2. **IInteractable** 인터페이스 구현
3. **3D 프리미티브 + Material** 기반 비주얼 설정

이 가이드를 따라하면 완전히 동작하는 2D 생존 탐험 게임을 만들 수 있습니다! 🎮✨

## 🎯 **비주얼 시스템 요약**

**오브젝트별 3D 프리미티브:**
- 🧑 **플레이어**: 파란색 **큐브** (0.8 크기)
- 🪨 **자원**: 회색 **구체** (0.5 크기)  
- ⛏️ **광산**: **큐브** (빨강→초록, 1.5 크기)
- ⚡ **발전기**: **실린더** (회색→노랑, 1.2 크기)
- 🚩 **깃발**: **캡슐** (회색→파랑, 얇고 높게)

**핵심 설정:**
- 3D 프리미티브의 **3D Collider 제거** 필수!
- **Mesh Renderer**를 스크립트에 드래그
- Material **Base Map** 색상으로 상태 표현

**카메라 시스템:**
- 📹 **Hollow Knight 스타일**: 부드러운 추적 + Look-Ahead
- 🎯 **자동 플레이어 추적**: Player 태그로 자동 감지
- 🗺️ **맵 경계 제한**: 카메라가 맵 밖으로 나가지 않음
- 🔧 **실시간 조정**: Inspector에서 실시간으로 설정 변경 가능

