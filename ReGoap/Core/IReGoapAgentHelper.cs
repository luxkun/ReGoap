using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// interface needed only in Unity to use GetComponent and such features for generic agents
public interface IReGoapAgentHelper
{
    Type[] GetGenericArguments();
}
