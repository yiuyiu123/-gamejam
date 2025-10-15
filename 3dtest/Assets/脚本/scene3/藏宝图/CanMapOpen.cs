using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanMapOpen : MonoBehaviour
{
    [Header("�򿪵�ͼ���")]
    public bool canMapOpen = false;
    public GameObject UI_TreasureMap;

    [Header("��Ҽ��")]
    public string playerTag = "Player2";

    private bool playerInRange = false; 
    private bool hasOpen = false;

    private void Start()
    {
        UI_TreasureMap.SetActive(false);
    }

    void Update()
    {
        if (!canMapOpen && playerInRange && Input.GetKeyDown(KeyCode.H))
        {
            canMapOpen = true;
            OpenMap();
        }
    }

    private void OpenMap()
    {
        if (UI_TreasureMap != null && !hasOpen)
        {
            hasOpen = true;
            UI_TreasureMap.SetActive(true);
        }
        else if (UI_TreasureMap == null)
        {
            Debug.LogWarning("�ر�ͼΪ��");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
            playerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
            playerInRange = false;
    }
}
