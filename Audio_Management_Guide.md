# Hướng dẫn quản lý âm thanh trong Dark UI System

## 1. Tổng quan hệ thống âm thanh

Hệ thống Dark UI sử dụng **3 tầng quản lý âm thanh**:

```
┌─────────────────────────────────────────────────────────────┐
│                    AUDIO SYSTEM                            │
├─────────────────────────────────────────────────────────────┤
│  ┌─────────────┐    ┌─────────────┐    ┌─────────────┐  │
│  │   MASTER    │    │    MUSIC    │    │     SFX     │  │
│  │   VOLUME    │    │   VOLUME    │    │   VOLUME    │  │
│  └─────────────┘    └─────────────┘    └─────────────┘  │
│         │                   │                   │         │
│         ▼                   ▼                   ▼         │
│  ┌─────────────┐    ┌─────────────┐    ┌─────────────┐  │
│  │AudioMixer   │    │AudioMixer   │    │AudioMixer   │  │
│  │- Master     │    │- Music      │    │- SFX        │  │
│  └─────────────┘    └─────────────┘    └─────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

## 2. Các component chính

### 2.1 QualityManager.cs
**Chức năng**: Quản lý âm thanh tổng thể và cài đặt

```csharp
[Header("AUDIO")]
public AudioMixer mixer;
public SliderManager masterSlider;
public SliderManager musicSlider;
public SliderManager sfxSlider;
```

**Các phương thức chính**:
```csharp
public void VolumeSetMaster(float volume)
{
    mixer.SetFloat("Master", Mathf.Log10(volume) * 20);
}

public void VolumeSetMusic(float volume)
{
    mixer.SetFloat("Music", Mathf.Log10(volume) * 20);
}

public void VolumeSetSFX(float volume)
{
    mixer.SetFloat("SFX", Mathf.Log10(volume) * 20);
}
```

### 2.2 UIElementSound.cs
**Chức năng**: Phát âm thanh cho UI elements

```csharp
[Header("RESOURCES")]
public AudioSource audioSource;
public AudioClip hoverSound;
public AudioClip clickSound;
public AudioClip notificationSound;

[Header("SETTINGS")]
public bool enableHoverSound = true;
public bool enableClickSound = true;
```

## 3. Cách thiết lập hệ thống âm thanh

### 3.1 Bước 1: Tạo AudioMixer

1. **Tạo AudioMixer**:
   - Chuột phải trong Project → Create → Audio → Audio Mixer
   - Đặt tên: "GameAudioMixer"

2. **Tạo các Groups**:
   ```
   GameAudioMixer
   ├── Master
   ├── Music
   └── SFX
   ```

3. **Cấu hình Parameters**:
   - Master: "Master"
   - Music: "Music" 
   - SFX: "SFX"

### 3.2 Bước 2: Thiết lập QualityManager

```csharp
// Trong QualityManager
[Header("AUDIO")]
public AudioMixer mixer;           // Gán GameAudioMixer
public SliderManager masterSlider; // Slider cho Master volume
public SliderManager musicSlider;  // Slider cho Music volume  
public SliderManager sfxSlider;    // Slider cho SFX volume
```

### 3.3 Bước 3: Cấu hình SliderManager

**Cho Master Volume**:
```csharp
// SliderManager component
[Header("SAVING")]
public bool enableSaving = true;
public string sliderTag = "MasterVolume";
public float defaultValue = 1f;

[Header("SETTINGS")]
public bool usePercent = true;
public bool showValue = true;
```

**Cho Music Volume**:
```csharp
public string sliderTag = "MusicVolume";
```

**Cho SFX Volume**:
```csharp
public string sliderTag = "SFXVolume";
```

### 3.4 Bước 4: Kết nối với UI

1. **Tạo Slider cho từng loại âm thanh**
2. **Gán SliderManager component**
3. **Kết nối với QualityManager**

## 4. Cách sử dụng trong code

### 4.1 Thiết lập ban đầu

```csharp
public class AudioManager : MonoBehaviour
{
    [Header("AUDIO MIXER")]
    public AudioMixer gameMixer;
    
