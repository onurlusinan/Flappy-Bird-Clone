using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Common.Core
{
    // Non-generic so SceneManager can locate a scene's controller without knowing its Context type.
    public abstract class ContextControllerBase : MonoBehaviour
    {
        public abstract UniTask Ready { get; }

        // Called by SceneManager on the outgoing scene's controller before it unloads.
        public abstract UniTask Deactivate();
    }
}