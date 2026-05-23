using System;

public abstract class ReapyControllerCommand
{
    public string command;
    public string SoundUUID;

}

public class NewTrack: ReapyControllerCommand {
    
    public string AudioID;
    public float Volume;

    public bool Loop;
    public uint Delay;
}

public class SourcePosition: ReapyControllerCommand
{
    public float azim; //Azimuth
    public float elev; //elevation
    public float distance;
}

public class DeleteTrack: ReapyControllerCommand{}

public class MuteTrack: ReapyControllerCommand{}

public class UnmuteTrack: ReapyControllerCommand{}

public class ResetReapyController
{
    public string command;
}

//Effects
public class AudioEffect: ReapyControllerCommand
{
    public string EffectName;
    public string[] EffectParams;
}

public class removeAudioEffect: ReapyControllerCommand
{
    public string EffectName;
}