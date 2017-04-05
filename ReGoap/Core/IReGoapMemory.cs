namespace ReGoap.Core
{
    public interface IReGoapMemory<T, W>
    {
        ReGoapState<T, W> GetWorldState();
    }
}