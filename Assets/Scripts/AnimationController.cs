using UnityEngine;

public class AnimationController : MonoBehaviour {
    PlayerController.PlayerState _playerState;
    Animator _animator;

    const string IdleAnimation = "idle";
    const string RunAnimation = "run";
    const string JumpAnimation = "jump";
    const string AirJumpAnimation = "double_jump";
    const string WallSlideAnimation = "wallslide";
    const string DieAnimation = "die";

    void Awake() {
        _animator = GetComponent<Animator>();
    }

    void OnEnable() {
        EventBroker.Instance.OnPlayerStateUpdate += UpdatePlayerState;
    }

    void OnDisable() {
        EventBroker.Instance.OnPlayerStateUpdate -= UpdatePlayerState;
    }

    void PlayAnimation() {
        switch (_playerState) {
            case PlayerController.PlayerState.Idle:
                _animator.Play(IdleAnimation);
                break;
            case PlayerController.PlayerState.Run:
                _animator.Play(RunAnimation);
                break;
            case PlayerController.PlayerState.Jump:
                _animator.Play(JumpAnimation);
                break;
            case PlayerController.PlayerState.AirJump:
                _animator.Play(AirJumpAnimation);
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

    void UpdatePlayerState(PlayerController.PlayerState newState) {
        _playerState = newState;
        PlayAnimation();
    }
}
