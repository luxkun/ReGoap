
namespace ReGoap.Unity.Test
{
    public class ReGoapTestMemory : ReGoapMemory<string, object>
    {
        public void Init()
        {
            Awake();
        }

        public void SetValue(string key, object value)
        {
            state.Set(key, value);
        }
    }
}