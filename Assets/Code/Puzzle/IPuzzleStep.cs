using System;

namespace Script.Puzzle
{
    public interface IPuzzleStep
    {
        void StartStep(Action onComplete);
    }
}