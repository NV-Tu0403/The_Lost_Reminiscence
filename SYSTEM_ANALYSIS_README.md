# The Lost Reminiscence - System Analysis Documentation

## Tổng quan dự án
**The Lost Reminiscence** là một Unity game project sử dụng kiến trúc Event-Driven với hệ thống EventBus trung tâm để quản lý các tương tác giữa các module. Dự án được tổ chức theo mô hình modular với các hệ thống độc lập nhưng có thể tương tác thông qua events.

---

## 🎭 HỆ THỐNG DIALOGUE

### Cấu trúc tổng quan
- **DialogueManager**: Singleton quản lý toàn bộ hệ thống dialogue
- **DialogueNodeSO**: ScriptableObject chứa dữ liệu dialogue với localization
- **3 Panel hiển thị**: FullDialoguePanel, BubbleDialoguePanel, StoryDialoguePanel
- **DialogueAction**: Action xử lý sự kiện kích hoạt dialogue

### Flow hoạt động chi tiết

```
EventBus → DialogueAction → DialogueManager → StartDialogue()
    ↓
Load DialogueNodeSO từ Addressables theo dialogueId
    ↓
CheckDisplayDialogue() - Chọn panel dựa vào displayMode
    ↓
Hiển thị panel tương ứng:
├── FullPanel: Dialogue toàn màn hình
├── BubblePanel: Dialogue dạng bong bóng
└── StoryPanel: Dialogue dạng story/cutscene
    ↓
Xử lý tương tác:
├── Linear: NextButton → chuyển sang nextNode
└── Branching: Choice buttons → chuyển theo choices[]
    ↓
Kết thúc khi nextNode == null → EndDialogue() → Callback
```

### Tính năng đặc biệt
- **Localization**: Hỗ trợ đa ngôn ngữ với LocalizedString
- **Typewriter Effect**: Hiệu ứng gõ chữ từng ký tự (0.05s/char)
- **Skip Function**: Có thể bỏ qua toàn bộ dialogue
- **Audio Integration**: Âm thanh khi mở dialogue
- **Visual Effects**: Hiệu ứng nhấp nháy cho các nút tương tác
- **Speaker System**: Hiển thị tên nhân vật (Kien, Fa, Unknown...)

### Data Structure
```csharp
DialogueNodeSO {
    speakerName: SpeakerName,
    displayMode: DialogueDisplayMode,
    dialogueText: LocalizedString,
    choices: DialogueChoiceData[],  // Cho branching
    nextNode: DialogueNodeSO        // Cho linear
}
```

---

## 🧩 HỆ THỐNG PUZZLE

### Cấu trúc tổng quan
- **PuzzleManager**: Singleton quản lý tất cả puzzle
- **IPuzzleStep**: Interface chung cho các bước puzzle
- **PuzzleAction**: Action xử lý sự kiện kích hoạt puzzle

### Flow hoạt động chi tiết

```
EventBus → PuzzleAction → PuzzleManager.StartPuzzle()
    ↓
Tìm puzzle step theo puzzleId trong Dictionary
    ↓
Gọi step.StartStep(FinishPuzzle callback)
    ↓
Puzzle tự quản lý logic riêng theo implementation
    ↓
Khi hoàn thành → Gọi callback → FinishPuzzle() → onFinish
```

### Các loại Puzzle hiện có
- **LightTree**: Puzzle thắp sáng cây
- **OpenGate**: Puzzle mở cổng
- **InteractBridge**: Puzzle tương tác cầu

### Đặc điểm kỹ thuật
- **Auto-Discovery**: Tự động tìm và đăng ký puzzle steps
- **Flexible Architecture**: Dễ dàng thêm puzzle mới bằng cách implement IPuzzleStep
- **Force Complete**: Có thể force complete puzzle (cheat/debug)

---

## 🎬 HỆ THỐNG CUTSCENE

### Cấu trúc tổng quan
- **CutsceneManager**: Quản lý phát video cutscene
- **CutsceneSO**: ScriptableObject chứa data cutscene
- **CutsceneAction**: Action xử lý sự kiện kích hoạt cutscene

### Flow hoạt động chi tiết

```
EventBus → CutsceneAction → CutsceneManager.StartCutscene()
    ↓
Load CutsceneSO từ Resources/Cutscenes/{cutsceneId}
    ↓
Kiểm tra VideoClip và tạo UI
    ↓
Hiển thị cutscenePanel với skip button (nếu skippable)
    ↓
Phát video với VideoPlayer + audio riêng biệt
    ↓
Kết thúc khi video end hoặc skip → Cleanup → Callback
```

