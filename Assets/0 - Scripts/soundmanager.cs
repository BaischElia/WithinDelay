using System.Security.Cryptography;
using UnityEngine;

public class soundmanager : MonoBehaviour
{

    public AudioSource soundSource;
    public AudioClip hitSound;
    public AudioClip shootSound;
    public AudioClip passwordSound;

    void Start()
    {
        hitSound.LoadAudioData();
        shootSound.LoadAudioData();
        passwordSound.LoadAudioData();
    }

    void Update()
    {
        
    }

    // gets called with button click in the startUI
    public void PlayStartPassword() {
        soundSource.PlayOneShot(passwordSound, 1f);
        
    }

    public void PlayHitSound() {
        soundSource.PlayOneShot(hitSound, 1f);
    }

    public void PlayShootSound() {
        soundSource.PlayOneShot(shootSound, 1f);
    }
}
