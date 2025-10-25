using UnityEngine;
using System.Collections;

public class TriggerAnimation : MonoBehaviour
{
    [SerializeField] private GameObject animatedObject;
    [SerializeField] private string triggerTag = "Player";
    private bool hasTriggered = false; // ��Ӵ�����־ 

    private void Start()
    {
        Animator animator = animatedObject.GetComponent<Animator>();
        if (animator != null)
        {
            animator.speed = 0;
            animator.Play("YourAnimationName", 0, 0f);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!hasTriggered && other.CompareTag(triggerTag))
        {
            hasTriggered = true; // ����Ѵ��� 
            Animator animator = animatedObject.GetComponent<Animator>();
            if (animator != null)
            {
                animator.speed = 1;
                animator.SetTrigger("Play");

                // ʹ��Э��ȷ��׼ȷ���� 
                StartCoroutine(DestroyAfterAnimation(animator));
            }
        }
    }

    private IEnumerator DestroyAfterAnimation(Animator animator)
    {
        // �ȴ��������ȵ�ʱ�� 
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        Destroy(animatedObject);
    }
}