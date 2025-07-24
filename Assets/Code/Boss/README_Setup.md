# Boss FSM System - Complete Setup Guide

## 🎯 Tổng quan
Hướng dẫn chi tiết setup toàn bộ Boss FSM system từ A-Z, bao gồm:
- Boss Controller & State Machine
- Animation System Integration
- Fa Skills Integration
- UI System Setup
- Audio & Effects
- Testing & Debug

---

## 📁 Cấu trúc Files Boss System

```
Boss/
├── Core System/
│   ├── BossEventSystem.cs          # Event system riêng cho Boss
│   ├── BossConfig.cs               # Configuration ScriptableObject
│   ├── BossStateMachine.cs         # FSM core engine
│   ├── BossController.cs           # Controller chính
│   └── BossSubSystems.cs           # Health, Soul, UI managers
├── States/
│   ├── Phase1/
│   │   ├── IdleState.cs
│   │   ├── LureState.cs
│   │   ├── MockState.cs
│   │   └── DecoyState.cs
│   ├── Phase2/
│   │   ├── AngryState.cs
│   │   ├── FearZoneState.cs
│   │   ├── ScreamState.cs
│   │   └── CookState.cs
│   └── Shared/
│       ├── SoulState.cs
│       └── PhaseChangeState.cs
├── Behaviors/
│   ├── BossBehaviors.cs            # Decoy, Soul, FearZone behaviors
│   └── BossUIComponents.cs         # Health bars, cast bars
├── Integration/
│   ├── BossManager.cs              # Manager tổng thể
│   └── FaBossIntegration.cs        # Tích hợp với Fa system
└── Testing/                        # Test environment (đã có)
```

---

## 🚀 Setup Step-by-Step

### **Step 1: Tạo Boss Configuration**

1. **Tạo BossConfig ScriptableObject:**
   ```
   Right-click trong Project → Create → Boss → Boss Configuration
   Đặt tên: "MainBossConfig"
   ```

2. **Cấu hình thông số:**
   ```csharp
   General Settings:
   - Max Health Per Phase: 3
   - Move Speed: 5
   - Rotation Speed: 90

   Phase 1 Settings:
   - Idle Duration: 2s
   - Lure Duration: 3s
   - Mock Duration: 2s
   - Decoy Cast Time: 2s
   - Soul State Cast Time: 1.5s

   Phase 2 Settings:
   - Angry Move Duration: 5s
   - Fear Zone Cast Time: 2s
   - Scream Cast Time: 3s
   - Cook State Duration: 3s

   Soul Settings:
   - Max Souls: 2
   - Soul Move Speed: 4
   - Soul Spawn Radius: 15

   UI Settings:
   - Boss Health Position: (0, 0.8)
   - Player Health Position: (0, -0.8)
   - Health Bar Size: (300, 30)
   ```

### **Step 2: Setup Boss GameObject**

1. **Tạo Boss GameObject:**
   ```
   Create Empty → Đặt tên "Boss"
   Position: (0, 1, 10) hoặc vị trí mong muốn
   ```

2. **Add Components:**
   ```csharp
   - BossController (script chính)
   - NavMeshAgent (cho di chuyển)
   - Animator (cho animations)
   - AudioSource (cho âm thanh)
   - Collider (BoxCollider hoặc CapsuleCollider)
   ```

3. **Configure Components:**
   ```csharp
   NavMeshAgent:
   - Speed: 5
   - Angular Speed: 90
   - Stopping Distance: 0.1
   - Auto Braking: true

   Collider:
   - Is Trigger: false (để player có thể tấn công)
   - Size phù hợp với Boss model
   ```

4. **Assign References trong BossController:**
   ```csharp
   - Boss Config: Assign MainBossConfig đã tạo
   - Player: Assign Player GameObject
   - Nav Mesh Center: Tạo empty GameObject làm center cho Phase 2
   ```

### **Step 3: Setup Animation System**

1. **Tạo Animator Controller:**
   ```
   Right-click → Create → Animator Controller → "BossAnimator"
   Assign vào Boss GameObject
   ```

2. **Tạo Animation States:**
   ```csharp
   // Phase 1 Animations
   - Idle (bool: isIdle)
   - Lure (trigger: lureStart)
   - Mock (trigger: mockStart)
   - DecoyCast (trigger: decoyCast)
   - DecoyActive (trigger: decoyActive)
   - SoulCast (trigger: soulCast)
   - SoulActive (trigger: soulActive)

   // Phase 2 Animations  
   - Angry (bool: isAngry)
   - FearZoneCast (trigger: fearZoneCast)
   - FearZoneActive (trigger: fearZoneActive)
   - ScreamCast (trigger: screamCast)
   - ScreamActive (trigger: screamActive)
   - Cook (trigger: cook)

   // Transition
   - PhaseChange (trigger: phaseChange)
   ```

