using System;
using UnityEngine;

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
        Timeline,
        Dialogue,
        Puzzle,
        Checkpoint,
        ChangeMap,
        Trap,
        Custom // cho phép mở rộng sau này
    }

public enum CharacterStateType
{
    Idle,
    Walk,
    Run,
    Dash,
    Sprint,
    Crouch,
    Jump,
    Attack,
    Defend,
    Die,
    Respawn,

    ThrowWeapon,
    ThrowItem,

    PickUp,
    Drop,
    Active,

    Skill1,
    Skill2,
    Skill3,
    Skill4,
    Skill5,
    Skill6,
    Skill7,
    Skill8,
    Skill9,
    Skill10
}

public enum CharacterActionType
{
    Idle,
    Walk,
    Run,
    Dash,
    Sprint,
    Crouch,
    Jump,
    Attack,
    Defend,
    Die,
    Respawn,

    ThrowWeapon,
    ThrowItem,

    PickUp,
    Drop,
    Active,

    Skill1,
    Skill2,
    Skill3,
    Skill4,
    Skill5,
    Skill6,
    Skill7,
    Skill8,
    Skill9,
    Skill10
}