using System;

namespace Code.Puzzle
{
    public interface IPuzzleStep
    {
        void StartStep(Action onComplete);
        void ForceComplete(bool instant = true);
    }
}