3. **Setup Animation Triggers:**
   ```csharp
   // Trong BossController.cs, uncomment animation code:
   public void PlayAnimation(string animationName)
   {
       Debug.Log($"[Boss Animation] Playing animation: {animationName}");
       
       // Enable khi có animations:
       if (animator != null)
       {
           animator.SetTrigger(animationName);
       }
   }
   ```

4. **Animation Parameters:**
   ```csharp
   // Thêm parameters trong Animator:
   - isIdle (Bool)
   - isAngry (Bool)
   - lureStart (Trigger)
   - mockStart (Trigger)
   - decoyCast (Trigger)
   - decoyActive (Trigger)
   - soulCast (Trigger)
   - soulActive (Trigger)
   - fearZoneCast (Trigger)
   - fearZoneActive (Trigger)
   - screamCast (Trigger)
   - screamActive (Trigger)
   - cook (Trigger)
   - phaseChange (Trigger)
   ```

### **Step 4: Setup Audio System**

1. **Chuẩn bị Audio Clips:**
   ```csharp
   Phase 1 Audio:
   - mockLaughSound (tiếng cười méo mó)
   - decoySpawnSound (âm thanh spawn decoy)
   - soulSpawnSound (âm thanh spawn soul)

   Phase 2 Audio:
   - screamSound (tiếng chê trách)
   - fearZoneSound (âm thanh vùng tối)
   - heartbeatSound (nhịp tim)

   General Audio:
   - phaseChangeSound (chuyển phase)
   - damageSound (nhận damage)
   - defeatSound (bị đánh bại)
   ```

2. **Assign Audio trong BossConfig:**
   ```csharp
   Audio Config section:
   - Assign tất cả audio clips
   - Set volume levels:
     * Master Volume: 1.0
     * SFX Volume: 0.8
     * Ambient Volume: 0.6
   ```

### **Step 5: Setup NavMesh**

1. **Bake NavMesh:**
   ```
   Window → AI → Navigation
   Select tất cả ground objects → Navigation Static ✓
   Bake tab → Bake
   ```

2. **Tạo NavMesh Center:**
   ```
   Create Empty → "NavMeshCenter"
   Position ở trung tâm khu vực boss di chuyển
   Assign vào BossController
   ```

### **Step 6: Setup UI System**

1. **Tạo UI Canvas:**
   ```
   Right-click → UI → Canvas
   Canvas Scaler → Scale With Screen Size
   Reference Resolution: 1920x1080
   ```

2. **Tạo Boss Health Bar:**
   ```
   Create → UI → Slider → "BossHealthBar"
   Position: Top center screen
   Assign BossHealthBar component
   ```

3. **Tạo Player Health Bar:**
   ```
   Create → UI → Slider → "PlayerHealthBar"  
   Position: Bottom center screen
   Assign PlayerHealthBar component
   ```

4. **Tạo Skill Cast Bar:**
   ```
   Create → UI → Slider → "SkillCastBar"
   Position: Below boss health bar
   Assign BossSkillCastBar component
   Hidden by default
   ```

### **Step 7: Setup BossManager**

1. **Tạo BossManager GameObject:**
   ```
   Create Empty → "BossManager"
   Add BossManager component
   ```

2. **Assign References:**
   ```csharp
   - Boss Controller: Boss GameObject
   - Boss Health Bar Prefab: UI prefab
   - Skill Cast Bar Prefab: UI prefab  
   - Player Health Bar Prefab: UI prefab
   - Player Max Health: 3
   ```

---

## 🔗 Fa Skills Integration

### **Step 1: Setup Fa Events**

1. **Trong Fa system, thêm integration:**
   ```csharp
   using Code.Boss;

   // Listen for boss requests
   private void Start()
   {
       FaBossIntegration.OnRequestFaSkill += HandleBossSkillRequest;
       FaBossIntegration.OnSoulCountChanged += HandleSoulCountChanged;
       FaBossIntegration.OnBossVulnerable += HandleBossVulnerability;
   }

   private void HandleBossSkillRequest(string skillName)
   {
       if (skillName == "Radar" && CanUseRadarSkill())
       {
           UseRadarSkill();
           FaBossIntegration.NotifyFaSkillUsed("Radar", true);
       }
   }

   private void HandleSoulCountChanged(int soulCount)
   {
       if (soulCount >= 2)
       {
           ShowRadarSkillSuggestion();
       }
   }

   private void HandleBossVulnerability(bool isVulnerable)
   {
       if (isVulnerable)
       {
           ShowAttackOpportunity();
       }
   }
   ```

### **Step 2: Fa Skills Implementation**

1. **Radar Skill (Destroy Souls):**
   ```csharp
   public void UseRadarSkill()
   {
       // Fa skill logic here
       // ...

       // Notify boss system
       FaBossIntegration.NotifyFaSkillUsed("Radar", true);
   }
   ```

2. **Protection Skill:**
   ```csharp
   public void UseProtectionSkill()
   {
       // Fa protection logic
       // ...

       FaBossIntegration.NotifyFaSkillUsed("Protection", true);
   }
   ```

