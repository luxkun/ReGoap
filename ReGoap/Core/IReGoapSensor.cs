// this is not strictly needed for a goap AI, but can be userful if your memory has many states and 
//  you want to re-use different sensors in different agents
// the interface does not dictate how you should update the memory from the sensor
// - in a unity game probably you will want to update the memory in the sensor's Update/FixedUpdate
namespace ReGoap.Core
{
    public interface IReGoapSensor<T, W>
    {
        void Init(IReGoapMemory<T, W> memory);
        IReGoapMemory<T, W> GetMemory();
        void UpdateSensor();
    }
}
