using System;
using Unity.Behavior;

[BlackboardEnum]
public enum FaState
{
    Idle,
	Follow,
	Observe,
	Signal,
	Support,
	CombatAssist,
	PuzzleAssist
}
