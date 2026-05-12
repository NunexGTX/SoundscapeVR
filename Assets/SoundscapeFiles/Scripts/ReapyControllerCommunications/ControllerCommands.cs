using System;

public abstract class ReapyControllerCommand
{
    public string command;
    public string AudioID;
}

public class NewTrack: ReapyControllerCommand {
    
    public string SoundUUID;
    public bool Loop;
}

public class SourcePosition: ReapyControllerCommand
{
    public (float, float) position;
    public float distance;
}

public class DeleteTrack: ReapyControllerCommand{}

public class ResetReapyController
{
    public string command;
}