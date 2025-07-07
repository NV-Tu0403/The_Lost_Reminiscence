# Hướng dẫn tích hợp FMOD với Dark UI System

## 1. Tổng quan FMOD Integration

FMOD cung cấp hệ thống quản lý âm thanh chuyên nghiệp với khả năng:
- **Real-time mixing và processing**
- **Dynamic music system**
- **Advanced audio effects**
- **Cross-platform compatibility**

```
┌─────────────────────────────────────────────────────────────┐
│                    FMOD + DARK UI                         │
├─────────────────────────────────────────────────────────────┤
│  ┌─────────────┐    ┌─────────────┐    ┌─────────────┐  │
│  │   FMOD      │    │   DARK UI   │    │   UNITY     │  │
│  │   STUDIO    │    │   SYSTEM    │    │   AUDIO     │  │
│  └─────────────┘    └─────────────┘    └─────────────┘  │
│         │                   │                   │         │
│         ▼                   ▼                   ▼         │
│  ┌─────────────┐    ┌─────────────┐    ┌─────────────┐  │
│  │Event System │    │UI Elements  │    │AudioMixer   │  │
│  │Parameters   │    │Sound Triggers│   │Fallback     │  │
│  └─────────────┘    └─────────────┘    └─────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

## 2. Thiết lập FMOD với Dark UI

### 2.1 Cài đặt FMOD

1. **Download FMOD Studio** từ trang chủ FMOD
2. **Import FMOD Unity Integration** vào project
3. **Cấu hình FMOD Settings** trong Unity

### 2.2 Tạo FMOD Project

```
FMOD_Project/
├── Banks/
│   ├── Master.bank
│   ├── Music.bank
│   └── SFX.bank
├── Events/
│   ├── UI/
│   │   ├── Button_Hover
│   │   ├── Button_Click
│   │   └── Panel_Transition
│   ├── Music/
│   │   ├── Background_Music
│   │   └── Menu_Music
│   └── SFX/
│       ├── Game_Effects
│       └── Ambient_Sounds
└── Parameters/
    ├── Master_Volume
    ├── Music_Volume
    └── SFX_Volume
```

## 3. FMOD Manager cho Dark UI

### 3.1 FMODAudioManager.cs

```csharp
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

namespace Michsky.UI.Dark
{
    public class FMODAudioManager : MonoBehaviour
    {
        [Header("FMOD SETTINGS")]
        public StudioEventEmitter masterVolumeEmitter;
        public StudioEventEmitter musicVolumeEmitter;
        public StudioEventEmitter sfxVolumeEmitter;
        
        [Header("UI EVENTS")]
        [EventRef] public string buttonHoverEvent;
        [EventRef] public string buttonClickEvent;
        [EventRef] public string panelOpenEvent;
        [EventRef] public string panelCloseEvent;
        
        [Header("MUSIC EVENTS")]
        [EventRef] public string backgroundMusicEvent;
        [EventRef] public string menuMusicEvent;
        
        [Header("PARAMETERS")]
        public string masterVolumeParameter = "Master_Volume";
        public string musicVolumeParameter = "Music_Volume";
        public string sfxVolumeParameter = "SFX_Volume";
        
        private EventInstance masterVolumeInstance;
        private EventInstance musicVolumeInstance;
        private EventInstance sfxVolumeInstance;
        
        void Start()
        {
            InitializeFMOD();
            LoadAudioSettings();
        }
        
        void InitializeFMOD()
        {
            // Khởi tạo các Event Instance
            if (!string.IsNullOrEmpty(buttonHoverEvent))
                masterVolumeInstance = RuntimeManager.CreateInstance(buttonHoverEvent);
                
            if (!string.IsNullOrEmpty(backgroundMusicEvent))
                musicVolumeInstance = RuntimeManager.CreateInstance(backgroundMusicEvent);
                
            if (!string.IsNullOrEmpty(buttonClickEvent))
                sfxVolumeInstance = RuntimeManager.CreateInstance(buttonClickEvent);
        }
        
        void LoadAudioSettings()
        {
            // Tải cài đặt âm thanh từ PlayerPrefs
            float masterVolume = PlayerPrefs.GetFloat("MasterVolumeDarkSliderValue", 1f);
            float musicVolume = PlayerPrefs.GetFloat("MusicVolumeDarkSliderValue", 1f);
            float sfxVolume = PlayerPrefs.GetFloat("SFXVolumeDarkSliderValue", 1f);
            
            SetMasterVolume(masterVolume);
            SetMusicVolume(musicVolume);
            SetSFXVolume(sfxVolume);
        }
        
