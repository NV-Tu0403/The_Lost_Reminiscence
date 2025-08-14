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

            // Lấy cả SkinnedMeshRenderer và MeshRenderer
            if (skinnedMeshRenderers == null)
                skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
            MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();

            // Với SkinnedMeshRenderer (nhân vật rig xương)
            for (int i = 0; i < skinnedMeshRenderers.Length; i++)
            {
                GameObject obj = new GameObject();
                obj.transform.SetPositionAndRotation(positionToSpawn.position, positionToSpawn.rotation);

                MeshRenderer mr = obj.AddComponent<MeshRenderer>();
                MeshFilter mf = obj.AddComponent<MeshFilter>();

                Mesh mesh = new Mesh();
                skinnedMeshRenderers[i].BakeMesh(mesh);

                mf.mesh = mesh;
                mr.material = new Material(mat);
                StartCoroutine(AnimateMaterialFloat(mr.material, 0, shaderVarRate, shaderVarRefreshRate));

                Destroy(obj, meshDestroyDelay);
            }

            // Với MeshRenderer thường (không rig)
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                var mfOriginal = meshRenderers[i].GetComponent<MeshFilter>();
                if (mfOriginal != null && mfOriginal.sharedMesh != null)
                {
                    GameObject obj = new GameObject();
                    obj.transform.SetPositionAndRotation(positionToSpawn.position, positionToSpawn.rotation);

                    MeshRenderer mr = obj.AddComponent<MeshRenderer>();
                    MeshFilter mf = obj.AddComponent<MeshFilter>();

                    mf.mesh = mfOriginal.sharedMesh;
                    mr.material = new Material(mat);
                    StartCoroutine(AnimateMaterialFloat(mr.material, 0, shaderVarRate, shaderVarRefreshRate));

                    Destroy(obj, meshDestroyDelay);
                }
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
