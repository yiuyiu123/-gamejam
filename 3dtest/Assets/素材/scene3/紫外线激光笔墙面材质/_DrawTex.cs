using UnityEngine;

[ExecuteAlways]
public class _DrawTex : MonoBehaviour
{
    [Header("手电筒")]
    public Transform flashlight;      // 手电筒 Transform
    public Light flashlightLight;     // 手电筒 Spot Light

    [Header("涂鸦墙")]
    public Transform[] graffitiPoints;     // 每面墙的涂鸦检测点
    public Renderer[] graffitiRenderers;   // 每面墙对应材质球
    [Range(0f, 1f)]
    public float minVisibility = 0.1f;     // 最小可见度

    void Update()
    {
        if (flashlight == null || flashlightLight == null) return;

        Vector3 flashPos = flashlight.position;
        Vector3 flashDir = flashlight.forward;

        for (int i = 0; i < graffitiPoints.Length; i++)
        {
            Transform spot = graffitiPoints[i];
            Renderer rend = graffitiRenderers[i];

            if (spot == null || rend == null) continue;

            // 距离和角度计算
            Vector3 toSpot = spot.position - flashPos;
            float dist = toSpot.magnitude;
            Vector3 dir = toSpot.normalized;

            float spotDot = Vector3.Dot(dir, flashDir);
            float cosHalfAngle = Mathf.Cos(flashlightLight.spotAngle * 0.5f * Mathf.Deg2Rad);
            float angleAtten = Mathf.Clamp01((spotDot - cosHalfAngle) / (1f - cosHalfAngle));
            float rangeAtten = Mathf.Clamp01(1f - dist / flashlightLight.range);

            float visibility = angleAtten * rangeAtten;
            if (visibility < minVisibility) visibility = 0f;

            // 更新材质参数
            rend.sharedMaterial.SetVector("_FlashPos", flashPos);
            rend.sharedMaterial.SetVector("_FlashDir", flashDir);
            rend.sharedMaterial.SetFloat("_FlashAngle", flashlightLight.spotAngle);
            rend.sharedMaterial.SetFloat("_FlashRange", flashlightLight.range);
            rend.sharedMaterial.SetFloat("_MinVisibility", minVisibility);
        }
    }
}
