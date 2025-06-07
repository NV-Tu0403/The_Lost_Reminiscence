using System;

namespace Script.Procession.Conditions
{
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
}