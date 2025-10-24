using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsSwitch : MonoBehaviour
{
    public static IsSwitch Instance;
    [Header("UI—°œÓ")]
    public GameObject UI_Yes;
    public GameObject UI_NO;

    [Header("UI’⁄’÷")]
    public GameObject UI_mask1;
    public GameObject UI_mask2;

    private void Start()
    {
        UI_mask1.SetActive(false);
        UI_mask2.SetActive(false);
    }

    public void ChooseYes()
    {
        UI_mask1.SetActive(true);
    }

    public void ChooseNo()
    {
        UI_mask2.SetActive(true);
    }
}
