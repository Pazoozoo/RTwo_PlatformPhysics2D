using System;
using UnityEngine;

public class SoundController : MonoBehaviour {
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioSource loopingAudioSource;
    [SerializeField] AudioClip jump;
    [SerializeField] AudioClip airJump;
    [SerializeField] AudioClip landing;
    [SerializeField] AudioClip wallSlide;
    [SerializeField] AudioClip die;

    void Start() {
        EventBroker.Instance.OnJump += PlayJumpSound;
        EventBroker.Instance.OnAirJump += PlayAirJumpSound;
        EventBroker.Instance.OnDeath += PlayDieSound;
        EventBroker.Instance.OnWallSlide += PlayWallSlideSound;
        EventBroker.Instance.OnWallSlideStop += StopSound;
        EventBroker.Instance.OnLanding += PlayLandingSound;
    }

    void OnDestroy() {
        EventBroker.Instance.OnJump -= PlayJumpSound;
        EventBroker.Instance.OnAirJump -= PlayAirJumpSound;
        EventBroker.Instance.OnDeath -= PlayDieSound;
        EventBroker.Instance.OnWallSlide -= PlayWallSlideSound;
        EventBroker.Instance.OnWallSlideStop -= StopSound;
        EventBroker.Instance.OnLanding -= PlayLandingSound;
    }

    void PlayJumpSound(int i) {
        audioSource.PlayOneShot(jump, 0.15f);
    }

    void PlayAirJumpSound() {
        audioSource.PlayOneShot(airJump, 0.2f);
    }

    void PlayDieSound() {
        audioSource.PlayOneShot(die, 0.8f);
    }

    void PlayLandingSound() {
        audioSource.PlayOneShot(landing, 0.4f);
    }

    void PlayWallSlideSound(int i) {
        if (loopingAudioSource.isPlaying && loopingAudioSource.clip == wallSlide) 
            return;
        
        loopingAudioSource.clip = wallSlide;
        loopingAudioSource.Play();
    }

    void StopSound() {
        loopingAudioSource.Stop();
    }
}
