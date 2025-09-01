using System.Threading.Tasks;
using System.Threading;
public interface IGameMode
{
    string ModeName { get; }
    void Initialize();
    void TriggerThisMode();
    void Pause();
    void Resume();
    void End();
    Task OnBeatHit();
    void OnNoteMissed();
}
