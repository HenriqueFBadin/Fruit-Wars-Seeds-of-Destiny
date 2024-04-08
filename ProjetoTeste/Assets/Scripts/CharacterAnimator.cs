using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class CharacterAnimator : MonoBehaviour
{
    [SerializeField] List<Sprite> walkDownSprites;
    [SerializeField] List<Sprite> walkUpSprites;
    [SerializeField] List<Sprite> walkLeftSprites;
    [SerializeField] List<Sprite> walkRightSprites;
    [SerializeField] FaceDirection defaultDirection = FaceDirection.Down;


    // Parameters
    public float MoveX { get; set; }
    public float MoveY { get; set; }
    public bool IsMoving { get; set; }
    public FaceDirection DefaultDirection { get => defaultDirection; }

    // States of the Animation
    SpriteAnimator walkDownAnimation;
    SpriteAnimator walkUpAnimation;
    SpriteAnimator walkLeftAnimation;
    SpriteAnimator walkRightAnimation;

    // Variable to hold the current animation
    SpriteAnimator currentAnimation;

    SpriteRenderer spriteRenderer;

    bool wasMoving;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Initialize the animations
        walkDownAnimation = new SpriteAnimator(walkDownSprites, spriteRenderer);
        walkUpAnimation = new SpriteAnimator(walkUpSprites, spriteRenderer);
        walkLeftAnimation = new SpriteAnimator(walkLeftSprites, spriteRenderer);
        walkRightAnimation = new SpriteAnimator(walkRightSprites, spriteRenderer);
        SetFacingDirection(defaultDirection);

        currentAnimation = walkDownAnimation;
    }

    private void Update()
    {
        // Check if the animation has changed
        var prevAnimation = currentAnimation;

        // If the character is moving, then we want to update the animation
        if (MoveX == 1)
        {
            currentAnimation = walkRightAnimation;
        }
        else if (MoveX == -1)
        {
            currentAnimation = walkLeftAnimation;
        }
        else if (MoveY == 1)
        {
            currentAnimation = walkUpAnimation;
        }
        else if (MoveY == -1)
        {
            currentAnimation = walkDownAnimation;
        }

        // If the animation has changed, then we need to start the new animation
        if (prevAnimation != currentAnimation || IsMoving != wasMoving)
        {
            currentAnimation.Start();
        }

        // If the character is moving, then we want to update the animation
        if (IsMoving)
        {
            currentAnimation.HandleUpdate();
        }
        else
        {
            spriteRenderer.sprite = currentAnimation.Frames[0];
        }

        wasMoving = IsMoving;
    }

    public void SetFacingDirection(FaceDirection dir)
    {
        if (dir == FaceDirection.Right)
        {
            MoveX = 1;
        }
        else if (dir == FaceDirection.Left)
        {
            MoveX = -1;
        }
        else if (dir == FaceDirection.Up)
        {
            MoveY = 1;
        }
        else if (dir == FaceDirection.Down)
        {
            MoveY = -1;
        }
    }
}

public enum FaceDirection
{
    Down,
    Left,
    Right,
    Up
}