3. **Reveal Skill:**
   ```csharp
   public void UseRevealSkill()
   {
       // Fa reveal logic
       // ...

       FaBossIntegration.NotifyFaSkillUsed("Reveal", true);
   }
   ```

### **Step 3: Player Integration**

1. **Player Attack System:**
   ```csharp
   // Trong player controller
   private void PerformAttack()
   {
       Collider[] hits = Physics.OverlapSphere(transform.position, attackRange);
       
       foreach (var hit in hits)
       {
           var boss = hit.GetComponent<BossController>();
           if (boss != null)
           {
               BossManager.Instance.PlayerAttackBoss();
               return;
           }
           
           var decoy = hit.GetComponent<DecoyBehavior>();
           if (decoy != null)
           {
               BossManager.Instance.PlayerAttackDecoy(hit.gameObject, decoy.IsReal);
               return;
           }
       }
   }
   ```

---

## 🎨 Visual Effects Setup

### **Decoy Effects:**
```csharp
// Trong DecoyBehavior.cs
private void CreateDecoyEffect()
{
    // Real decoy: Normal appearance
    // Fake decoy: Slightly transparent
    if (!isReal)
    {
        var renderer = GetComponent<Renderer>();
        var material = renderer.material;
        var color = material.color;
        color.a = 0.8f;
        material.color = color;
    }
}
```

### **Soul Effects:**
```csharp
// Trong SoulBehavior.cs  
private void CreateSoulEffect()
{
    // Floating animation
    // Glowing material
    // Particle effects
}
```

### **Fear Zone Effects:**
```csharp
// Trong FearZoneBehavior.cs
private void CreateFearZoneEffect()
{
    // Dark cylinder on ground
    // Post-processing effects
    // Audio changes
}
```

---

## 🧪 Testing & Debug

### **Debug Features:**
```csharp
// Console logs for state changes
[Boss State] Entered IdleState - Boss đứng yên tại chỗ
[Boss Animation] Playing animation: Idle
[Boss Event] Phase changed to 2

// Visual debug
- Gizmos show spawn radiuses
- Attack range visualization
- NavMesh path display
```

### **Test Controls:**
```csharp
// Development shortcuts
F1 - Reset boss
F2 - Force next phase  
F3 - Spawn test soul
F4 - Damage boss
F5 - Heal player
```

---

## ⚙️ Performance Optimization

### **Recommended Settings:**
```csharp
// Update frequencies
Boss FSM Update: Every frame
Soul tracking: Every 0.1s
UI updates: Every 0.2s

// Object pooling
Souls: Pool 5 objects
Decoys: Pool 3 objects
Effects: Pool 10 objects

// LOD (nếu cần)
Boss model: 3 LOD levels
Soul effects: 2 LOD levels
```

---

## 🐛 Troubleshooting

### **Common Issues:**

1. **Boss không spawn:**
   ```
   ✓ Check BossManager có BossController reference
   ✓ Check BossConfig đã assign
   ✓ Check NavMesh đã bake
   ```

2. **Animation không chạy:**
   ```
   ✓ Check Animator Controller assigned
   ✓ Check animation triggers đúng tên
   ✓ Check PlayAnimation() uncommented
   ```

3. **Fa skills không work:**
   ```
   ✓ Check FaBossIntegration trong scene
   ✓ Check event subscriptions
   ✓ Check NotifyFaSkillUsed() calls
   ```

4. **UI không hiện:**
   ```
   ✓ Check Canvas trong scene
   ✓ Check UI prefabs assigned
   ✓ Check BossManager references
   ```

5. **NavMesh issues:**
   ```
   ✓ Check ground objects Navigation Static
   ✓ Check NavMesh baked
   ✓ Check NavMeshAgent settings
   ```

---

## 🎯 Production Checklist

### **Before Release:**
- [ ] Tất cả animations đã implement
- [ ] Audio clips đã assign đầy đủ
- [ ] UI responsive trên các resolution
- [ ] Performance test trên target device
- [ ] Boss balance testing
- [ ] Fa skills integration hoàn chỉnh
- [ ] Error handling robust
- [ ] Debug logs removed/disabled
- [ ] Documentation update

### **Optional Enhancements:**
- [ ] Boss intro cutscene
- [ ] Death animations
- [ ] Particle effects
- [ ] Screen shake
- [ ] Post-processing effects
- [ ] Dynamic music
- [ ] Achievement system
- [ ] Boss variants

---

## 🎉 Congratulations!

Bạn đã setup xong toàn bộ Boss FSM System! 

**Hệ thống bao gồm:**
- ✅ Complete FSM với 10 states
- ✅ 2 phases với mechanics khác nhau  
- ✅ Fa skills integration
- ✅ UI system hoàn chỉnh
- ✅ Audio & visual effects
- ✅ Debug & testing tools
- ✅ Performance optimized
- ✅ Production ready

**Next Steps:**
1. Test toàn bộ flow Phase 1 → Phase 2
2. Balance thời gian và damage
3. Polish animations và effects
4. Integration testing với full game
5. Performance optimization cuối

Good luck with your Boss fight! 🚀
