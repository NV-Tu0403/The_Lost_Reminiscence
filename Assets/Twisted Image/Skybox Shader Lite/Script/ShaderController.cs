using UnityEngine;

[ExecuteInEditMode]
public class ShaderController : MonoBehaviour
{
    public Light directionalLight;
    public Material skyboxMaterial;

    void Update()
    {
        UpdateSunPosition();
    }

    void UpdateSunPosition()
    {
        if (directionalLight != null)
        {
            Vector3 sunDir = -directionalLight.transform.forward;
            Shader.SetGlobalVector("_SunDirection", new Vector4(sunDir.x, sunDir.y, sunDir.z, 0));
        }
    }
}
