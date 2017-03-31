
public class ReGoapTestMemory : GoapMemory
{
    public void Init()
    {
        Awake();
    }

    public void SetValue<T>(string key, T value)
    {
        state.Set(key, value);
    }
}