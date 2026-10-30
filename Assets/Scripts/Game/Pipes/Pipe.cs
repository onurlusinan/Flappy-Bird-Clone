using Common.Core.Pooling;

namespace Game.Pipes
{
    // The root's trigger is the scoring gap; the two pipe halves underneath carry Hazard.
    public sealed class Pipe : PoolableObject<GamePoolKey>
    {
        protected override void OnGetFromPool()
        {
        }

        protected override void OnBackToPool()
        {
        }
    }
}
