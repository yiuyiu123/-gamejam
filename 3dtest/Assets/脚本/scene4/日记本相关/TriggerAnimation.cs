using UnityEngine;
using System.Collections;

public class TriggerAnimation : MonoBehaviour
{
    [SerializeField] private GameObject animatedObject;
    [SerializeField] private string triggerTag = "Player";
    private bool hasTriggered = false; // 添加触发标志 

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
            hasTriggered = true; // 标记已触发 
            Animator animator = animatedObject.GetComponent<Animator>();
            if (animator != null)
            {
                animator.speed = 1;
                animator.SetTrigger("Play");

                // 使用协程确保准确销毁 
                StartCoroutine(DestroyAfterAnimation(animator));
            }
        }
    }

    private IEnumerator DestroyAfterAnimation(Animator animator)
    {
        // 等待动画长度的时间 
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        Destroy(animatedObject);
    }
}