using UnityEngine;

public class AudioManager : MonoBehaviour
{
    #region Singleton
    public static AudioManager Instance { get; private set; }
    #endregion


    private AudioSource audioSource;
    [SerializeField] private AudioClipSO audioClipSO;

    private AudioClip ambientAudioForThisScene;

    public enum AmbientAudios
    {
        DesertAmbience,
        MedievalAmbience,
        NormalAmbience,
    }
    public AmbientAudios ambientAudio;



    private void Awake()
    {
        Instance = this;
        audioSource = GetComponent<AudioSource>();

        switch (ambientAudio)
        {
            default:
            case AmbientAudios.NormalAmbience:
                ambientAudioForThisScene = audioClipSO.normalAmbience;
                break;
            case AmbientAudios.DesertAmbience:
                ambientAudioForThisScene = audioClipSO.desertAmbience;
                break;
            case AmbientAudios.MedievalAmbience:
                ambientAudioForThisScene = audioClipSO.medievalAmbience;
                break;
        }


        PlayAmbientAudio();
    }


    public void PlayAmbientAudio()
    {
        audioSource.PlayOneShot(ambientAudioForThisScene);
    }


    public void PlaySwordClashSound()
    {
        audioSource.PlayOneShot(audioClipSO.swordClash);
    }
}
