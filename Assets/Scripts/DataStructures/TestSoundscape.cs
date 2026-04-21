using System;
using UnityEngine;

public class TestSoundscape
{
    public GameObject video360;
    public string title;
    public int RoomLayerIndex = 6;
    public PlayerControl player;

    public TestSoundscape(string title, GameObject video360)
    {
        this.video360 = video360;
        this.title = title;
    }

    public void WithParameters(PlayerControl player, int roomLayerIndex)
    {
        this.player = player;
        this.RoomLayerIndex = roomLayerIndex;
    }

    
}