### Tính năng
- **Video Playback**: Sử dụng VideoPlayer cho video HD
- **Separate Audio**: Audio riêng biệt không phụ thuộc video
- **Skip Function**: Có thể bỏ qua cutscene (tùy theo setting)
- **Mouse Control**: Tự động hiển thị chuột khi phát cutscene
- **RenderTexture**: Hiển thị video qua RenderTexture trên UI

### Data Structure
```csharp
CutsceneSO {
    videoClip: VideoClip,
    audioClip: AudioClip,
    skippable: bool
}
```

---

## ⏰ HỆ THỐNG TIMELINE

### Cấu trúc tổng quan
- **TimelineManager**: Quản lý Timeline sequences
- **TimelineAction**: Action xử lý sự kiện kích hoạt timeline
- **PlayableDirector**: Unity Timeline player

### Flow hoạt động chi tiết

```
EventBus → TimelineAction → TimelineManager.StartTimeline()
    ↓
Load PlayableAsset từ Resources/Timelines/{timelineId}
    ↓
Hiển thị timelinePanel với skip button
    ↓
Phát Timeline qua PlayableDirector
    ↓
Lắng nghe stopped event hoặc skip → EndTimeline() → Callback
```

### Ứng dụng
- **Cinematic Sequences**: Các cảnh điện ảnh phức tạp
- **Animation Coordination**: Đồng bộ nhiều animation
- **Camera Movements**: Điều khiển camera theo script
- **Complex Events**: Trigger nhiều events theo timeline

---

## 📍 HỆ THỐNG CHECKPOINT

### Cấu trúc tổng quan
- **CheckpointAction**: Action lưu checkpoint
- **CheckpointZone**: Trigger zone tự động checkpoint
- **PlayerRespawnManager**: Quản lý respawn (không trong scope hiện tại)

### Flow hoạt động chi tiết

```
Trigger/Event → CheckpointAction → EventBus.Publish("Checkpoint")
    ↓
PlayerRespawnManager lắng nghe và lưu checkpoint
    ↓
Callback hoàn thành → Tiếp tục game progression
```

### Tính năng
- **Auto Save**: Tự động lưu vị trí và trạng thái
- **Event Integration**: Tích hợp với hệ thống event
- **Simple API**: Chỉ cần publish event "Checkpoint"

---

## 🎯 HỆ THỐNG TRIGGER

### Cấu trúc tổng quan
- **TriggerZone**: Abstract base class cho tất cả trigger
- **Specialized Triggers**: Các trigger chuyên biệt cho từng mục đích

### Các loại Trigger

#### 1. **CheckpointZone**
- Tự động lưu checkpoint khi player đi qua
- Disable sau khi trigger để tránh spam

#### 2. **DeadTriggerZone** 
- Trigger khi player chết/rơi xuống vực
- Xử lý respawn logic

#### 3. **DungeonTrigger**
- Trigger cho các sự kiện dungeon
- Thường liên kết với puzzle hoặc boss

#### 4. **FaTriggerZone**
- Trigger đặc biệt cho nhân vật Fa
- Xử lý events liên quan đến storyline

#### 5. **IdTriggerZone**
- Generic trigger với eventId
- Linh hoạt cho nhiều mục đích

#### 6. **PlayerTriggerZone**
- Trigger chỉ respond với Player
- Base trigger phổ biến nhất

#### 7. **RespawnTriggerZone**
- Trigger respawn player
- Reset về checkpoint gần nhất

#### 8. **RockTriggerZone**
- Trigger tương tác với rock/stone objects
- Có thể dùng cho puzzle

#### 9. **SupTriggerZone**
- Support/helper trigger
- Xử lý các sự kiện phụ trợ

### Flow hoạt động chung

```
Collider Enter → IsValidTrigger() check → OnTriggered()
    ↓
Publish event với eventId tương ứng
    ↓
Disable zone để tránh re-trigger (nếu cần)
```

---

## 🎯 HỆ THỐNG GAME EVENT SYSTEM

