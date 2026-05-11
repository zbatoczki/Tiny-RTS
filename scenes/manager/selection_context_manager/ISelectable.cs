namespace Game.Selection;

/// <summary>
/// Implemented by any entity that can be selected by the player (units, buildings, etc.). 
/// </summary>
public interface ISelectable
{
    void SetSelected(bool selected);
    bool IsSelected { get; }
}