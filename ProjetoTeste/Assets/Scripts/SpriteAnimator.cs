using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteAnimator
{
    SpriteRenderer spriteRenderer;
    List<Sprite> frames;
    float framesPerSecond;

    int currentFrame;
    float timePerFrame;
    float timer;

    public List<Sprite> Frames { get => frames; }

    public SpriteAnimator(List<Sprite> frames, SpriteRenderer spriteRenderer, float framesPerSecond = 6f)
    {
        this.frames = frames;
        this.spriteRenderer = spriteRenderer;
        this.framesPerSecond = framesPerSecond;

        // Calculate time per frame based on frames per second
        timePerFrame = 1f / framesPerSecond;
    }

    public void Start()
    {
        currentFrame = 0;
        timer = 0f;
        spriteRenderer.sprite = frames[currentFrame];
    }

    public void HandleUpdate()
    {
        timer += Time.deltaTime;
        while (timer > timePerFrame)
        {
            timer -= timePerFrame;
            currentFrame = (currentFrame + 1) % frames.Count;
            spriteRenderer.sprite = frames[currentFrame];
        }
    }
}
