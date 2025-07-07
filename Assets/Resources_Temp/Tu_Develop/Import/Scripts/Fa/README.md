# Fa AI System - Hướng dẫn sử dụng

## 📋 Tổng quan

Fa là một AI companion trong game "The Lost Reminiscence" với khả năng đi theo người chơi một cách thông minh, hỗ trợ cả NavMesh và basic movement.

## 🏗️ Cấu trúc hệ thống

### Core Components:
- **IFaAI**: Interface cơ bản cho AI system
- **IFaMovement**: Interface cho movement system với NavMesh support
- **FaAIBase**: Abstract base class cho Fa AI
- **FaMovement**: Implementation movement system với NavMesh + fallback
- **FaAIController**: Main AI controller với NavMesh integration
- **FaConfig**: ScriptableObject cho configuration

## 🚀 Cách sử dụng

### 1. Tạo GameObject cho Fa
```csharp
// Tạo GameObject
GameObject faObject = new GameObject("Fa");
faObject.tag = "Fa";

// Thêm các component cần thiết
var aiController = faObject.AddComponent<FaAIController>();
var movement = faObject.AddComponent<FaMovement>();
```

### 2. Cấu hình Fa
```csharp
// Tạo config
var config = ScriptableObject.CreateInstance<FaConfig>();
config.ApplyToController(aiController);
config.ApplyToMovement(movement);
```

### 3. Khởi tạo Fa
```csharp
// Fa sẽ tự động tìm Player và khởi tạo
aiController.Initialize();
```

## ⚙️ Cấu hình

### Movement Settings:
- **Move Speed**: Tốc độ di chuyển (mặc định: 3f)
- **Min Follow Distance**: Khoảng cách tối thiểu đến player (mặc định: 1.5f)
- **Max Follow Distance**: Khoảng cách tối đa đến player (mặc định: 3f)
- **Stopping Distance**: Khoảng cách dừng (mặc định: 0.1f)

### NavMesh Settings:
- **Use NavMesh**: Bật/tắt NavMesh (mặc định: true)
- **NavMesh Sample Radius**: Bán kính tìm NavMesh (mặc định: 5f)
- **NavMesh Agent Radius**: Bán kính NavMesh Agent (mặc định: 0.5f)
- **NavMesh Agent Height**: Chiều cao NavMesh Agent (mặc định: 2f)

### Fallback Settings:
- **Basic Movement Speed**: Tốc độ basic movement (mặc định: 2f)
- **Stuck Detection Time**: Thời gian phát hiện stuck (mặc định: 3f)
- **Stuck Detection Distance**: Khoảng cách phát hiện stuck (mặc định: 0.1f)
- **Enable Stuck Recovery**: Bật/tắt recovery khi stuck (mặc định: true)

### AI Behavior:
- **Enable Smooth Follow**: Bật/tắt smooth following
- **Smooth Follow Speed**: Tốc độ smooth follow
- **Enable Random Movement**: Bật/tắt random movement
- **Random Movement Radius**: Bán kính random movement

## 🎮 Tính năng hiện tại

### ✅ Đã hoàn thành:
- [x] Interface và abstract classes
- [x] Movement system với NavMesh support
- [x] Fallback về basic movement khi không có NavMesh
- [x] Follow player logic thông minh
- [x] Config system với ScriptableObject
- [x] Debug visualization và on-screen debug
- [x] Stuck detection và recovery
- [x] Auto-switch giữa NavMesh và basic movement
- [x] Smooth following với khoảng cách tối ưu

### 🔄 Đang phát triển:
- [ ] Skill system (6 skills chính)
- [ ] Perception module
- [ ] Decision module
- [ ] Learning system
- [ ] Player input integration

## 🧠 NavMesh Integration

### Tự động phát hiện NavMesh:
- Fa sẽ tự động kiểm tra NavMesh availability
- Nếu có NavMesh → Sử dụng NavMesh Agent
- Nếu không có NavMesh → Fallback về basic movement

### Stuck Detection:
- Phát hiện khi Fa bị kẹt
- Tự động chuyển sang basic movement
- Thử tìm đường khác

### Movement States:
- **Idle**: Đứng yên
- **Moving**: Đang di chuyển (basic)
- **Following**: Đang follow player
- **Pathfinding**: Đang tìm đường (NavMesh)
- **Stuck**: Bị kẹt

## 🐛 Debug

### Debug Info:
```csharp
string debugInfo = aiController.GetDebugInfo();
Debug.Log(debugInfo);
```

### Debug Gizmos:
- **Green sphere**: Min follow distance
- **Red sphere**: Max follow distance
- **Blue sphere**: Optimal position
- **Yellow line**: Movement path
- **Cyan sphere**: NavMesh sample radius
- **Red sphere**: Stuck detection area
- **Colored sphere**: Movement state indicator

