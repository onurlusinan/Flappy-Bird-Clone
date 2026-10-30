namespace Common.Core.Saving
{
    // Implemented by any system whose state persists; SaveService treats payloads as opaque JSON.
    public interface ISaveParticipant
    {
        string SaveKey { get; }
        string CaptureState();
        void RestoreState(string json);
    }
}