using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TerrianScanner : MonoBehaviour
{
    public GameObject TerrianScannerPrefab;
    public float duration = 10;
    public float size = 500;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            SwapnTerrianScanner();
        }
    }

    void SwapnTerrianScanner()
    {
        GameObject terrianScanner = Instantiate(TerrianScannerPrefab, gameObject.transform.position, Quaternion.identity);
        ParticleSystem terrianScannerPS = terrianScanner.transform.GetChild(0).GetComponent<ParticleSystem>();

        if (terrianScannerPS != null)
        {
            var main = terrianScannerPS.main;
            main.startLifetime = duration;
            main.startSize = size;
        }
        else
            Debug.Log("The first child doesn't have a particle system");

        Destroy(terrianScanner, duration + 1);
    }

}
