# Boss System Setup Guide

## 📋 Tổng quan hệ thống
Hệ thống Boss FSM với 2 Phase, UI components riêng biệt, và tích hợp với kĩ năng Fa.

## 🗂️ Cấu trúc folder hiện tại

```
Boss/
├── CoreSystem/          # Hệ thống cốt lõi
│   ├── BossConfig.cs           # Configuration ScriptableObject
│   ├── BossController.cs       # Controller chính
│   ├── BossEventSystem.cs      # Event management
│   ├── BossStateMachine.cs     # FSM và BossState base class
│   └── BossSubSystems.cs       # Health & Soul management
│
├── States/              # Tất cả các state của Boss
│   ├── Phase1/                 # States cho Phase 1
│   │   ├── IdleState.cs        # Đứng yên
│   │   ├── LureState.cs        # Tiến lại gần rồi rút lui
│   │   ├── MockState.cs        # Vặn vẹo và cười méo mó
│   │   └── DecoyState.cs       # Skill spawn bóng ảo
│   ├── Phase2/                 # States cho Phase 2
│   │   ├── AngryState.cs       # Di chuyển xoay quanh center
│   │   ├── FearZoneState.cs    # Skill tạo vùng tối
│   │   ├── ScreamState.cs      # Skill gây hiệu ứng screen
│   │   └── CookState.cs        # Khi boss bị defeat
│   └── Shared/                 # States dùng chung
│       ├── SoulState.cs        # Teleport và spawn soul
│       └── PhaseChangeState.cs # Chuyển phase
│
├── UI/                  # UI components riêng biệt
│   ├── BossHealthBar.cs        # Boss HP slider
│   ├── PlayerHealthBar.cs      # Player HP slider  
│   └── BossSkillCastBar.cs     # Skill cast progress
│
├── Behaviors/           # Behavior scripts cho entities
│   ├── DecoyBehavior.cs        # AI cho decoys
│   ├── SoulBehavior.cs         # AI cho souls
│   ├── FearBehaviour.cs        # Fear zone effects
│   └── MemoryFragmentBehavior.cs # Memory fragment
│
├── Testing/             # Scripts để test
│   ├── PlayerTestController.cs # Player movement & attack
│   └── FaSkillSimulator.cs     # Giả lập kĩ năng Fa
│
├── Integration/         # Tích hợp với hệ thống khác
│   ├── BossManager.cs          # Manager tổng
│   └── FaBossIntegration.cs    # Liên kết với Fa
│
└── README_Setup.md      # File hướng dẫn này
```

## 🚀 Setup nhanh

### 1. Tạo GameObject Boss trong Scene
```
1. Tạo Empty GameObject, đặt tên "Boss"
2. Add tag "Boss" cho GameObject này
3. Add các components sau:
   - NavMeshAgent
   - Animator (optional)
   - AudioSource (optional)
   - Collider (Box/Capsule)
   - BossController script (từ CoreSystem/)
```

### 2. Setup UI System
```
1. Tạo Canvas trong scene
2. Add các UI components:
   - BossHealthBar (từ UI/)
   - PlayerHealthBar (từ UI/)
   - BossSkillCastBar (từ UI/)
3. Assign references trong Inspector
```

### 3. Tạo Player Test Object
```
1. Tạo Empty GameObject, đặt tên "PlayerTest" 
2. Add tag "Player"
3. Add components:
   - CharacterController (auto-add)
   - PlayerTestController (từ Testing/)
4. Đặt ở vị trí phù hợp trong scene
```

### 4. Tạo BossConfig ScriptableObject
```
1. Click chuột phải trong Project
2. Create > [Create Menu cho BossConfig]
3. Điều chỉnh các thông số:
   - Max Health Per Phase: 3
   - Phase Transition Duration: 2f
   - Defeat Animation Duration: 3f
   - Skill Cast Times, Movement Speeds, etc.
```

### 5. Setup NavMesh
```
1. Tạo Plane làm ground
2. Window > AI > Navigation
3. Chọn ground object > Mark as Navigation Static
4. Bake NavMesh
5. Assign NavMesh Center cho Boss (Empty GameObject ở giữa area)
```

### 6. Setup Boss Manager (Optional)
```
1. Tạo Empty GameObject, đặt tên "BossManager"
2. Add BossManager script (từ Integration/)
3. Assign Boss reference trong Inspector
```

## 🎮 UI Components được tách riêng

### BossHealthBar.cs
- Boss HP slider ở top center
- Đổi màu theo phase (đỏ→tím)
- Smooth transitions

### PlayerHealthBar.cs  
- Player HP slider ở bottom center
- Text hiển thị "3/3"
- Damage effects

### BossSkillCastBar.cs
- Progress bar cho skill casting
- Countdown timer
- Auto show/hide

## 🎯 Controls cho testing

### PlayerTestController.cs
- **WASD**: Di chuyển
- **Space/LMB**: Tấn công
- **Mouse**: Camera control