### On-Screen Debug:
- Hiển thị movement state
- NavMesh availability
- Current movement mode
- Distance to target

## 📁 File Structure

```
Assets/Resources_Temp/Tu_Develop/Import/Scripts/Fa/
├── AI/
│   ├── IFaAI.cs              # Interface cơ bản
│   ├── IFaMovement.cs        # Interface movement với NavMesh
│   ├── FaAIBase.cs           # Abstract base class
│   ├── FaMovement.cs         # Movement với NavMesh + fallback
│   ├── FaAIController.cs     # Main AI controller
│   └── FaConfig.cs           # Configuration với NavMesh settings
└── README.md                 # Hướng dẫn này
```

## 🔧 Extension Points

### Thêm skill mới:
1. Định nghĩa trong `SkillType` enum
2. Implement trong `SkillManager`
3. Thêm logic trong `DecisionModule`

### Thêm behavior mới:
1. Extend `FaAIBase`
2. Override `UpdateAILogic()`
3. Implement custom logic

### Tùy chỉnh NavMesh behavior:
1. Modify `FaMovement.cs`
2. Thêm custom NavMesh logic
3. Extend stuck recovery methods

## 📝 Notes

- Fa sẽ tự động tìm Player với tag "Player"
- NavMesh Agent sẽ được tạo tự động nếu cần
- Có thể tùy chỉnh khoảng cách follow trong Inspector
- Debug gizmos chỉ hiển thị khi select GameObject
- Config có thể tạo từ menu Create > Fa > AI Configuration
- Fa sẽ tự động chuyển đổi giữa NavMesh và basic movement

## 🎯 Next Steps

1. Implement Perception module để thu thập data
2. Implement Decision module với priority queue
3. Implement 6 skills chính của Fa
4. Thêm learning system
5. Tích hợp với player input

## 🚨 Troubleshooting

### Fa không di chuyển:
- Kiểm tra Player có tag "Player" không
- Kiểm tra NavMesh có được bake không
- Kiểm tra NavMesh Agent settings

### Fa bị kẹt:
- Tăng stuck detection time
- Kiểm tra NavMesh Agent radius
- Thử tắt NavMesh và dùng basic movement

### Performance issues:
- Giảm NavMesh sample radius
- Tắt debug gizmos
- Tối ưu NavMesh Agent settings 

## 🦋 Giải pháp cho Fa là vật thể bay

### 1. **Không phụ thuộc hoàn toàn vào NavMesh**
- Khi Fa gặp vùng không có NavMesh, thay vì "nhảy", Fa sẽ **bay thẳng qua** vùng đó bằng cách di chuyển trực tiếp (dùng transform hoặc lerp).
- Chỉ sử dụng NavMesh để di chuyển trong các vùng có NavMesh (nếu cần tránh vật cản, v.v).

### 2. **Logic chuyển đổi giữa NavMesh và bay tự do**
- **Nếu có NavMesh:** Sử dụng NavMeshAgent như bình thường.
- **Nếu không có NavMesh:** Tự động chuyển sang chế độ bay tự do (move trực tiếp đến target).

### 3. **Pseudo-code minh họa**
```csharp
if (NavMeshAgent.isOnNavMesh && NavMesh.SamplePosition(target, out hit, 1f, NavMesh.AllAreas))
{
    // Di chuyển bằng NavMesh
    NavMeshAgent.SetDestination(target);
}
else
{
    // Bay tự do (không NavMesh)
    NavMeshAgent.enabled = false;
    transform.position = Vector3.MoveTowards(transform.position, target, flySpeed * Time.deltaTime);
    // Khi đến vùng có NavMesh, bật lại NavMeshAgent nếu muốn
}
```

### 4. **Gợi ý triển khai**
- Trong script di chuyển của Fa, luôn kiểm tra NavMesh availability.
- Nếu không có NavMesh, chuyển sang **fly mode** (di chuyển tự do, không va chạm mặt đất).
- Có thể thêm hiệu ứng bay (particle, animation) để tăng cảm giác "bay".

---

## **Tóm lại:**
- **Fa là vật thể bay** → Không cần "nhảy" qua vùng không có NavMesh.
- **Có thể bay tự do** qua mọi vùng, chỉ cần kiểm soát va chạm với tường hoặc vật cản nếu cần.
- **Chuyển đổi linh hoạt** giữa NavMesh và bay tự do giúp Fa di chuyển mượt mà, không bị kẹt.

---

Bạn muốn tôi hướng dẫn chi tiết cách code logic "bay tự do khi không có NavMesh" cho Fa không? Hay bạn muốn tối ưu lại hệ thống movement 