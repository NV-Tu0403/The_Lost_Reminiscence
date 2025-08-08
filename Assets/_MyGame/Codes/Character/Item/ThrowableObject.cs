using UnityEngine;
//using Fusion;
//using Fusion.Addons.Physics;
using DuckLe;

namespace Duckle
{
    public class ThrowableObject : MonoBehaviour
    {
        private PlayerController thrower;
        private PlayerController_02 thrower_02;
        private string usableName;
        private float effectValue;
        [SerializeField] private float LifeTime = 5f;

        public void SetThrower(PlayerController thrower) => this.thrower = thrower;
        public void SetThrower_02(PlayerController_02 thrower_02) => this.thrower_02 = thrower_02;
        public void SetUsableData(string name, float effectValue)
        {
            this.usableName = name;
            this.effectValue = effectValue;
        }
        public void Update()
        {
            // Kiểm tra nếu vật thể tồn tại đủ lifetime thì hủy nó
            if (LifeTime > 0)
            {
                LifeTime -= Time.deltaTime;
            }
            else
            {
                Destroy(gameObject);
                //Debug.Log($"Destroy {usableName} after {LifeTime} seconds");
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            // Kiểm tra va chạm với PlayerManager (player khác)
            if (collision.gameObject.TryGetComponent<PlayerManager>(out var playerManager) && playerManager != thrower.GetComponent<PlayerManager>())
            {
                playerManager.ApplyDamage(effectValue, thrower.name, usableName);
                Debug.Log($"Thrown {usableName} hit {collision.gameObject.name}, effect: {effectValue}");
            }
            /*Destroy(gameObject); */// Hủy vật thể sau khi va chạm
        }
    }
}