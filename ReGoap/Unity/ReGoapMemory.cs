using ReGoap.Core;
using UnityEngine;

namespace ReGoap.Unity
{
    public class ReGoapMemory<T, W> : MonoBehaviour, IReGoapMemory<T, W>
    {
        protected ReGoapState<T, W> state;

        #region UnityFunctions
        protected virtual void Awake()
        {
            state = ReGoapState<T, W>.Instantiate();
        }

        protected virtual void OnDestroy()
        {
            state.Recycle();
        }

        protected virtual void Start()
        {
        }
        #endregion

        public virtual ReGoapState<T, W> GetWorldState()
        {
            return state;
        }
    }
}
