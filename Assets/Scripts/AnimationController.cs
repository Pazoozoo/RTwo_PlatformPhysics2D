using UnityEngine;

public class AnimationController : MonoBehaviour {
    [SerializeField] GameObject dustEffect;
    
    Animator _animator;
    Bounds _bounds;
    float _airJumpTime;
    float _airJumpLength;
    float _dustTime;    
    const float WallDustSpawnInterval = 0.17f;
    
    const string IdleAnimation = "idle";
    const string RunAnimation = "run";
    const string JumpAnimation = "jump";
    const string AirJumpAnimation = "double_jump";
    const string WallSlideAnimation = "wall_slide";
    const string DieAnimation = "die";

    bool InAirJumpAnimation => Time.time < _airJumpTime + _airJumpLength;
    bool DustAnimationPlaying => Time.time < _dustTime + WallDustSpawnInterval;

    void Awake() {
        _animator = GetComponent<Animator>();
        _bounds = GetComponent<Collider2D>().bounds;
    }

    void OnEnable() {
        EventBroker.Instance.OnPlayerStateUpdate += PlayAnimation;
        EventBroker.Instance.OnWallSlide += PlayWallSlideDustEffect;
        EventBroker.Instance.OnJump += PlayJumpDustEffect;
    }

    void OnDisable() {
        EventBroker.Instance.OnPlayerStateUpdate -= PlayAnimation;      
        EventBroker.Instance.OnWallSlide -= PlayWallSlideDustEffect;
        EventBroker.Instance.OnJump -= PlayJumpDustEffect;
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

    void PlayJumpDustEffect(int direction) {
        SpawnDustEffect(direction);
    }
    
    void PlayWallSlideDustEffect(int direction) {
        if (DustAnimationPlaying) return;
        SpawnDustEffect(-direction, true);
        _dustTime = Time.time;
    }

    void SpawnDustEffect(int direction, bool vertical = false) {
        float xOffSet = vertical ? 0f : -_bounds.size.x * direction;
        float yOffSet = vertical ? _bounds.extents.y : 0f;
        Vector3 spawnPosition = transform.position + new Vector3(xOffSet, yOffSet, 0f);
        Quaternion rotation = vertical ? Quaternion.Euler(0f, 0f, -90f * direction) : Quaternion.identity;
        GameObject dust = Instantiate(dustEffect, spawnPosition, rotation);
        dust.transform.localScale = new Vector3(direction, 1f, 1f);
    }
}
