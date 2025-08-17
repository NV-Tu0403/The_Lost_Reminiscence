using Script.Puzzle.Config.Base;
using UnityEngine;

namespace Script.Puzzle.Config.SO
{
    [CreateAssetMenu(menuName = "Puzzle/PuzzleConfig")]
    public class PuzzleConfigSO : ScriptableObject
    {
        
        [Tooltip("Độ cao mà các khối cầu được nâng lên.")]
        [Range(0.5f, 50f)]
        public float raiseHeight = 1.5f;

        [Tooltip("Thời gian để nâng mỗi khối cầu.")]
        [Range(0.1f, 3f)]
        public float raiseDuration = 0.5f;

        [Tooltip("Độ trễ giữa việc nâng từng khối cầu.")]
        [Range(0f, 1f)]
        public float raiseDelay = 0.2f;

        [Header("Collapse Settings")] 
        [Tooltip("Thời gian đếm ngược trước khi cầu sập.")]
        [Range(1f, 20f)]
        public float countdownTime = 5f;

        [Tooltip("Khoảng thời gian delay giữa từng khối cầu khi sập.")]
        [Range(0f, 1f)]
        public float fallDelayBetweenPieces = 0.2f;

        [Tooltip("Khoảng cách mà các khối cầu rơi xuống.")]
        [Range(1f, 500f)]
        public float fallDistance = 500f;

        [Tooltip("Thời gian để khối cầu rơi xuống.")]
        [Range(0.1f, 3f)]
        public float fallDuration = 0.6f;

        [Tooltip("Cường độ rung của các khối cầu trước khi rơi.")]
        public Vector3 shakeStrength = new Vector3(0.2f, 0.2f, 0.2f);

        
        public PuzzleConfig ToRunTimeData()
        {
            return new PuzzleConfig
            {
                RaiseHeight = raiseHeight,
                RaiseDuration = raiseDuration,
                RaiseDelay = raiseDelay,
                CountdownTime = countdownTime,
                FallDelayBetweenPieces = fallDelayBetweenPieces,
                FallDistance = fallDistance,
                FallDuration = fallDuration,
                ShakeStrength = shakeStrength
            };
        }
    }
}