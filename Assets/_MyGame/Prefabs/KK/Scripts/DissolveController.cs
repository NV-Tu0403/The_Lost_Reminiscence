using System.Collections;
using UnityEngine;
using UnityEngine.VFX;


public class DissolveController : MonoBehaviour
{
    public Renderer targetRenderer;
    //public SkinnedMeshRenderer skinnedMesh;
    public VisualEffect VFXGraph;
    public float dissolveRate = 0.0125f;
    public float refreshRate = 0.025f;

    private Material[] targetMaterials;

    void Start()
    {
        //if (skinnedMesh != null)
        //skinnedMaterials = skinnedMesh.materials;
        if (targetRenderer != null)
            targetMaterials = targetRenderer.materials;
    }

    void Update()
    {
        if(Input.GetKeyDown (KeyCode.Space))
        {
            StartCoroutine(DissolveCo());
        }
    }

    IEnumerator DissolveCo()
    {
        if(VFXGraph != null)
        { 
           VFXGraph.Play();
        }

        if(targetMaterials.Length > 0)
        {
            float counter = 0;

            while (targetMaterials[0].GetFloat("_DissolveAmount") < 1)
            {
                counter += dissolveRate;
                for(int i = 0; i < targetMaterials.Length; i++)
                {
                    targetMaterials[i].SetFloat("_DissolveAmount", counter);
                }
                yield return new WaitForSeconds(refreshRate);
            }    
        }    
    }    
}
