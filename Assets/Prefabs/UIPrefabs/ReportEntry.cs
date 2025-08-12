using System;
using UnityEngine;

[System.Serializable]
public class ReportEntry
{
    public string ReportName;
    public string Team;
    public string ActionType;
    public string Description;
    public GameObject DisplayObject;

    // Event to notify when this ReportEntry is destroyed
    public event Action OnDestroyed;

    // Call this method to trigger the OnDestroyed event
    public void Destroy()
    {
        // If you need to do cleanup before destroying, add here

        // Notify subscribers that this ReportEntry is destroyed
        OnDestroyed?.Invoke();

        // Optionally, destroy the associated GameObject if needed
        if (DisplayObject != null)
        {
            UnityEngine.Object.Destroy(DisplayObject);
        }
    }
}