        public void SetMasterVolume(float volume)
        {
            RuntimeManager.StudioSystem.setParameterByName(masterVolumeParameter, volume);
            PlayerPrefs.SetFloat("MasterVolumeDarkSliderValue", volume);
        }
        
        public void SetMusicVolume(float volume)
        {
            RuntimeManager.StudioSystem.setParameterByName(musicVolumeParameter, volume);
            PlayerPrefs.SetFloat("MusicVolumeDarkSliderValue", volume);
        }
        
        public void SetSFXVolume(float volume)
        {
            RuntimeManager.StudioSystem.setParameterByName(sfxVolumeParameter, volume);
            PlayerPrefs.SetFloat("SFXVolumeDarkSliderValue", volume);
        }
        
        public void PlayUIHoverSound()
        {
            if (!string.IsNullOrEmpty(buttonHoverEvent))
            {
                EventInstance hoverInstance = RuntimeManager.CreateInstance(buttonHoverEvent);
                hoverInstance.start();
                hoverInstance.release();
            }
        }
        
        public void PlayUIClickSound()
        {
            if (!string.IsNullOrEmpty(buttonClickEvent))
            {
                EventInstance clickInstance = RuntimeManager.CreateInstance(buttonClickEvent);
                clickInstance.start();
                clickInstance.release();
            }
        }
        
        public void PlayPanelOpenSound()
        {
            if (!string.IsNullOrEmpty(panelOpenEvent))
            {
                EventInstance panelInstance = RuntimeManager.CreateInstance(panelOpenEvent);
                panelInstance.start();
                panelInstance.release();
            }
        }
        
        public void PlayPanelCloseSound()
        {
            if (!string.IsNullOrEmpty(panelCloseEvent))
            {
                EventInstance panelInstance = RuntimeManager.CreateInstance(panelCloseEvent);
                panelInstance.start();
                panelInstance.release();
            }
        }
        
        public void PlayBackgroundMusic()
        {
            if (!string.IsNullOrEmpty(backgroundMusicEvent))
            {
                musicVolumeInstance = RuntimeManager.CreateInstance(backgroundMusicEvent);
                musicVolumeInstance.start();
            }
        }
        
        public void StopBackgroundMusic()
        {
            if (musicVolumeInstance.isValid())
            {
                musicVolumeInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                musicVolumeInstance.release();
            }
        }
        
        void OnDestroy()
        {
            // Cleanup FMOD instances
            if (masterVolumeInstance.isValid())
                masterVolumeInstance.release();
                
            if (musicVolumeInstance.isValid())
                musicVolumeInstance.release();
                
            if (sfxVolumeInstance.isValid())
                sfxVolumeInstance.release();
        }
    }
}
```

### 3.2 FMODSliderManager.cs

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Michsky.UI.Dark
{
    public class FMODSliderManager : MonoBehaviour
    {
        [Header("FMOD MANAGER")]
        public FMODAudioManager fmodManager;
        
        [Header("SLIDER SETTINGS")]
        public Slider volumeSlider;
        public TextMeshProUGUI valueText;
        
        [Header("VOLUME TYPE")]
        public VolumeType volumeType;
        
        [Header("SAVING")]
        public bool enableSaving = true;
        public string sliderTag = "FMODSlider";
        public float defaultValue = 1f;
        
        public enum VolumeType
        {
            Master,
            Music,
            SFX
        }
        
        void Start()
        {
            if (volumeSlider == null)
                volumeSlider = GetComponent<Slider>();
                
            if (fmodManager == null)
                fmodManager = FindObjectOfType<FMODAudioManager>();
                
            SetupSlider();
        }
        
        void SetupSlider()
        {
            // Tải giá trị đã lưu
            if (enableSaving)
            {
                float savedValue = PlayerPrefs.GetFloat(sliderTag + "DarkSliderValue", defaultValue);
                volumeSlider.value = savedValue;
            }
            
            // Thêm listener cho slider
            volumeSlider.onValueChanged.AddListener(OnSliderValueChanged);
            
            UpdateValueText();
        }
        
        void OnSliderValueChanged(float value)
        {
            // Cập nhật FMOD parameter
            switch (volumeType)
            {
                case VolumeType.Master:
                    fmodManager.SetMasterVolume(value);
                    break;
                case VolumeType.Music:
                    fmodManager.SetMusicVolume(value);
                    break;
                case VolumeType.SFX:
                    fmodManager.SetSFXVolume(value);
                    break;
            }
            
            // Lưu giá trị
            if (enableSaving)
            {
                PlayerPrefs.SetFloat(sliderTag + "DarkSliderValue", value);
            }
            
            UpdateValueText();
        }
        
        void UpdateValueText()
        {
            if (valueText != null)
            {
                float percentage = volumeSlider.value * 100f;
                valueText.text = Mathf.RoundToInt(percentage) + "%";
            }
        }
    }
}
```

