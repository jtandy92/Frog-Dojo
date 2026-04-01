using UnityEngine;
namespace Sv6.Dojo.State{

public class Idle : Base
{
    public override void Enter()
    {
        base.Enter();

        // Stop the player's movement
        _rigidbody.velocity = Vector2.zero;

        // Use the last non-zero direction for idle animation
        Vector2 direction = _playerController.lastNonZeroDirection;

        string animationName = GetIdleAnimationName(direction);

        _animator.Play(animationName);
    }

    public override void Do()
    {
        if (_xInput != 0 || _yInput != 0 || Input.GetKeyDown(KeyCode.Space))
        {
            _isComplete = true;
        }    
    }

    public override void Exit()
    {
        // Any necessary cleanup
    }

    private string GetIdleAnimationName(Vector2 direction)
    {
        string animationName = "";
        
        Vector2 normalizedDirection = direction.normalized;
        float x = Mathf.Round(normalizedDirection.x);
        float y = Mathf.Round(normalizedDirection.y);

        if (Mathf.Abs(x) > Mathf.Abs(y))
        {
            if (x > 0)
                animationName = "IdleEast";
            else
                animationName = "IdleWest";
        }
        else
        {
            if (y > 0)
                animationName = "IdleNorth";
            else
                animationName = "IdleSouth"; // Default to south if y is zero
        }

        return animationName;
    }
}
}