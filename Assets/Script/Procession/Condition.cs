using System;


/// <summary>
/// Lớp trừu tượng cho điều kiện thực hiện tiến trình.
/// các lớp điều kiện cụ thể sẽ kế thừa từ lớp này.
/// </summary>
[Serializable]
public abstract class Condition
{
    public string Type;             // Loại điều kiện con (CollectItem, DefeatEnemy,...)

    /// <summary>
    /// Dựa trên (object data (dạng tuple)) được truyền vào để kiểm tra điều kiện và trả về true/false.
    /// ghi đè phương thức này để kiểm tra điều kiện cụ thể.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public abstract bool IsSatisfied(object data);
}

/// <summary>
/// Điều kiện thu thập item
/// </summary>
[Serializable]
public class InteractCondition : Condition
{
    public string TargetType; // "Weapon", "Loot", hoặc khác
    public string TargetName;           // ID Target
    public int RequiredAmount;      // Số lượng cần thu thập
    // các thuộc tính khác như loại item, cấp độ item, v.v. có thể thêm tiếp vào đây

    public override bool IsSatisfied(object data)
    {
        if (data is (string itemType, string itemId, int amount))
        {
            return itemType == TargetType && itemId == TargetName && amount >= RequiredAmount;
        }
        return false;

        //  bạn hoàn toàn có thể mở rộng tuple để kiểm tra thêm nhiều điều kiện khác, ví dụ:
        //if (data is (string itemId, int amount, string itemType))
        //{
        //    // Kiểm tra thêm itemType hoặc các điều kiện khác
        //    return itemId == Target && amount >= RequiredAmount && itemType == "Weapon";
        //}
        //•	Khi truyền dữ liệu vào phương thức, bạn cũng phải truyền đúng định dạng tuple, ví dụ:
        //  IsSatisfied(("sword01", 5, "Weapon"))
        //•	Nếu số lượng trường quá nhiều, nên cân nhắc tạo một class hoặc struct để truyền dữ liệu cho dễ quản lý và mở rộng.
    }
}

// Điều kiện tiêu diệt kẻ địch (đang lỗi)
[Serializable]
public class DefeatEnemyCondition : Condition
{
    public string EnemyType;
    public int RequiredAmount;

    public override bool IsSatisfied(object data)
    {
        if (data is (string enemyType, int amount))
        {
            return enemyType == EnemyType && amount >= RequiredAmount;
        }
        return false;
    }
}

