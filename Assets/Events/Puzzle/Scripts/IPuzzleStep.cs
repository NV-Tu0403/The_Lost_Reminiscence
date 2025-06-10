using System;

namespace Events.Puzzle.Scripts
{
    public interface IPuzzleStep
    {
        void StartStep(Action onComplete);
    }
}