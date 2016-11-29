using UnityEngine;
using System.Collections;

public class WorkstationsManager : MonoBehaviour
{
    public static WorkstationsManager instance;
    public Workstation[] workstations;
    private int currentIndex;

    protected virtual void Awake()
    {
        if (instance != null)
            throw new UnityException("[WorkstationsManager] Can have only one instance per scene.");
        instance = this;
    }

    public Workstation GetWorkstation()
    {
        var result = workstations[currentIndex];
        currentIndex = currentIndex++ % workstations.Length;
        return result;
    }
}