    [Header("SLIDERS")]
    public SliderManager masterVolumeSlider;
    public SliderManager musicVolumeSlider;
    public SliderManager sfxVolumeSlider;
    
    void Start()
    {
        // Tải cài đặt âm thanh đã lưu
        LoadAudioSettings();
    }
    
    void LoadAudioSettings()
    {
        // Master Volume
        float masterVolume = PlayerPrefs.GetFloat("MasterVolumeDarkSliderValue", 1f);
        gameMixer.SetFloat("Master", Mathf.Log10(masterVolume) * 20);
        
        // Music Volume  
        float musicVolume = PlayerPrefs.GetFloat("MusicVolumeDarkSliderValue", 1f);
        gameMixer.SetFloat("Music", Mathf.Log10(musicVolume) * 20);
        
        // SFX Volume
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolumeDarkSliderValue", 1f);
        gameMixer.SetFloat("SFX", Mathf.Log10(sfxVolume) * 20);
    }
}
```

### 4.2 Phát âm thanh UI

```csharp
public class UISoundController : MonoBehaviour
{
    [Header("AUDIO SOURCES")]
    public AudioSource uiAudioSource;
    
    [Header("SOUND CLIPS")]
    public AudioClip buttonHoverSound;
    public AudioClip buttonClickSound;
    public AudioClip panelOpenSound;
    public AudioClip panelCloseSound;
    
    public void PlayHoverSound()
    {
        if (uiAudioSource != null && buttonHoverSound != null)
            uiAudioSource.PlayOneShot(buttonHoverSound);
    }
    
    public void PlayClickSound()
    {
        if (uiAudioSource != null && buttonClickSound != null)
            uiAudioSource.PlayOneShot(buttonClickSound);
    }
    
    public void PlayPanelOpenSound()
    {
        if (uiAudioSource != null && panelOpenSound != null)
            uiAudioSource.PlayOneShot(panelOpenSound);
    }
    
    public void PlayPanelCloseSound()
    {
        if (uiAudioSource != null && panelCloseSound != null)
            uiAudioSource.PlayOneShot(panelCloseSound);
    }
}
```

### 4.3 Tích hợp với UI Elements

```csharp
// Trên Button component
public class ButtonSoundController : MonoBehaviour
{
    public UISoundController soundController;
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        soundController.PlayHoverSound();
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        soundController.PlayClickSound();
    }
}
```

## 5. Cấu hình AudioMixer Groups

### 5.1 Master Group
```
Volume: 0 dB (default)
Effects: None
```

### 5.2 Music Group
```
Volume: 0 dB (default)
Effects: 
- Lowpass Filter (optional)
- Reverb (optional)
```

### 5.3 SFX Group
```
Volume: 0 dB (default)
Effects:
- Highpass Filter (optional)
- Compressor (optional)
```

## 6. Luồng hoạt động âm thanh

```
┌─────────────────┐
│ User Input      │
│ (Slider/Button) │
└─────────┬───────┘
          │
          ▼
┌─────────────────┐
│ QualityManager  │
│ - Update Volume │
└─────────┬───────┘
          │
          ▼
┌─────────────────┐
│ AudioMixer      │
│ - Set Parameter │
└─────────┬───────┘
          │
          ▼
┌─────────────────┐
│ PlayerPrefs     │
│ - Save Setting  │
└─────────────────┘
```

## 7. Best Practices

### 7.1 Tổ chức âm thanh
```
Audio/
├── UI/
│   ├── ButtonHover.wav
│   ├── ButtonClick.wav
│   └── PanelTransition.wav
├── Music/
│   ├── BackgroundMusic.ogg
│   └── MenuMusic.ogg
└── SFX/
    ├── GameSFX.wav
    └── AmbientSFX.wav
