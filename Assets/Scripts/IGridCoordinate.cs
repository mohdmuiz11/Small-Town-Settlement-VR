/// <summary>
/// Interface to aid GRID coordination system for slots
/// </summary>
public interface IGridCoordinate
{
    int posX { get; }
    int posZ { get; }
    bool hasPlaced { get; }

    /// <summary>
    /// Set coordinate according to GRID coordinate system
    /// </summary>
    /// <param name="x">The horizontal coordinate</param>
    /// <param name="z">The vertical coordinate</param>
    void SetCoordinate(int x, int z);
}
