using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// this is not strictly needed for a goap AI, but can be userful if your memory has many states and 
//  you want to re-use different sensors in different agents
// the interface does not dictate how you should update the memory from the sensor
// - in a unity game probably you will want to update the memory in the sensor's Update/FixedUpdate
public interface IReGoapSensor
{
    void Init(IReGoapMemory memory);
    IReGoapMemory GetMemory();
    void UpdateSensor();
}