```

### 7.2 Cài đặt AudioSource
```csharp
// UI AudioSource
AudioSource uiSource = gameObject.AddComponent<AudioSource>();
uiSource.playOnAwake = false;
uiSource.outputAudioMixerGroup = sfxGroup; // Gán vào SFX group

// Music AudioSource  
AudioSource musicSource = gameObject.AddComponent<AudioSource>();
musicSource.playOnAwake = true;
musicSource.loop = true;
musicSource.outputAudioMixerGroup = musicGroup; // Gán vào Music group
```

### 7.3 Tối ưu hóa
```csharp
public class AudioOptimizer : MonoBehaviour
{
    [Header("POOLING")]
    public int audioSourcePoolSize = 10;
    private Queue<AudioSource> audioSourcePool;
    
    void Start()
    {
        InitializeAudioPool();
    }
    
    void InitializeAudioPool()
    {
        audioSourcePool = new Queue<AudioSource>();
        
        for (int i = 0; i < audioSourcePoolSize; i++)
        {
            GameObject audioObj = new GameObject("PooledAudioSource");
            audioObj.transform.SetParent(transform);
            
            AudioSource source = audioObj.AddComponent<AudioSource>();
            source.playOnAwake = false;
            
            audioSourcePool.Enqueue(source);
        }
    }
    
    public AudioSource GetPooledAudioSource()
    {
        if (audioSourcePool.Count > 0)
            return audioSourcePool.Dequeue();
        
        return null;
    }
    
    public void ReturnToPool(AudioSource source)
    {
        source.Stop();
        audioSourcePool.Enqueue(source);
    }
}
```

## 8. Troubleshooting

### 8.1 Âm thanh không phát
- Kiểm tra AudioSource component
- Kiểm tra AudioMixer Groups
- Kiểm tra Volume settings
- Kiểm tra AudioListener trên Camera

### 8.2 Âm thanh bị trễ
- Sử dụng AudioSource Pooling
- Preload audio clips
- Tối ưu hóa audio format

### 8.3 Âm thanh bị overlap
- Sử dụng AudioSource Pooling
- Implement audio priority system
- Limit số lượng audio sources đồng thời

## 9. Ví dụ hoàn chỉnh

```csharp
public class CompleteAudioManager : MonoBehaviour
{
    [Header("AUDIO MIXER")]
    public AudioMixer gameMixer;
    
    [Header("UI SOUNDS")]
    public AudioClip[] uiHoverSounds;
    public AudioClip[] uiClickSounds;
    
    [Header("MUSIC")]
    public AudioClip[] backgroundMusic;
    
    [Header("SFX")]
    public AudioClip[] gameSFX;
    
    private AudioSource musicSource;
    private AudioSourcePool sfxPool;
    
    void Start()
    {
        InitializeAudioSystem();
        LoadAudioSettings();
    }
    
    void InitializeAudioSystem()
    {
        // Tạo music source
        GameObject musicObj = new GameObject("MusicSource");
        musicObj.transform.SetParent(transform);
        musicSource = musicObj.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.outputAudioMixerGroup = gameMixer.FindMatchingGroups("Music")[0];
        
        // Tạo SFX pool
        sfxPool = new AudioSourcePool(10, gameMixer.FindMatchingGroups("SFX")[0]);
    }
    
    public void PlayUISound(AudioClip clip)
    {
        AudioSource source = sfxPool.GetAudioSource();
        source.clip = clip;
        source.Play();
        
        StartCoroutine(ReturnToPoolAfterPlay(source, clip.length));
    }
    
    IEnumerator ReturnToPoolAfterPlay(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);
        sfxPool.ReturnAudioSource(source);
    }
}
```

Với hướng dẫn này, bạn có thể thiết lập một hệ thống quản lý âm thanh hoàn chỉnh cho Dark UI system với khả năng tùy chỉnh cao và hiệu suất tốt. 