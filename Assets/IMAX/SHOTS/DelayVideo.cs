using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Video;

public class DelayVideo : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    Animator animator;
    float startTimer;
    public float startDelay;//How long of a delay to wait until video turns on
    public bool videoStarted;

    void Start()
    {
        videoPlayer = GetComponentInChildren<VideoPlayer>();
        animator = GetComponent<Animator>();
        animator.SetFloat("AnimationSpeed", 0);
    }

    // Update is called once per frame
    void Update()
    {
        if(startTimer < startDelay && !videoStarted)
        {
            startTimer += Time.deltaTime;
        }
        else if(startTimer > startDelay && !videoStarted)
        {
            videoPlayer.Play();
            animator.SetFloat("AnimationSpeed", 1);
            videoStarted = true;
        }
    }
}