### FaSkillSimulator.cs
- **Q**: Fa Radar Skill (Xóa Soul)
- **E**: Fa Second Skill  
- **R**: Fa Third Skill

## 🔧 Cấu hình Boss Behavior

### Phase 1 States Flow
```
IdleState → LureState → MockState → DecoyState (Skill)
↑                                        ↓
└── Hit Real Boss ←────── SoulState ←── Hit Fake Boss
```

### Phase 2 States Flow  
```
AngryState → FearZoneState (Skill) → ScreamState (Skill) → CookState
    ↑              ↓                      ↓
    └─── SoulState ←──────────────────────┘
```

### Skills với Cast Time
- **DecoyState**: Spawn 2 bóng ảo (1 thật 1 giả)
- **FearZoneState**: Tạo vùng tối dưới chân player
- **ScreamState**: Cơ hội duy nhất để damage boss trong Phase 2
- **SoulState**: Teleport + spawn souls

## 🎭 Behavior Systems

### DecoyBehavior.cs
- AI cho bóng ảo trong DecoyState
- Slow chase behavior
- Visual differences (real vs fake)

### SoulBehavior.cs
- AI cho soul entities
- Follow player behavior
- Floating animation

### FearBehaviour.cs
- Quản lý vùng fear zone
- Visual effects cho darkness
- Player detection

### MemoryFragmentBehavior.cs
- Behavior cho memory fragment
- Collection mechanics

## 📱 Integration với Fa Skills

### FaBossIntegration.cs
```csharp
// Liên kết với hệ thống Fa
public void OnFaRadarUsed() // Skill 1 - Xóa souls
public void OnFaSecondSkill() // Skill 2 - TBD
public void OnFaThirdSkill() // Skill 3 - TBD
```

### Event System
```csharp
// BossEventSystem.cs handles:
- Player attack events
- Fa skill usage events
- Phase change events
- Boss state transitions
```

## 🎨 Animation Setup (Khi có assets)

### Animation Parameters cần thiết
```
Phase 1:
- IdleAnimation (Trigger)
- LureAnimation (Trigger)  
- MockAnimation (Trigger)
- DecoyAnimation (Trigger)

Phase 2:
- AngryAnimation (Trigger)
- FearZoneAnimation (Trigger)
- ScreamAnimation (Trigger)
- CookAnimation (Trigger)

Shared:
- SoulAnimation (Trigger)
- PhaseChangeAnimation (Trigger)
```

### Integration trong States
```csharp
// Trong mỗi state Enter() method:
bossController.BossAnimator?.SetTrigger("StateName");
```

## 🐛 Troubleshooting

### UI không hiển thị
1. Check UI components được assign đúng
2. Ensure Canvas có proper sorting order
3. Verify script references

### Boss không di chuyển  
1. NavMesh đã bake chưa
2. NavMeshAgent enabled
3. NavMesh Center assigned

### States không transition
1. Check BossStateMachine initialization
2. Verify state conditions
3. Look at Debug.Log outputs

### Events không fire
1. BossEventSystem initialized
2. Event subscriptions correct
3. Unsubscribe in OnDestroy

## 🚀 Performance & Architecture

### Modular Design
- Mỗi system có thể test độc lập
- UI components tách biệt
- Behaviors có thể reuse

### Memory Management
- Auto cleanup decoys, souls
- Proper event unsubscription
- Efficient state transitions

### Extensibility
- Easy to add new states
- New behaviors can be plugged in
- UI system scalable

## 📝 Mở rộng hệ thống

### Thêm State mới
1. Tạo class trong States/Phase1 hoặc Phase2
2. Inherit từ BossState
3. Implement abstract methods
4. Add transition logic

### Thêm Behavior mới
1. Tạo script trong Behaviors/
2. Inherit từ MonoBehaviour
3. Add Initialize() method
4. Reference trong state tương ứng

### Thêm UI component
1. Tạo script trong UI/
2. Handle specific UI logic
3. Link với BossController events

## ✅ Testing Checklist

### Core Functionality
- [ ] Boss spawns correctly
- [ ] UI displays proper health/phase
- [ ] Player controls responsive
- [ ] Attack hit detection works

### Phase 1 Testing
- [ ] IdleState → random transitions
- [ ] LureState approach/retreat
- [ ] MockState visual/audio
- [ ] DecoyState spawn/interaction
- [ ] Hit real decoy → damage boss
- [ ] Hit fake decoy → damage player + soul

### Phase 2 Testing
- [ ] Phase transition smooth
- [ ] AngryState movement pattern
- [ ] FearZoneState creates dark area
- [ ] ScreamState damage window
- [ ] Boss defeat → memory fragment

### Integration Testing
- [ ] Fa skills remove souls
- [ ] Events fire correctly
- [ ] UI updates properly
- [ ] No memory leaks
- [ ] Performance stable

### Final Polish
- [ ] All animations connected
- [ ] Audio cues working
- [ ] Visual effects polished
- [ ] Debug logs cleaned up
