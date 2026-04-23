using System;

public class WorldAudioSource
{
    private AudioSource audioSource;
    private Guid uuid;

    public SoundscapeWrapper soundscapeWrapper;

    public void DeleteSound(){}

    public WorldAudioSource(AudioSource src, Guid audioId)
    {
        this.audioSource = src;
        this.uuid = audioId;
    }
}