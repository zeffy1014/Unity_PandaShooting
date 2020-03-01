using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifePanel : MonoBehaviour
{
    public GameObject[] marks;
    
    public void UpdateLife(int life)
    {
        for (int i=0; i<marks.Length; i++)
        {
            if (life > i) marks[i].SetActive(true);
            else marks[i].SetActive(false);
        }
    }
}
