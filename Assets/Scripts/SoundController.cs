using System;
using UnityEngine;

public class SoundController : MonoBehaviour {
    [SerializeField] AudioClip Jump;
    [SerializeField] AudioClip AirJump;
    [SerializeField] AudioClip Land;
    [SerializeField] AudioClip WallSlide;
    [SerializeField] AudioClip Die;

    AudioSource _audioSource;

    void Start() {
        _audioSource = GetComponent<AudioSource>();
        EventBroker.Instance.OnJump += PlayJumpSound;
        EventBroker.Instance.OnAirJump += PlayAirJumpSound;
        EventBroker.Instance.OnDeath += PlayDieSound;
    }

    void OnDestroy() {
        EventBroker.Instance.OnJump -= PlayJumpSound;
        EventBroker.Instance.OnAirJump -= PlayAirJumpSound;
        EventBroker.Instance.OnDeath -= PlayDieSound;
    }

    void PlayJumpSound(int i) {
        _audioSource.PlayOneShot(Jump, 0.15f);
    }

    void PlayAirJumpSound() {
        _audioSource.PlayOneShot(AirJump, 0.2f);
    }

    void PlayDieSound() {
        _audioSource.PlayOneShot(Die, 0.8f);
    }
}