### 3.3 FMODUIElementSound.cs

```csharp
using UnityEngine;
using UnityEngine.EventSystems;

namespace Michsky.UI.Dark
{
    public class FMODUIElementSound : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
    {
        [Header("FMOD MANAGER")]
        public FMODAudioManager fmodManager;
        
        [Header("SOUND SETTINGS")]
        public bool enableHoverSound = true;
        public bool enableClickSound = true;
        
        [Header("CUSTOM EVENTS")]
        [EventRef] public string customHoverEvent;
        [EventRef] public string customClickEvent;
        
        void Start()
        {
            if (fmodManager == null)
                fmodManager = FindObjectOfType<FMODAudioManager>();
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (enableHoverSound && fmodManager != null)
            {
                if (!string.IsNullOrEmpty(customHoverEvent))
                {
                    // Phát custom hover sound
                    EventInstance hoverInstance = RuntimeManager.CreateInstance(customHoverEvent);
                    hoverInstance.start();
                    hoverInstance.release();
                }
                else
                {
                    // Phát default hover sound
                    fmodManager.PlayUIHoverSound();
                }
            }
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            if (enableClickSound && fmodManager != null)
            {
                if (!string.IsNullOrEmpty(customClickEvent))
                {
                    // Phát custom click sound
                    EventInstance clickInstance = RuntimeManager.CreateInstance(customClickEvent);
                    clickInstance.start();
                    clickInstance.release();
                }
                else
                {
                    // Phát default click sound
                    fmodManager.PlayUIClickSound();
                }
            }
        }
    }
}
```

## 4. Tích hợp với Dark UI Components

### 4.1 Cập nhật MainPanelManager

```csharp
public class MainPanelManager : MonoBehaviour
{
    [Header("FMOD INTEGRATION")]
    public FMODAudioManager fmodManager;
    
    // ... existing code ...
    
    public void PanelAnim(int newPanel)
    {
        if (newPanel != currentPanelIndex)
        {
            // ... existing panel animation code ...
            
            // Phát âm thanh panel transition
            if (fmodManager != null)
            {
                fmodManager.PlayPanelOpenSound();
            }
            
            // ... rest of existing code ...
        }
    }
}
```

### 4.2 Cập nhật QualityManager

```csharp
public class QualityManager : MonoBehaviour
{
    [Header("FMOD AUDIO")]
    public FMODAudioManager fmodManager;
    public FMODSliderManager masterVolumeSlider;
    public FMODSliderManager musicVolumeSlider;
    public FMODSliderManager sfxVolumeSlider;
    
    // ... existing code ...
    
    public void VolumeSetMaster(float volume)
    {
        if (fmodManager != null)
            fmodManager.SetMasterVolume(volume);
    }
    
    public void VolumeSetMusic(float volume)
    {
        if (fmodManager != null)
            fmodManager.SetMusicVolume(volume);
    }
    
    public void VolumeSetSFX(float volume)
    {
        if (fmodManager != null)
            fmodManager.SetSFXVolume(volume);
    }
}
```

## 5. FMOD Studio Setup

### 5.1 Tạo Events trong FMOD Studio

1. **UI Events**:
   ```
   UI_Button_Hover
   ├── Parameters: Volume, Pitch
   ├── Effects: Reverb, EQ
   └── Output: SFX Bus
   
   UI_Button_Click
   ├── Parameters: Volume, Pitch
   ├── Effects: Compressor
   └── Output: SFX Bus
   
   UI_Panel_Open
   ├── Parameters: Volume, Transition_Time
   ├── Effects: Reverb, Delay
   └── Output: SFX Bus
   ```