### Cấu trúc tổng quan
- **EventBus**: Static class quản lý pub/sub pattern trung tâm
- **EventManager**: Singleton quản lý event sequences và progression
- **EventDatabase**: Cơ sở dữ liệu sự kiện
- **EventExecutor**: Thực thi các action từ events
- **IEventAction**: Interface chung cho tất cả actions

### Flow hoạt động chi tiết

```
Event Trigger → EventBus.Publish() → EventManager.OnEventFinished()
    ↓
ProgressionManager.HandleEventFinished() → UpdateEventIndex()
    ↓
TryTriggerNextEvent() → Auto progression
```

### EventBus - Core Communication Hub

**Chức năng chính:**
- **Subscribe**: Đăng ký lắng nghe sự kiện
- **Unsubscribe**: Hủy đăng ký để tránh memory leak
- **Publish**: Phát sự kiện với data
- **ClearAll**: Reset toàn bộ subscribers

**Usage Pattern:**
```csharp
// Subscribe
EventBus.Subscribe("StartDialogue", OnDialogueStart);

// Publish
EventBus.Publish("StartDialogue", eventData);

// Unsubscribe
EventBus.Unsubscribe("StartDialogue", OnDialogueStart);
```

### EventManager - Sequence Controller

**Chức năng:**
- Quản lý chuỗi events theo thứ tự
- Auto-trigger event đầu tiên
- Cập nhật progression khi event hoàn thành
- Tích hợp với ProgressionManager

**Flow sequence:**
1. **Init**: Nhận danh sách eventIds
2. **RegisterEventBusListeners**: Đăng ký lắng nghe tất cả events
3. **AutoTriggerFirstEvent**: Kích hoạt event đầu nếu có thể
4. **OnEventFinished**: Xử lý khi event hoàn thành
5. **TryTriggerNextEvent**: Tiếp tục event sequence

---

## 📊 HỆ THỐNG GAME PROGRESSION

### Cấu trúc tổng quan
- **ProgressionManager**: Singleton quản lý tiến trình game
- **GameProgression**: Data structure chứa toàn bộ progression
- **ProgressionScriptableObject**: SO configuration cho progression
- **MainProcess & SubProcess**: Hierarchy của game progression

### Kiến trúc Progression

```
GameProgression
    ├── MainProcess[] (Chapters, Quests, Events...)
    │   ├── SubProcess[] (Individual steps)
    │   │   ├── Conditions[] (Requirements to complete)
    │   │   └── Rewards[] (Loot, items, abilities...)
    │   └── Rewards[] (Chapter completion rewards)
```

### Process Types & Status

**ProcessType Enum:**
- **Chapter**: Chương game chính
- **Quest**: Nhiệm vụ/task cụ thể
- **Dialogue**: Hội thoại story
- **Cutscene**: Video sequences
- **Puzzle**: Câu đố/mini-games
- **Checkpoint**: Save points
- **Event**: Special events
- **Timeline**: Complex sequences

**ProcessStatus Enum:**
- **Locked**: Chưa mở khóa
- **InProgress**: Đang thực hiện
- **Completed**: Đã hoàn thành

**TriggerType Enum:**
- **Manual**: Cần player trigger
- **Automatic**: Tự động kích hoạt

### Flow hoạt động chi tiết

```
Game Start → LoadProgression() → InitProgression()
    ↓
BuildEventSequence() → EventManager.Init()
    ↓
Auto-trigger first available event
    ↓
Event execution → OnEventFinished()
    ↓
HandleEventFinished() → Update progression status
    ↓
Check conditions → Unlock next processes
    ↓
Give rewards → Continue progression
```

### Progression Management

**Core Methods:**
- **CanTrigger(eventId)**: Kiểm tra có thể trigger event
- **IsWaitingForEvent(eventId)**: Kiểm tra đang chờ event
- **UnlockProcess(eventId)**: Mở khóa process
- **HandleEventFinished(eventId)**: Xử lý khi event hoàn thành
- **JumpToMainProcess(mainId)**: Nhảy đến process (debug)

**Integration với Save System:**
- Implement **ISaveable** interface
- Auto-save khi progression thay đổi
- Load progression từ ScriptableObject hoặc JSON

### Condition & Reward System

**Conditions**: Điều kiện để hoàn thành SubProcess
- Item collection
- Location reaching
- Character interaction
- Puzzle completion

**Rewards**: Phần thưởng khi hoàn thành
- Items và loot
- Abilities unlock
- Story progression
- Character development

---

## 🔧 KIẾN TRÚC TÍCH HỢP

