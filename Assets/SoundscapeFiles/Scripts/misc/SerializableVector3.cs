using UnityEngine;

[System.Serializable]
public struct SerializableVector3
{
    public float x; 
    public float y;
    public float z;

    // Helper constructor to convert from Unity Vector3
    public SerializableVector3(Vector3 v3)
    {
        x = v3.x;
        y = v3.y;
        z = v3.z;
    }

    // Helper method to convert back to Unity Vector3
    public Vector3 ToVector3() => new Vector3(x, y, z);
}