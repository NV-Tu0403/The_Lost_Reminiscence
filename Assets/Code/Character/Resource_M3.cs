using DuckLe;
using UnityEngine;

namespace Duckle
{
    /// <summary>
    /// định nghĩa các thuộc tính và phương thức cơ bản của một đối tượng có thể sử dụng (như vũ khí)
    /// Giao diện chung cho mọi đối tượng có thể sử dụng trong hành động
    /// </summary>
    public interface IUsable
    {
        string Name { get; }
        string Classify { get; }
        float GetEffectValue();       // Giá trị hiệu ứng (Power cho Weapon, Heal cho Item, v.v.)
        void OnUse(PlayerController user); // Hành vi khi được sử dụng
    }

    /// <summary>
    /// Lớp cha trừu tượng, Cơ sở cho mọi tài nguyên (vũ khí, vật phẩm, ...)
    /// cung cấp các thuộc tính chung (Icon, Name, Classify, Power, Range, v.v.).
    /// </summary>
    public abstract class Resource_M3 : MonoBehaviour
    {
        public Sprite Icon { get; protected set; }
        public string Name { get; protected set; }
        public string Classify { get; protected set; } // Phân loại tài nguyên (vũ khí, vật phẩm, v.v.)
        public string Description { get; protected set; }

        public float Power { get; protected set; } // Sát thương hoặc hiệu ứng của tài nguyên
        public float Range { get; protected set; } // Phạm vi tác động
        public float UseSpeed { get; protected set; } // Tốc độ sử dụng
        public float ReloadSpeed { get; protected set; } // Tốc độ nạp lại
        public float Durability { get; protected set; } // Độ bền

        public abstract float GetEffectValue(); // Trả về giá trị hiệu ứng của tài nguyên
        public virtual void OnUse(PlayerController user) { /* Có thể để trống hoặc thêm logic mặc định */ }
    }

    /// <summary>
    /// Lớp cha trừu tượng, Cơ sở cho mọi vũ khí, Loot... .
    /// Item triển khai IUsable gián tiếp qua các phương thức Name, Classify, GetEffectValue(), và OnUse() được định nghĩa trong Resource_M3 và được ghi đè trong Weapon_N2.
    /// </summary>
    public abstract class Item : Resource_M3, IUsable
    {
        public override float GetEffectValue() => Power; // Trả về giá trị hiệu ứng của Item (sát thương hoặc hiệu ứng)

        /// <summary>
        /// Phương thức trừu tượng để áp dụng nâng cấp cho vũ khí.
        /// </summary>
        /// <param name="upgrade"></param>
        public abstract void ApplyUpgrade(Upgrade upgrade);

    }

    /// <summary>
    /// Lớp để lưu trữ thông tin nâng cấp vũ khí.
    /// </summary>
    public class Upgrade
    {
        public float? PowerOverride { get; set; }
        public float? RangeOverride { get; set; }
        public float? UseSpeedOverride { get; set; }
        public float? ReloadSpeedOverride { get; set; }
        public float? DurabilityOverride { get; set; }

        public Upgrade(float? powerOverride, float? rangeOverride, float? useSpeedOverride, float? reloadSpeedOverride, float? durabilityOverride)
        {
            PowerOverride = powerOverride;
            RangeOverride = rangeOverride;
            UseSpeedOverride = useSpeedOverride;
            ReloadSpeedOverride = reloadSpeedOverride;
            DurabilityOverride = durabilityOverride;
        }
    }

}

//_____________________________________ ĐANG LỖI / CẦN CẢI TIẾN _____________________________________


//Thiếu cơ chế quản lý tài nguyên (ví dụ: giới hạn số lượng vật phẩm mang theo). (cần tính toán thêm nhé :) )

// Trong ThrowAction, đối tượng ném được tạo bằng Object.Instantiate mỗi khi ném. Nếu tần suất ném cao, việc này có thể gây tốn hiệu suất và tạo ra rác bộ nhớ (garbage collection).