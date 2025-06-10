using System;
using UnityEngine;

namespace Duckle
{
    /// <summary>
    /// Các loại di chuyển
    /// </summary>
    public enum MoveType
    {
        Walk,
        Run,
        Sprint,
        Dash
    }

    /// <summary>
    /// Các loại ném
    /// </summary>
    public enum ThrowType
    {
        ThrowWeapon,
        ThrowItem
    }

    /// <summary>
    /// Các loại tấn công cận chiến
    /// </summary>
    public enum MeleeType
    {
        Melee_01,
        Melee_02,
        Melee_03,
        Melee_04
    }

    /// <summary>
    /// Các loại tương tác
    /// </summary>
    public enum InteractType
    {
        PickUp,
        Drop,
        Active,
        Interact_04
    }

    /// <summary>
    /// các loại sự kiện
    /// </summary>
    public enum EventType_Dl
    {
        Cutscene,
        ChangeMap,
        Dialogue,
        Trap,
        Puzzle, 
        Custom // cho phép mở rộng sau này
    }

}