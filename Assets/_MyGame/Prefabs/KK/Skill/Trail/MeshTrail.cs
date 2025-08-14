using System.Collections;
using UnityEngine;

public class MeshTrail : MonoBehaviour
{
    public bool alwaysActive = true; // Luôn chạy trail hay không
    public float activeTime = 2f;

    [Header("Mesh Related")]
    public float meshRefreshRate = 0.1f;
    public float meshDestroyDelay = 3f;

    public Transform positionToSpawn;

    [Header("Shader Related")]
    public Material mat;
    public string shaderVaRef;
    public float shaderVarRate = 0.1f;
    public float shaderVarRefreshRate = 0.05f;

    private bool isTrailActive;
    private SkinnedMeshRenderer[] skinnedMeshRenderers;

    void Start()
    {
        if (alwaysActive) // Nếu luôn chạy thì bật trail ngay khi Start
        {
            isTrailActive = true;
            StartCoroutine(ActivateTrail(activeTime));
        }
    }

    void Update()
    {
        // Nếu không luôn chạy, bạn có thể tự kích hoạt theo logic khác (VD: khi chạy nhanh)
        if (!alwaysActive && !isTrailActive)
        {
            isTrailActive = true;
            StartCoroutine(ActivateTrail(activeTime));
        }
    }

    IEnumerator ActivateTrail(float timeActive)
    {
        while (alwaysActive || timeActive > 0)
        {
            if (!alwaysActive)
                timeActive -= meshRefreshRate;

            if (skinnedMeshRenderers == null)
                skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();

            for (int i = 0; i < skinnedMeshRenderers.Length; i++)
            {
                GameObject obj = new GameObject();
                obj.transform.SetPositionAndRotation(positionToSpawn.position, positionToSpawn.rotation);

                MeshRenderer mr = obj.AddComponent<MeshRenderer>();
                MeshFilter mf = obj.AddComponent<MeshFilter>();

                Mesh mesh = new Mesh();
                skinnedMeshRenderers[i].BakeMesh(mesh);

                mf.mesh = mesh;
                mr.material = new Material(mat); // tạo bản copy tránh ảnh hưởng material gốc

                StartCoroutine(AnimateMaterialFloat(mr.material, 0, shaderVarRate, shaderVarRefreshRate));
                Destroy(obj, meshDestroyDelay);
            }

            yield return new WaitForSeconds(meshRefreshRate);
        }

        isTrailActive = false;
    }

    IEnumerator AnimateMaterialFloat(Material mat, float goal, float rate, float refreshRate)
    {
        float valueToAnimate = mat.GetFloat(shaderVaRef);

        while (valueToAnimate > goal)
        {
            valueToAnimate -= rate;
            mat.SetFloat(shaderVaRef, valueToAnimate);
            yield return new WaitForSeconds(refreshRate);
        }
    }
}