2. **Music Events**:
   ```
   Music_Background
   ├── Parameters: Volume, Tempo, Intensity
   ├── Effects: Reverb, EQ, Compressor
   └── Output: Music Bus
   
   Music_Menu
   ├── Parameters: Volume, Loop_Start, Loop_End
   ├── Effects: Reverb, EQ
   └── Output: Music Bus
   ```

3. **Global Parameters**:
   ```
   Master_Volume (0-1)
   Music_Volume (0-1)
   SFX_Volume (0-1)
   ```

### 5.2 Cấu hình Buses

```
Master Bus
├── Music Bus
│   ├── Background Music
│   └── Menu Music
└── SFX Bus
    ├── UI Sounds
    ├── Game Effects
    └── Ambient Sounds
```

## 6. Advanced FMOD Features

### 6.1 Dynamic Music System

```csharp
public class FMODDynamicMusic : MonoBehaviour
{
    [Header("MUSIC EVENTS")]
    [EventRef] public string calmMusicEvent;
    [EventRef] public string intenseMusicEvent;
    [EventRef] public string victoryMusicEvent;
    
    [Header("TRANSITION PARAMETERS")]
    public string intensityParameter = "Intensity";
    public string moodParameter = "Mood";
    
    private EventInstance currentMusicInstance;
    
    public void SetMusicIntensity(float intensity)
    {
        if (currentMusicInstance.isValid())
        {
            currentMusicInstance.setParameterByName(intensityParameter, intensity);
        }
    }
    
    public void SetMusicMood(string mood)
    {
        if (currentMusicInstance.isValid())
        {
            currentMusicInstance.setParameterByName(moodParameter, GetMoodValue(mood));
        }
    }
    
    public void TransitionToMusic(string musicEvent)
    {
        // Fade out current music
        if (currentMusicInstance.isValid())
        {
            currentMusicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            currentMusicInstance.release();
        }
        
        // Start new music
        currentMusicInstance = RuntimeManager.CreateInstance(musicEvent);
        currentMusicInstance.start();
    }
    
    private float GetMoodValue(string mood)
    {
        switch (mood.ToLower())
        {
            case "calm": return 0f;
            case "neutral": return 0.5f;
            case "intense": return 1f;
            default: return 0.5f;
        }
    }
}
```

### 6.2 Audio Effects Processing

```csharp
public class FMODAudioEffects : MonoBehaviour
{
    [Header("EFFECTS")]
    public bool enableReverb = true;
    public bool enableEQ = true;
    public bool enableCompressor = true;
    
    [Header("REVERB SETTINGS")]
    [Range(0f, 1f)] public float reverbLevel = 0.3f;
    [Range(0f, 1f)] public float reverbDecay = 0.5f;
    
    [Header("EQ SETTINGS")]
    [Range(-20f, 20f)] public float lowGain = 0f;
    [Range(-20f, 20f)] public float midGain = 0f;
    [Range(-20f, 20f)] public float highGain = 0f;
    
    public void ApplyReverbToEvent(string eventRef)
    {
        EventInstance instance = RuntimeManager.CreateInstance(eventRef);
        
        // Apply reverb effect
        if (enableReverb)
        {
            instance.setParameterByName("Reverb_Level", reverbLevel);
            instance.setParameterByName("Reverb_Decay", reverbDecay);
        }
        
        instance.start();
        instance.release();
    }
    
    public void ApplyEQToEvent(string eventRef)
    {
        EventInstance instance = RuntimeManager.CreateInstance(eventRef);
        
        // Apply EQ effect
        if (enableEQ)
        {
            instance.setParameterByName("EQ_Low", lowGain);
            instance.setParameterByName("EQ_Mid", midGain);
            instance.setParameterByName("EQ_High", highGain);
        }
        
        instance.start();
        instance.release();
    }
}
```

## 7. Performance Optimization

### 7.1 Event Pooling

