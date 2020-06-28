using System.Collections.Generic;
using UnityEngine;

public class CycleOffset : MonoBehaviour
{
    public List<Animator> allCharacters = new List<Animator>();
    void Start()
    {
        foreach (Animator anim in allCharacters)
        {
            anim.SetFloat("Offset", Random.Range(0.0f, anim.GetCurrentAnimatorStateInfo(0).length));
        }
    }
}
