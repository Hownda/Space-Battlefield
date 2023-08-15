using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnlockSound : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        AudioSource[] audioSources = animator.GetComponents<AudioSource>();
        audioSources[0].Play();
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        AudioSource[] audioSources = animator.GetComponents<AudioSource>();
        audioSources[1].Play();
    }
}
