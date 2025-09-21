# 🚀 SimpleTeleporter 사용법

## 📋 간단 설정 가이드

### 1. 텔레포트 목표 지점 만들기
1. **빈 오브젝트 생성**: `TeleportTarget`
2. **원하는 위치에 배치**: 예) Position (50, 10, 0)

### 2. 텔레포터 설정
1. **광산 오브젝트 선택** (또는 새 오브젝트 생성)
2. **SimpleTeleporter.cs 스크립트 추가**
3. **Collider2D 추가** (없다면)
   - BoxCollider2D 또는 CircleCollider2D
   - **Is Trigger 체크** ✅
4. **Inspector 설정**:
   ```
   텔레포트 설정:
   ├ Teleport Target: TeleportTarget 드래그
   ├ Teleport Key: W
   └ Interaction Range: 3
   
   페이드 효과:
   ├ Fade Time: 1
   └ Fade Color: Black
   
   비용 (선택사항):
   ├ Required Mineral: (없으면 무료)
   └ Required Amount: 0
   ```

## 🎮 사용법

### 플레이어 조작
```
1. 광산(텔레포터) 근처로 이동
2. "W" 키 입력
3. 화면이 검게 변함 (페이드 아웃)
4. 플레이어가 목표 지점으로 이동
5. 화면이 다시 보임 (페이드 인)
6. 텔레포트 완료!
```

### 콘솔 메시지
```
"[W] 키를 눌러 텔레포트 (범위 진입)" - 범위 안에 들어옴
"텔레포트 시작!" - W키 눌림
"텔레포트 완료!" - 이동 완료
"텔레포트 범위 이탈" - 범위에서 나감
```

## 🔧 추가 설정

### 비용 설정 (광물 소모)
1. **Required Mineral**: 철광석 등 선택
2. **Required Amount**: 5 (5개 소모)
3. 부족하면 텔레포트 불가

### 여러 텔레포터 만들기
```
Teleporter1 (광산) → Target1 (점프맵)
Teleporter2 (점프맵) → Target2 (광산) - 돌아오기용
```

## 🎯 완성된 결과

```
플레이어가 광산 근처에서 W키 누름
↓
화면이 1초간 검게 변함
↓
플레이어가 목표 지점으로 순간이동
↓
화면이 다시 정상으로 돌아옴
```

---

이제 광산에서 W키만 누르면 바로 텔레포트됩니다! 🚀