```csharp
public class FMODEventPool : MonoBehaviour
{
    [Header("POOL SETTINGS")]
    public int poolSize = 20;
    public string[] eventRefs;
    
    private Dictionary<string, Queue<EventInstance>> eventPools;
    
    void Start()
    {
        InitializePools();
    }
    
    void InitializePools()
    {
        eventPools = new Dictionary<string, Queue<EventInstance>>();
        
        foreach (string eventRef in eventRefs)
        {
            Queue<EventInstance> pool = new Queue<EventInstance>();
            
            for (int i = 0; i < poolSize; i++)
            {
                EventInstance instance = RuntimeManager.CreateInstance(eventRef);
                pool.Enqueue(instance);
            }
            
            eventPools[eventRef] = pool;
        }
    }
    
    public EventInstance GetEventInstance(string eventRef)
    {
        if (eventPools.ContainsKey(eventRef) && eventPools[eventRef].Count > 0)
        {
            return eventPools[eventRef].Dequeue();
        }
        
        return RuntimeManager.CreateInstance(eventRef);
    }
    
    public void ReturnEventInstance(string eventRef, EventInstance instance)
    {
        if (eventPools.ContainsKey(eventRef))
        {
            instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            eventPools[eventRef].Enqueue(instance);
        }
        else
        {
            instance.release();
        }
    }
}
```

### 7.2 Memory Management

```csharp
public class FMODMemoryManager : MonoBehaviour
{
    [Header("MEMORY SETTINGS")]
    public bool enableBankStreaming = true;
    public bool enableEventPreloading = true;
    
    [Header("BANKS")]
    public string[] bankNames;
    
    void Start()
    {
        if (enableBankStreaming)
        {
            LoadBanksAsync();
        }
    }
    
    async void LoadBanksAsync()
    {
        foreach (string bankName in bankNames)
        {
            await LoadBankAsync(bankName);
        }
    }
    
    async Task LoadBankAsync(string bankName)
    {
        FMOD.Studio.Bank bank;
        FMOD.RESULT result = RuntimeManager.StudioSystem.loadBankFile(bankName, FMOD.Studio.LOAD_BANK_FLAGS.NORMAL, out bank);
        
        if (result == FMOD.RESULT.OK)
        {
            Debug.Log($"Loaded bank: {bankName}");
        }
        else
        {
            Debug.LogError($"Failed to load bank: {bankName}");
        }
    }
    
    void OnDestroy()
    {
        // Unload banks
        foreach (string bankName in bankNames)
        {
            RuntimeManager.StudioSystem.unloadBankFile(bankName);
        }
    }
}
```

## 8. Troubleshooting FMOD Integration

### 8.1 Common Issues

1. **Events không phát**:
   - Kiểm tra Event References
   - Kiểm tra Bank loading
   - Kiểm tra FMOD Studio project

2. **Parameters không hoạt động**:
   - Kiểm tra parameter names
   - Kiểm tra parameter ranges
   - Kiểm tra FMOD Studio setup

3. **Performance issues**:
   - Sử dụng Event Pooling
   - Enable Bank Streaming
   - Optimize Event complexity

### 8.2 Debug Tools

```csharp
public class FMODDebugger : MonoBehaviour
{
    [Header("DEBUG SETTINGS")]
    public bool enableDebugLogs = true;
    public bool showEventInfo = true;
    
    public void DebugEventInfo(string eventRef)
    {
        if (enableDebugLogs)
        {
            EventDescription eventDesc;
            FMOD.RESULT result = RuntimeManager.StudioSystem.getEvent(eventRef, out eventDesc);
            
            if (result == FMOD.RESULT.OK)
            {
                int parameterCount;
                eventDesc.getParameterCount(out parameterCount);
                Debug.Log($"Event: {eventRef}, Parameters: {parameterCount}");
            }
        }
    }
    
    public void DebugParameterValue(string eventRef, string parameterName, float value)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"Event: {eventRef}, Parameter: {parameterName}, Value: {value}");
        }
    }
}
```

## 9. Migration từ Unity Audio sang FMOD

### 9.1 Step-by-step Migration

1. **Backup existing audio system**
2. **Install FMOD Unity Integration**
3. **Create FMOD Studio project**
4. **Import audio assets vào FMOD**
5. **Update code references**
6. **Test và optimize**

### 9.2 Code Migration Example

```csharp
// OLD: Unity Audio
public void PlaySound(AudioClip clip)
{
    audioSource.PlayOneShot(clip);
}

// NEW: FMOD
public void PlaySound(string eventRef)
{
    EventInstance instance = RuntimeManager.CreateInstance(eventRef);
    instance.start();
    instance.release();
}
```

Với hướng dẫn này, bạn có thể tích hợp FMOD một cách chuyên nghiệp vào hệ thống Dark UI, tận dụng tất cả các tính năng mạnh mẽ của FMOD như real-time mixing, dynamic music system, và advanced audio processing. 