using Code.Procession;

namespace Script.Procession.Conditions
{
    public class CutsceneCompletedCondition : Condition
    {
        private string _cutsceneId;
        
        public CutsceneCompletedCondition(string cutsceneId)
        {
            _cutsceneId = cutsceneId;
        }
        
        public override bool IsSatisfied(object data)
        {
            return ProgressionManager.Instance.IsEventCompleted(_cutsceneId);
        }
    }
}