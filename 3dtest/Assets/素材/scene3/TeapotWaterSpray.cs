using UnityEngine;

public class TeapotWaterSpray : MonoBehaviour
{
    [Header("喷水设置")]
    public ParticleSystem waterParticleSystem;  // 喷水粒子效果 
    public float sprayRange = 5f;               // 喷水距离 
    public LayerMask targetLayer;               // 目标物体所在的层 

    [Header("触发设置")]
    public GameObject targetObjectA;           // 被水击中的物体A 
    public GameObject objectB;                 // 播放动画的物体B 
    public string animationName = "PlayAnimation"; // 物体B的动画名称 

    private bool isHolding = false;
    private Animator objectBAnimator;

    void Start()
    {
        // 获取物体B的动画组件 
        if (objectB != null)
        {
            objectBAnimator = objectB.GetComponent<Animator>();
        }

        // 初始关闭喷水效果 
        if (waterParticleSystem != null)
        {
            waterParticleSystem.Stop();
        }
    }

    void Update()
    {
        if (isHolding && Input.GetKeyDown(KeyCode.E))
        {
            StartSpraying();
        }

        if (Input.GetKeyUp(KeyCode.E))
        {
            StopSpraying();
        }

        // 持续检测喷水碰撞 
        if (waterParticleSystem != null && waterParticleSystem.isPlaying)
        {
            DetectWaterCollision();
        }
    }

    // 开始喷水 
    void StartSpraying()
    {
        if (waterParticleSystem != null)
        {
            waterParticleSystem.Play();
        }
    }

    // 停止喷水 
    void StopSpraying()
    {
        if (waterParticleSystem != null)
        {
            waterParticleSystem.Stop();
        }
    }

    // 检测水是否击中物体A 
    void DetectWaterCollision()
    {
        RaycastHit hit;
        Vector3 direction = transform.forward;

        // 从茶壶口发射射线检测碰撞 
        if (Physics.Raycast(transform.position, direction, out hit, sprayRange, targetLayer))
        {
            if (hit.collider.gameObject == targetObjectA)
            {
                OnWaterHitObjectA();
            }
        }
    }

    // 当水击中物体A时的处理 
    void OnWaterHitObjectA()
    {
        Debug.Log("水击中了物体A！");

        // 触发物体B的动画 
        if (objectBAnimator != null && !string.IsNullOrEmpty(animationName))
        {
            objectBAnimator.Play(animationName);
        }

        // 可以在这里添加其他效果，比如声音、粒子等 
    }

    // 当玩家拿起茶壶时调用 
    public void OnPickUp()
    {
        isHolding = true;
        Debug.Log("拿起茶壶");
    }

    // 当玩家放下茶壶时调用 
    public void OnPutDown()
    {
        isHolding = false;
        StopSpraying();
        Debug.Log("放下茶壶");
    }
}