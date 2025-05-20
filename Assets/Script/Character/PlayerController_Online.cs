using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine;

namespace DuckLe
{
    ///// <summary>
    ///// Cấu trúc dữ liệu mạng dùng để lưu trữ thông tin của local Character Controller.
    ///// Được Fusion dùng để đồng bộ hóa trạng thái của nhân vật giữa các client.
    ///// 
    ///// Cách hoạt động:
    /////     Offline     : Dùng _offlineData (bản sao cục bộ trong PlayerController).
    /////     Online      : Dữ liệu được đồng bộ qua Photon Fusion.
    /////     
    ///// Nhận vào: 
    /////                 : Trạng thái từ PlayerController (vị trí, vận tốc, sprint, strafe, v.v.).
    ///// Trả về:
    /////     Offline     : _offlineData -> PersonAnimator.
    /////     Online      : Photon Fusion -> các client khác.
    /////     
    ///// </summary>
    //[StructLayout(LayoutKind.Explicit)]
    //[NetworkStructWeaved(WORDS + 50)]
    //public unsafe struct NetworkCCData_M3 : INetworkStruct
    //{
    //    public const int WORDS = NetworkTRSPData.WORDS + 50;
    //    public const int SIZE = WORDS * 4;

    //    [FieldOffset(0)]
    //    public NetworkTRSPData TRSPData;

    //    [FieldOffset((NetworkTRSPData.WORDS + 0) * Allocator.REPLICATE_WORD_SIZE)]
    //    int _grounded;

    //    [FieldOffset((NetworkTRSPData.WORDS + 1) * Allocator.REPLICATE_WORD_SIZE)]
    //    Vector3Compressed _velocityData;

    //    [FieldOffset((NetworkTRSPData.WORDS + 2) * Allocator.REPLICATE_WORD_SIZE)]
    //    int _strafing;

    //    [FieldOffset((NetworkTRSPData.WORDS + 3) * Allocator.REPLICATE_WORD_SIZE)]
    //    float _verticalSpeed;

    //    [FieldOffset((NetworkTRSPData.WORDS + 4) * Allocator.REPLICATE_WORD_SIZE)]
    //    float _horizontalSpeed;

    //    [FieldOffset((NetworkTRSPData.WORDS + 5) * Allocator.REPLICATE_WORD_SIZE)]
    //    float _inputMagnitude;

    //    [FieldOffset((NetworkTRSPData.WORDS + 6) * Allocator.REPLICATE_WORD_SIZE)]
    //    float _groundDistance;

    //    [FieldOffset((NetworkTRSPData.WORDS + 7) * Allocator.REPLICATE_WORD_SIZE)]
    //    int _attacking;

    //    [FieldOffset((NetworkTRSPData.WORDS + 8) * Allocator.REPLICATE_WORD_SIZE)]
    //    int _attackingMelee;

    //    [FieldOffset((NetworkTRSPData.WORDS + 9) * Allocator.REPLICATE_WORD_SIZE)]
    //    int _attackingThrow;

    //    public bool IsAttacking
    //    {
    //        get => _attacking == 1;
    //        set => _attacking = (value ? 1 : 0);
    //    }

    //    public bool IsMeleeAttacking
    //    {
    //        get => _attackingMelee == 1;
    //        set => _attackingMelee = (value ? 1 : 0);
    //    }

    //    public bool IsThrowing
    //    {
    //        get => _attackingThrow == 1;
    //        set => _attackingThrow = (value ? 1 : 0);
    //    }

    //    public bool Grounded
    //    {
    //        get => _grounded == 1;
    //        set => _grounded = (value ? 1 : 0);
    //    }

    //    public Vector3 Velocity
    //    {
    //        get => _velocityData;
    //        set => _velocityData = value;
    //    }

    //    public bool IsStrafing
    //    {
    //        get => _strafing == 1;
    //        set => _strafing = (value ? 1 : 0);
    //    }

    //    public float VerticalSpeed
    //    {
    //        get => _verticalSpeed;
    //        set => _verticalSpeed = value;
    //    }

    //    public float HorizontalSpeed
    //    {
    //        get => _horizontalSpeed;
    //        set => _horizontalSpeed = value;
    //    }

    //    public float InputMagnitude
    //    {
    //        get => _inputMagnitude;
    //        set => _inputMagnitude = value;
    //    }

    //    public float GroundDistance
    //    {
    //        get => _groundDistance;
    //        set => _groundDistance = value;
    //    }
    //}

    ///// <summary>
    ///// Quản lý di chuyển và trạng thái của nhân vật (dùng Rigidbody).
    ///// Class này hoạt động hoàn toàn Local, gửi dữ liệu đến networkCCDara_M3.
    /////
    ///// Nhận vào: 
    /////                 : PlayerInput.
    ///// Trả về: 
    /////     Offline     : _offlineData.
    /////     Online      : NetworkCCData_M3.
    /////     
    ///// </summary>
    //[DisallowMultipleComponent]                                 // Đảm bảo chỉ có một instance của component này trên GameObject
    //[RequireComponent(typeof(Rigidbody))]                       // Yêu cầu GameObject phải có Rigidbody
    //[NetworkBehaviourWeaved(NetworkCCData_M3.WORDS)]            // Chỉ định kích thước dữ liệu mạng dựa trên NetworkCCData_M3

    public class PlayerController_Online : MonoBehaviour
    {
        
    }

}