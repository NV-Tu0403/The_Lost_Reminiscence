using DuckLe;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // Add this at the top of the file


namespace Duckle
{
    /// <summary>
    /// Giao diện định nghĩa trạng thái của nhân vật trong game.
    /// </summary>
    public interface ICharacterState
    {
        /// <summary>
        /// Xác định xem trạng thái có phải là hành động chủ động hay không.
        /// </summary>
        bool IsAction { get; }

        /// <summary>
        /// Tên của trạng thái, dùng cho debug hoặc điều khiển animation.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gọi khi trạng thái được kích hoạt.
        /// </summary>
        void OnEnter(PlayerController controller);

        /// <summary>
        /// Gọi khi trạng thái bị hủy.
        /// </summary>
        void OnExit(PlayerController controller);

        /// <summary>
        /// Kiểm tra xem có thể chuyển đổi sang trạng thái khác hay không.
        /// </summary>
        bool CanTransitionTo(ICharacterState nextState);
    }

    /// <summary>
    /// Lớp cơ sở trừu tượng cho các trạng thái nhân vật.
    /// </summary>
    public abstract class CharacterState : ICharacterState
    {
        public abstract bool IsAction { get; }
        public abstract string Name { get; }

        public virtual void OnEnter(PlayerController controller) { }
        public virtual void OnExit(PlayerController controller) { }
        public virtual bool CanTransitionTo(ICharacterState nextState) => true;

        /// <summary>
        /// Kiểm tra xem trạng thái có giống với trạng thái khác hay không.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;
            return true;
        }

