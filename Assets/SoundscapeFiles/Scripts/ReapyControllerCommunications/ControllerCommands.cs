public abstract class ReapyControllerCommand
{
    public string command;
}

public class NewTrack: ReapyControllerCommand {
    public string AudioID;
    public string SoundUUID;
}
