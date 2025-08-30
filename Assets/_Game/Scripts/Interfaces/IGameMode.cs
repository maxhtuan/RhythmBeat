public interface IGameMode
{
    string ModeName { get; }
    void Initialize();
    void Start();
    void Pause();
    void Resume();
    void End();
    void OnBeatHit();
}
