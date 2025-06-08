using System;

namespace Script.Procession.Conditions
{
    /// <summary>
    /// Điều kiện thu thập item
    /// </summary>
    [Serializable]
    public class InteractCondition : Condition
    {
        public string TargetType;           // "Weapon", "Loot", hoặc khác
        public string TargetName;           // ID Target
        public int RequiredAmount;          // Số lượng cần thu thập
        // các thuộc tính khác như loại item, cấp độ item, v.v. có thể thêm tiếp vào đây
        
        public override bool IsSatisfied(object data)
        {
            if (data is (string itemType, string itemId, int amount))
            {
                return itemType == TargetType && itemId == TargetName && amount >= RequiredAmount;
            }
            return false;
        }
    }
}