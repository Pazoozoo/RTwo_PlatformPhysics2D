using UnityEngine;

public class AnimationController : MonoBehaviour {
    Animator _animator;
    float _airJumpTime;
    float _airJumpLength;

    const string IdleAnimation = "idle";
    const string RunAnimation = "run";
    const string JumpAnimation = "jump";
    const string AirJumpAnimation = "double_jump";
    const string WallSlideAnimation = "wall_slide";
    const string DieAnimation = "die";

    bool InAirJumpAnimation => Time.time < _airJumpTime + _airJumpLength;

    void Awake() {
        _animator = GetComponent<Animator>();
    }

    void OnEnable() {
        EventBroker.Instance.OnPlayerStateUpdate += PlayAnimation;
    }

    void OnDisable() {
        EventBroker.Instance.OnPlayerStateUpdate -= PlayAnimation;
    }

    void PlayAnimation(PlayerController.PlayerState newState) {
        switch (newState) {
            case PlayerController.PlayerState.Idle:
                _animator.Play(IdleAnimation);
                break;
            case PlayerController.PlayerState.Run:
                _animator.Play(RunAnimation);
                break;
            case PlayerController.PlayerState.Jump:
                if (!InAirJumpAnimation)
                    _animator.Play(JumpAnimation);
                break;
            case PlayerController.PlayerState.AirJump:
                _animator.Play(AirJumpAnimation);
                _airJumpTime = Time.time;
                _airJumpLength = _animator.GetCurrentAnimatorStateInfo(0).length;
                break;
            case PlayerController.PlayerState.WallJump:
                _animator.Play(JumpAnimation);
                break;
            case PlayerController.PlayerState.WallSlide:
                _animator.Play(WallSlideAnimation);
                break;
            case PlayerController.PlayerState.Die:
                _animator.Play(DieAnimation);
                break;
        }
    }
}
