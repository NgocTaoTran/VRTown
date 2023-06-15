using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GOpponentState : MonoBehaviour, IState
{
    public Animator Animator
    {
        get
        {
            if (_animator == null)
            {
                AssignAnimationIDs();
            }
            return _animator;
        }
    }
    private Animator _animator = null;

    private int _animIDSpeed;
    private int _animIDMotionSpeed;
    private int _animIDJump;
    private int _animIDGrounded;
    private int _animIDFreeFall;
    private int _animIDFly;


    public void Setup(StateData stateData = null)
    {
        if (stateData == null)
        {
            Animator.SetBool(_animIDGrounded, true);
            Animator.SetFloat(_animIDSpeed, 0f);
            Animator.SetFloat(_animIDMotionSpeed, 1f);
            return;
        }
        
        Animator.SetFloat(_animIDSpeed, stateData.Speed);
        Animator.SetFloat(_animIDMotionSpeed, stateData.MotionSpeed);
        Animator.SetBool(_animIDGrounded, stateData.Grounded);
        Animator.SetBool(_animIDJump, stateData.Jump);
        Animator.SetBool(_animIDFly, stateData.Fly);
        Animator.SetBool(_animIDFreeFall, stateData.FreeFall);
    }

    public void AssignAnimationIDs()
    {
        _animator = GetComponentInChildren<Animator>();
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
        _animIDFly = Animator.StringToHash("Fly");

    }
}
