using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Character : MonoBehaviour
{
    public float moveSpeed;
    public bool IsMoving { get; private set; }
    CharacterAnimator animator;

    public CharacterAnimator Animator { get => animator; }

    private void Awake()
    {
        animator = GetComponent<CharacterAnimator>();
        SetPositionAndSnapToTile(transform.position);
    }

    public void SetPositionAndSnapToTile(Vector2 position)
    {
        position.x = Mathf.Floor(position.x) + .5f;
        position.y = Mathf.Floor(position.y) + .7f;

        transform.position = position;
    }

    public IEnumerator Move(Vector2 moveVector, Action OnMoveOver = null)
    {

        animator.MoveX = Mathf.Clamp(moveVector.x, -1f, 1f); // set animation x
        animator.MoveY = Mathf.Clamp(moveVector.y, -1f, 1f); // set animation y
        var targetPos = transform.position;
        targetPos.x += moveVector.x;
        targetPos.y += moveVector.y;

        if (!IsPathClear(targetPos))
        {
            yield break;
        }

        IsMoving = true;

        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;

        IsMoving = false;

        OnMoveOver?.Invoke();
    }

    public void HandleUpdate()
    {
        animator.IsMoving = IsMoving;
    }

    private bool IsPathClear(Vector3 targetPos)
    {
        var diff = targetPos - transform.position;
        var dir = diff.normalized;

        if (Physics2D.BoxCast(transform.position + dir, new Vector2(0.2f, 0.2f), 0f, dir, diff.magnitude - 1, GameLayers.i.SolidObjectsLayer | GameLayers.i.InteractableLayer | GameLayers.i.PlayerLayer) == true)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public void LookTowards(Vector3 targetPos)
    {
        var xdiff = Mathf.Floor(targetPos.x) - Mathf.Floor(transform.position.x);
        var ydiff = Mathf.Floor(targetPos.y) - Mathf.Floor(transform.position.y);

        if (xdiff == 0 || ydiff == 0)
        {
            animator.MoveX = Mathf.Clamp(xdiff, -1f, 1f); // set animation x
            animator.MoveY = Mathf.Clamp(ydiff, -1f, 1f); // set animation y
        }
    }

    private bool isWalkable(Vector3 targetPos)
    {
        if (Physics2D.OverlapCircle(targetPos, 0.2f, GameLayers.i.SolidObjectsLayer | GameLayers.i.InteractableLayer) != null)
        {
            return false;
        }
        return true;
    }

}
