using System;
using UnityEngine;

public class EntranceDoorController : MonoBehaviour
{

    public Animator Animator;

    public void PlayAnimation()
    {
        Animator.Play("DoorClosing");
    }
}