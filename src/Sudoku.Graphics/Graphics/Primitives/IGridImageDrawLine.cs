namespace Sudoku.Graphics.Primitives;

/// <summary>
/// Provides line drawing method set.
/// </summary>
internal interface IGridImageDrawLine
{
	/// <summary>
	/// Draw grid lines.
	/// </summary>
	/// <param name="options">Indicates the options.</param>
	void DrawGridLine(ImageDrawingOptions? options = null);
}
