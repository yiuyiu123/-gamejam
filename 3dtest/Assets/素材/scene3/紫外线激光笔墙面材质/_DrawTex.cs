using UnityEngine;

[ExecuteAlways]
public class _DrawTex : MonoBehaviour
{
    [Header("手电筒")]
    public Transform flashlight;      // 手电筒 Transform
    public Light flashlightLight;     // 手电筒 Light (Spot)

    [Header("涂鸦墙/点")]
    public Transform[] graffitiPoints;   // 每面墙涂鸦点
    public Renderer[] graffitiRenderers; // 对应材质球
    [Range(0, 1)]
    public float minVisibility = 0.1f;   // 最小显示阈值

    void Update()
    {
        if (flashlight == null || flashlightLight == null) return;
        if (graffitiPoints.Length != graffitiRenderers.Length) return;

        for (int i = 0; i < graffitiPoints.Length; i++)
        {
            Transform spot = graffitiPoints[i];
            Renderer rend = graffitiRenderers[i];
            if (spot == null || rend == null) continue;

            Vector3 toSpot = spot.position - flashlight.position;
            float distance = toSpot.magnitude;
            float angle = Vector3.Angle(flashlight.forward, toSpot);

            // 在光锥内
            bool insideCone = angle <= flashlightLight.spotAngle * 0.5f && distance <= flashlightLight.range;
            float visibility = 0f;

            if (insideCone)
            {
                // 简单距离衰减
                visibility = 1.0f - (distance / flashlightLight.range);
                visibility = Mathf.Clamp01(visibility);
                if (visibility < minVisibility) visibility = 0f;
            }

            // 更新材质属性（sharedMaterial 避免泄漏）
            rend.sharedMaterial.SetFloat("_DrawTexVisibility", visibility);
        }
    }
}