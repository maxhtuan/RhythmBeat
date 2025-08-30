public interface IGameMode
{
    string ModeName { get; }
    void Initialize();
    void TriggerThisMode();
    void Pause();
    void Resume();
    void End();
    void OnBeatHit();
}