        public override int GetHashCode()
        {
            return GetType().GetHashCode();
        }
    }

    #region Trạng thái

    /// <summary>
    /// Trạng thái sống của nhân vật.
    /// </summary>
    public class IsLifeState : CharacterState
    {
        public override bool IsAction => false;
        public override string Name => "Life";
    }

    /// <summary>
    /// Trạng thái chết của nhân vật.
    /// </summary>
    public class IsDeadState : CharacterState
    {
        public override bool IsAction => false;
        public override string Name => "Dead";
    }

    /// <summary>
    /// Trạng thái nghỉ (Idle).
    /// </summary>
    public class IdleState : CharacterState
    {
        public override bool IsAction => false;
        public override string Name => "Idle";
    }

    /// <summary>
    /// Trạng thái rơi (Falling).
    /// </summary>
    public class FallingState : CharacterState
    {
        public override bool IsAction => false;
        public override string Name => "Falling";
    }

    /// <summary>
    /// Trạng thái di chuyển (Moving).
    /// </summary>
    public class MovingState : CharacterState
    {
        public override bool IsAction => true;
        public override string Name => "Moving";
    }

    /// <summary>
    /// Trạng thái nhảy (Jumping).
    /// </summary>
    public class JumpingState : CharacterState
    {
        public override bool IsAction => true;
        public override string Name => "Jumping";
    }

    /// <summary>
    /// Trạng thái cúi (Crouching).
    /// </summary>
    public class CrouchingState : CharacterState
    {
        public override bool IsAction => true;
        public override string Name => "Crouching";
    }

    /// <summary>
    /// Trạng thái lao nhanh (Dashing).
    /// </summary>
    public class DashingState : CharacterState
    {
        public float StartTime { get; }
        public float Duration { get; }

        public DashingState(float duration)
        {
            StartTime = Time.time;
            Duration = duration;
        }

        public override bool IsAction => true;
        public override string Name => "Dashing";

        public bool IsExpired => Time.time >= StartTime + Duration;
    }

    /// <summary>
    /// Trạng thái tấn công cận chiến (Melee Attacking).
    /// </summary>
    public class MeleeAttackingState : CharacterState
    {
        public float StartTime { get; }
        public float Duration { get; }
        public MeleeType MeleeType { get; }

        public MeleeAttackingState(float duration, MeleeType meleeType)
        {
            StartTime = Time.time;
            Duration = duration;
            MeleeType = meleeType;
        }

        public override bool IsAction => true;
        public override string Name => "MeleeAttacking";

        public bool IsExpired => Time.time >= StartTime + Duration;

        public override bool CanTransitionTo(ICharacterState nextState)
        {
            // Không thể chuyển sang trạng thái hành động khác khi đang tấn công
            return !nextState.IsAction || nextState is FallingState;
        }
    }

    /// <summary>
    /// Trạng thái ném (Throwing).
    /// </summary>
    public class ThrowingState : CharacterState
    {
        public float StartTime { get; }
        public float Duration { get; }

        public ThrowingState(float duration)
        {
            StartTime = Time.time;
            Duration = duration;
        }

        public override bool IsAction => true;
        public override string Name => "Throwing";

        public bool IsExpired => Time.time >= StartTime + Duration;

        public override bool CanTransitionTo(ICharacterState nextState)
        {
            // Không thể chuyển sang trạng thái hành động khác khi đang ném
            return !nextState.IsAction || nextState is FallingState;
        }
    }

    /// <summary>
    /// Trạng thái tương tác (Interacting).
    /// </summary>
    public class InteractingState : CharacterState
    {
        public float StartTime { get; }
        public float Duration { get; }
        public InteractType InteractType { get; }

        public InteractingState(float duration, InteractType interactType)
        {
            StartTime = Time.time;
            Duration = duration;
            InteractType = interactType;
        }

        public override bool IsAction => true;
        public override string Name => "Interacting";

        public bool IsExpired => Time.time >= StartTime + Duration;

        public override bool CanTransitionTo(ICharacterState nextState)
        {
            // Không thể chuyển sang trạng thái hành động khác khi đang tương tác
            return !nextState.IsAction || nextState is FallingState;
        }
    }
    #endregion

    /// <summary>
    /// Máy trạng thái để quản lý trạng thái chính và phụ của nhân vật.
    /// </summary>
    public class CharacterStateMachine
    {
        private ICharacterState _primaryState;
        private readonly HashSet<ICharacterState> _secondaryStates = new HashSet<ICharacterState>();
        private readonly PlayerController _controller;

        public CharacterStateMachine(PlayerController controller)
        {
            _controller = controller;
            _primaryState = new IdleState();
            _primaryState.OnEnter(controller);
        }

        /// <summary>
        /// Đặt trạng thái chính mới.
        /// </summary>
        public bool SetPrimaryState(ICharacterState state)
        {
            if (state == null)
            {
                Debug.LogWarning("Cannot set null primary state.");
                return false;
            }

            if (!_primaryState.CanTransitionTo(state))
            {
                Debug.LogWarning($"Cannot transition from {_primaryState.Name} to {state.Name}.");
                return false;
            }

            if (_primaryState != state)
            {
                _primaryState.OnExit(_controller);
                _primaryState = state;
                _primaryState.OnEnter(_controller);
                Debug.Log($"Primary state changed to: {state.Name}");
            }
            return true;
        }

        /// <summary>
        /// Thêm trạng thái phụ.
        /// </summary>
        public void AddSecondaryState(ICharacterState state)
        {
            if (state == null || _secondaryStates.Contains(state))
                return;

            _secondaryStates.Add(state);
            state.OnEnter(_controller);
            Debug.Log($"Added secondary state: {state.Name}");
        }

        /// <summary>
        /// Xóa trạng thái phụ.
        /// </summary>
        public void RemoveSecondaryState(ICharacterState state)
        {
            if (state == null || !_secondaryStates.Contains(state))
                return;

            _secondaryStates.Remove(state);
            state.OnExit(_controller);
            //Debug.Log($"Removed secondary state: {state.Name}");
        }

        /// <summary>
        /// Kiểm tra xem nhân vật có trạng thái cụ thể hay không.
        /// </summary>
        public bool HasState(ICharacterState state)
        {
            return _primaryState == state || _secondaryStates.Contains(state);
        }

        /// <summary>
        /// Cập nhật trạng thái, kiểm tra các trạng thái hết hạn.
        /// </summary>
        public void Update()
        {
            // Kiểm tra trạng thái chính
            if (_primaryState is MeleeAttackingState melee && melee.IsExpired ||
                _primaryState is ThrowingState throwing && throwing.IsExpired ||
                _primaryState is InteractingState interacting && interacting.IsExpired ||
                _primaryState is DashingState dashing && dashing.IsExpired)
            {
                SetPrimaryState(new IdleState());
            }

            // Kiểm tra trạng thái phụ
            var expiredStates = _secondaryStates
                .Where(s => (s is MeleeAttackingState m && m.IsExpired) ||
                            (s is ThrowingState t && t.IsExpired) ||
                            (s is InteractingState i && i.IsExpired) ||
                            (s is DashingState d && d.IsExpired))
                .ToList();

            foreach (var state in expiredStates)
            {
                RemoveSecondaryState(state);
            }

            // Đảm bảo luôn có trạng thái chính
            if (_primaryState == null)
            {
                SetPrimaryState(new IdleState());
            }
        }

        /// <summary>
        /// Lấy danh sách tất cả trạng thái hiện tại (dùng cho debug hoặc animation).
        /// </summary>
        public string[] GetAllStateNames()
        {
            var names = new List<string> { _primaryState.Name };
            names.AddRange(_secondaryStates.Select(s => s.Name));
            return names.ToArray();
        }
    }
}

//_____________________________________ ĐANG LỖI / CẦN CẢI TIẾN _____________________________________

//Logic cập nhật trạng thái trong Update sử dụng LINQ (Where, ToList), có thể không tối ưu về hiệu suất nếu danh sách trạng thái lớn.

// Thêm hệ thống sự kiện (event) để thông báo khi trạng thái thay đổi, giúp các thành phần khác (như UI, animation) phản ứng mà không cần truy cập trực tiếp _stateMachine.

//Một số trạng thái (như MeleeAttackingState) sử dụng tham số mặc định (duration = 0) khi kiểm tra HasState, có thể gây nhầm lẫn.

