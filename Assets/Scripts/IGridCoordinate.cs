using UnityEngine;
/// <summary>
/// Interface to aid GRID coordination system for slots
/// </summary>
public interface IGridCoordinate
{
    int PosX { get; }
    int PosZ { get; }

    /// <summary>
    /// Check if slot has occupied by road, building, event, or obstacle
    /// </summary>
    bool HasPlaced { get; }

    /// <summary>
    /// Set coordinate according to GRID coordinate system
    /// </summary>
    /// <param name="x">The horizontal coordinate</param>
    /// <param name="z">The vertical coordinate</param>
    void SetCoordinate(int x, int z);
    GameObject GetObjectToHide();
}
