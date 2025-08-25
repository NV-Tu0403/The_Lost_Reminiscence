using UnityEngine;
using UnityEngine.AI;


namespace _MyGame.Codes.Timeline
{
    public static class PlayerLocker
    {
        public struct Snapshot
        {
            public bool HasRb, HasAgent;
            public bool RbKinematic, RbGravity;
            public bool AgentEnabled, AgentStopped;
            public MonoBehaviour[] DisabledInputs;
            // Camera lock snapshot
            public bool HasCamera;
            public MonoBehaviour[] DisabledCameraInputs;
        }

        public static GameObject FindByTag(string tag) => GameObject.FindGameObjectWithTag(tag);

        public static Snapshot Lock(GameObject player)
        {
            Snapshot s = new Snapshot();

            // Tắt input (disable mọi MonoBehaviour trừ Animator/Audio/etc. nếu cần lọc kỹ hơn)
            var inputs = player.GetComponents<MonoBehaviour>();
            var disabled = new System.Collections.Generic.List<MonoBehaviour>();
            foreach (var sc in inputs)
            {
                if (!sc.enabled) continue;
                sc.enabled = false; disabled.Add(sc);
            }
            s.DisabledInputs = disabled.ToArray();

            // Rigidbody
            var rb = player.GetComponent<Rigidbody>();
            if (rb) { s.HasRb = true; s.RbKinematic = rb.isKinematic; s.RbGravity = rb.useGravity; rb.isKinematic = true; rb.useGravity = false; }

            // NavMeshAgent
            var ag = player.GetComponent<NavMeshAgent>();
            if (ag) { s.HasAgent = true; s.AgentEnabled = ag.enabled; s.AgentStopped = ag.isStopped; ag.isStopped = true; ag.enabled = false; }

            return s;
        }

        // Overload: Lock player + optionally camera
        public static Snapshot Lock(GameObject player, bool includeCamera)
        {
            var s = Lock(player);
            if (!includeCamera) return s;

            var mainCam = Camera.main ? Camera.main.gameObject : FindByTag("MainCamera");
            if (mainCam)
            {
                var camBehaviours = mainCam.GetComponents<MonoBehaviour>();
                var disabledCam = new System.Collections.Generic.List<MonoBehaviour>();
                foreach (var sc in camBehaviours)
                {
                    if (!sc.enabled) continue;
                    sc.enabled = false; disabledCam.Add(sc);
                }
                s.HasCamera = true;
                s.DisabledCameraInputs = disabledCam.ToArray();
            }

            return s;
        }

        public static void Unlock(GameObject player, Snapshot s)
        {
            // Restore input
            if (s.DisabledInputs != null)
                foreach (var sc in s.DisabledInputs) if (sc) sc.enabled = true;

            // Restore RB
            var rb = player.GetComponent<Rigidbody>();
            if (s.HasRb && rb) { rb.isKinematic = s.RbKinematic; rb.useGravity = s.RbGravity; }

            // Restore Agent
            var ag = player.GetComponent<NavMeshAgent>();
            if (s.HasAgent && ag) { ag.enabled = s.AgentEnabled; ag.isStopped = s.AgentStopped; }

            // Restore camera behaviours
            if (s.DisabledCameraInputs != null)
            {
                foreach (var sc in s.DisabledCameraInputs)
                {
                    if (sc) sc.enabled = true;
                }
            }
        }
    }

}