using UnityEngine;

[System.Serializable]
public class AudioSource
{
    [SerializeField] private string audioId;
    [SerializeField] private SerializableVector3 spherePosition;

    public AudioSource(string audioId, Vector3 spherePosition)
    {
        this.audioId = audioId;
        this.spherePosition = new(spherePosition);
    }
    public AudioSource(Audio audio, Vector3 spherePosition)
    {
        this.audioId = audio.id;
        this.spherePosition = new(spherePosition);
    }
    public string GetAudioId(){return audioId;}

    public Vector3 GetSpherePosition()
    {
        return spherePosition.ToVector3();
    }

    public void WithNewPosition(Vector3 newCoords)
    {
        this.spherePosition = new(newCoords);
    }
}