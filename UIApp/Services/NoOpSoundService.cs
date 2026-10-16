using System.Threading.Tasks;

namespace UIApp.Services
{
    public class NoOpSoundService : ISoundService
    {
        public Task PlayShortBeepAsync() => Task.CompletedTask;
    }
}