using UnityEngine;

public class FPSDisplay : MonoBehaviour
{
    private float deltaTime = 0.0f;

    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.05f;
    }

    void OnGUI()
    {
        int w = Screen.width, h = Screen.height;

        GUIStyle style = new GUIStyle();
        Rect rect = new Rect(10, 10, w, h * 0.02f);

        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = h * 2 / 100;
        style.normal.textColor = Color.white;

        float msec = deltaTime * 1000.0f;  // ms cho mỗi frame
        float fps = 1.0f / deltaTime;      // FPS
        string text = $"{msec:0.0} ms ({fps:0.} fps)";

        GUI.Label(rect, text, style);
    }
}
