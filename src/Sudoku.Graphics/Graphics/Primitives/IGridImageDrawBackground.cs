namespace Sudoku.Graphics.Primitives;

/// <summary>
/// Provides background drawing method set.
/// </summary>
internal interface IGridImageDrawBackground
{
	/// <summary>
	/// Draw background.
	/// </summary>
	/// <param name="color">Color.</param>
	void DrawBackground(SKColor color);
}
