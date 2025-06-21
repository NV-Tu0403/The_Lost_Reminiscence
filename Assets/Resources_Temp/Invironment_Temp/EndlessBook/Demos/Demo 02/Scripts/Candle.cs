namespace echo17.EndlessBook.Demo02
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// điều khiên hiệu ứng ánh sáng của nến.
    /// </summary>
    public class Candle : MonoBehaviour
    {
        protected Light candleLight;
        protected float flickerTimeLeft;

        public Vector2 minMaxIntensity;
        public Vector2 minMaxInterval;

        void Awake()
        {
            candleLight = GetComponent<Light>();
        }

        void Update()
        {
            flickerTimeLeft -= Time.deltaTime;

            if (flickerTimeLeft <= 0)
            {
                flickerTimeLeft = Random.Range(minMaxInterval.x, minMaxInterval.y);
                candleLight.intensity = Random.Range(minMaxIntensity.x, minMaxIntensity.y);
            }

        }
    }
}