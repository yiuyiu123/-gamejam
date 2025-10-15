using UnityEngine;

public class TeapotWaterSpray : MonoBehaviour
{
    [Header("��ˮ����")]
    public ParticleSystem waterParticleSystem;  // ��ˮ����Ч�� 
    public float sprayRange = 5f;               // ��ˮ���� 
    public LayerMask targetLayer;               // Ŀ���������ڵĲ� 

    [Header("��������")]
    public GameObject targetObjectA;           // ��ˮ���е�����A 
    public GameObject objectB;                 // ���Ŷ���������B 
    public string animationName = "PlayAnimation"; // ����B�Ķ������� 

    private bool isHolding = false;
    private Animator objectBAnimator;

    void Start()
    {
        // ��ȡ����B�Ķ������ 
        if (objectB != null)
        {
            objectBAnimator = objectB.GetComponent<Animator>();
        }

        // ��ʼ�ر���ˮЧ�� 
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

        // ���������ˮ��ײ 
        if (waterParticleSystem != null && waterParticleSystem.isPlaying)
        {
            DetectWaterCollision();
        }
    }

    // ��ʼ��ˮ 
    void StartSpraying()
    {
        if (waterParticleSystem != null)
        {
            waterParticleSystem.Play();
        }
    }

    // ֹͣ��ˮ 
    void StopSpraying()
    {
        if (waterParticleSystem != null)
        {
            waterParticleSystem.Stop();
        }
    }

    // ���ˮ�Ƿ��������A 
    void DetectWaterCollision()
    {
        RaycastHit hit;
        Vector3 direction = transform.forward;

        // �Ӳ���ڷ������߼����ײ 
        if (Physics.Raycast(transform.position, direction, out hit, sprayRange, targetLayer))
        {
            if (hit.collider.gameObject == targetObjectA)
            {
                OnWaterHitObjectA();
            }
        }
    }

    // ��ˮ��������Aʱ�Ĵ��� 
    void OnWaterHitObjectA()
    {
        Debug.Log("ˮ����������A��");

        // ��������B�Ķ��� 
        if (objectBAnimator != null && !string.IsNullOrEmpty(animationName))
        {
            objectBAnimator.Play(animationName);
        }

        // �����������������Ч�����������������ӵ� 
    }

    // �����������ʱ���� 
    public void OnPickUp()
    {
        isHolding = true;
        Debug.Log("������");
    }

    // ����ҷ��²��ʱ���� 
    public void OnPutDown()
    {
        isHolding = false;
        StopSpraying();
        Debug.Log("���²��");
    }
}