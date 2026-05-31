using System;

public abstract class ReapyControllerCommand
{
    public string command;

}

public abstract class TrackCommand: ReapyControllerCommand
{
    public string SoundUUID;
}

public class NewSoundscape: ReapyControllerCommand
{
    public int Duration;
    public bool Loop;
}

public class StartSoundscape: ReapyControllerCommand{}

public class StopSoundscape: ReapyControllerCommand{}

public class NewTrack: TrackCommand {
    
    public string AudioID;
    public float Volume;

    public bool Loop;
    public uint Delay;
}

public class SourcePosition: TrackCommand
{
    public float azim; //Azimuth
    public float elev; //elevation
    public float distance;
}

public class DeleteTrack: TrackCommand{}

public class MuteTrack: TrackCommand{}

public class UnmuteTrack: TrackCommand{}

public class ResetReapyController
{
    public string command;
}

//Effects
public class AudioEffect: TrackCommand
{
    public string EffectName;
    public string[] EffectParams;
}

public class removeAudioEffect: TrackCommand
{
    public string EffectName;
}