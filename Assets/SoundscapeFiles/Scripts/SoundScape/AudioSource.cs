
using UnityEngine;

[System.Serializable]
public class AudioSource
{
    [SerializeField] private Audio audio;
    [SerializeField] private SerializableVector3 spherePosition;

    public AudioSource(Audio audio, Vector3 spherePosition)
    {
        this.audio = audio;
        this.spherePosition = new(spherePosition);
    }

    public Vector3 GetSpherePosition()
    {
        return spherePosition.ToVector3();
    }

}