using UnityEngine;


[CreateAssetMenu(fileName = "AudioClipSO", menuName = "Scriptable Objects/AudioClipSO")]
public class AudioClipSO : ScriptableObject
{
    public AudioClip buttonClick;
    public AudioClip swordClash;

    public AudioClip desertAmbience;
    public AudioClip normalAmbience;
    public AudioClip medievalAmbience;
}