### EventBus System
Tất cả hệ thống đều sử dụng **EventBus** làm trung tâm giao tiếp:

```csharp
// Publish event
EventBus.Publish("EventName", data);

// Subscribe event  
EventBus.Subscribe("EventName", OnEventHandler);

// Unsubscribe event
EventBus.Unsubscribe("EventName", OnEventHandler);
```

### BaseEventData Structure
```csharp
BaseEventData {
    eventId: string,           // ID của event
    OnFinish: Action          // Callback khi hoàn thành
}
```

### Action Pattern
Tất cả hệ thống implement **IEventAction**:
```csharp
interface IEventAction {
    void Execute(BaseEventData data);
}
```

### Flow tổng quát của hệ thống

```
Trigger/Input → EventBus → Action → Manager → Logic/UI → Callback → Next Event
    ↑                                     ↓                              ↓
    └─── ProgressionManager ──── EventManager ────── Auto Progression ───┘
```

### Event-Driven Architecture Flow

```
1. TRIGGER PHASE
   TriggerZone → Check ProgressionManager.CanTrigger()
   
2. EVENT EXECUTION
   EventBus.Publish() → EventAction.Execute() → Manager.Handle()
   
3. PROGRESSION UPDATE
   Event Finished → ProgressionManager.HandleEventFinished()
   
4. AUTO PROGRESSION
   Update Status → Check Conditions → Unlock Next → Auto Trigger
```

## 🎮 TÍCH HỢP GAMEPLAY

### Ví dụ luồng game điển hình:
1. **Player trigger zone** → ProgressionManager check → Trigger gửi event
2. **Dialogue hiển thị** → EventManager quản lý sequence → Giải thích nhiệm vụ  
3. **Puzzle activation** → Player giải puzzle → Complete event
4. **Progression update** → Mark completed → Unlock next MainProcess
5. **Cutscene phát** → Kết quả của puzzle → Story advancement
6. **Checkpoint save** → ProgressionManager save → Lưu tiến độ
7. **Timeline sequence** → EventManager auto-trigger → Chuyển scene tiếp theo

### Ưu điểm kiến trúc:
- **Loose Coupling**: Các hệ thống độc lập, dễ maintain
- **Event-Driven**: Phản ứng linh hoạt với các sự kiện
- **Progression Control**: Quản lý tiến trình game chặt chẽ
- **Auto Progression**: Tự động unlock và trigger events
- **Save Integration**: Tích hợp sẵn với save system
- **Modular**: Dễ dàng thêm/bớt tính năng
- **Reusable**: Code có thể tái sử dụng
- **Debuggable**: Dễ debug thông qua EventBus và ProgressionManager

### Nhược điểm cần lưu ý:
- **Event Overflow**: Cần quản lý subscribe/unsubscribe cẩn thận
- **Debug Complexity**: Khó trace flow khi có nhiều event
- **Progression Complexity**: Logic progression có thể phức tạp
- **Performance**: EventBus có thể bottleneck nếu quá nhiều event
- **Data Integrity**: Cần đảm bảo consistency giữa progression và actual game state

---

## 📋 KẾT LUẬN

Hệ thống **The Lost Reminiscence** được thiết kế theo pattern Event-Driven Architecture với EventBus làm backbone và ProgressionManager làm brain điều khiển flow game. Điều này tạo ra một framework mạnh mẽ và linh hoạt cho game narrative với khả năng tích hợp mượt mà giữa các systems.

**Core Systems:**
- **GameEventSystem**: Event communication backbone
- **Game Progression**: Intelligent progression management  
- **Dialogue**: Storytelling và character interaction
- **Puzzle**: Gameplay mechanics và challenges  
- **Cutscene**: Cinematic presentation
- **Timeline**: Complex sequencing
- **Checkpoint**: Progress management
- **Trigger**: World interaction và event activation

**Key Features:**
- **Smart Progression**: Tự động quản lý unlock và progression
- **Event Sequencing**: Điều khiển flow game thông qua event chains
- **Save Integration**: Seamless save/load với progression tracking
- **Modular Design**: Dễ dàng mở rộng và maintain
- **Debug Support**: Tools hỗ trợ debug và test progression

Kiến trúc này đặc biệt phù hợp cho các game narrative-driven như adventure, RPG, hoặc story-rich platformer với yêu cầu quản lý progression phức tạp và storytelling tuyến tính.
