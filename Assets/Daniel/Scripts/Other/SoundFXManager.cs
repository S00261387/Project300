using UnityEngine;

public class SoundFXManager : MonoBehaviour
{
public static SoundFXManager Instance;

    [SerializeField] private AudioSource soundFXObject;

    private void Awake()
    {
        if (Instance == null) 
        { 
        Instance = this;
        }
    }

    public void PlaySoundFXClip(AudioClip clip, Transform spawnTransform)
    {
        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);

        audioSource.clip = clip;

        audioSource.Play();

        float clipLength = audioSource.clip.length;

        Destroy(audioSource.gameObject, clipLength );
    }

    public void PlayRandomSoundFXClip(AudioClip[] clip, Transform spawnTransform)
    {
        int rand = Random.Range( 0, clip.Length );

        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);

        audioSource.clip = clip[rand];

        audioSource.Play();

        float clipLength = audioSource.clip.length;

        Destroy(audioSource.gameObject, clipLength);
    }
}
