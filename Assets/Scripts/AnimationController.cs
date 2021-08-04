using UnityEngine;

public class AnimationController : MonoBehaviour {
    [SerializeField] GameObject dustEffect;
    
    Animator _animator;
    int _faceDirection = 1;
    float _airJumpTime;
    float _airJumpLength;
    
    Bounds _bounds;

    const string IdleAnimation = "idle";
    const string RunAnimation = "run";
    const string JumpAnimation = "jump";
    const string AirJumpAnimation = "double_jump";
    const string WallSlideAnimation = "wall_slide";
    const string DieAnimation = "die";

    bool InAirJumpAnimation => Time.time < _airJumpTime + _airJumpLength;

    void Awake() {
        _animator = GetComponent<Animator>();
        _bounds = GetComponent<Collider2D>().bounds;
    }

    void OnEnable() {
        EventBroker.Instance.OnPlayerStateUpdate += PlayAnimation;
        EventBroker.Instance.OnDirectionChange += ChangeDirection;
    }

    void OnDisable() {
        EventBroker.Instance.OnPlayerStateUpdate -= PlayAnimation;
        EventBroker.Instance.OnDirectionChange -= ChangeDirection;
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
                if (!InAirJumpAnimation) {
                    _animator.Play(JumpAnimation);
                    SpawnDustEffect();
                }
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

    void ChangeDirection(int direction) {
        _faceDirection = direction;
    }
    
    void SpawnDustEffect() {
        float xOffSet = -_bounds.size.x * _faceDirection;
        Vector3 spawnPosition = transform.position + new Vector3(xOffSet, 0, 0);
        GameObject dust = Instantiate(dustEffect, spawnPosition, Quaternion.identity);
        dust.transform.localScale = new Vector3(_faceDirection, 1, 1);
    }
}
