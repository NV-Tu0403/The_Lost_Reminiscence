using System;

namespace Script.Procession.Reward.Base
{
    /// <summary>
    /// Lớp cơ sở cho phần thưởng.
    /// </summary>
    [Serializable]
    public abstract class Reward
    {
        public string Type; // Loại phần thưởng (Item, Experience,...)
        public abstract void Grant(); // Cấp phần thưởng
    }
}