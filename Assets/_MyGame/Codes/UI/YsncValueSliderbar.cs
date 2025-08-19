using UnityEngine;
using System;
using UnityEngine.UI;

[Serializable]
struct YsncValueSliderbarData
{
    public Slider[] targetSlider; // Slider B (nhận giá trị)
}

public class YsncValueSliderbar : MonoBehaviour
{
    [SerializeField] private YsncValueSliderbarData targetSlider;

    private Slider thisSlider; // Slider A (nơi gắn script)

    private void Awake()
    {
        thisSlider = GetComponent<Slider>(); // Lấy slider A từ chính GameObject gắn script
    }

    private void OnEnable()
    {
        // Đăng ký sự kiện khi giá trị thay đổi
        thisSlider.onValueChanged.AddListener(SyncValue);
    }

    private void OnDisable()
    {
        // Hủy đăng ký để tránh memory leak
        thisSlider.onValueChanged.RemoveListener(SyncValue);
    }

    private void SyncValue(float value)
    {
        // Cập nhật giá trị cho tất cả các slider B
        foreach (Slider slider in targetSlider.targetSlider)
        {
            if (slider != null) // Kiểm tra xem slider có null không
            {
                slider.value = value; // Cập nhật giá trị của slider B
            }
        }
    }
}
