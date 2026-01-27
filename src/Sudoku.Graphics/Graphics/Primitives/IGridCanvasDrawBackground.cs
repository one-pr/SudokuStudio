namespace Sudoku.Graphics.Primitives;

/// <summary>
/// Provides background drawing method set.
/// </summary>
public interface IGridCanvasDrawBackground
{
	/// <summary>
	/// Draw background. Properties in <paramref name="options"/> used:
	/// <list type="bullet">
	/// <item><see cref="ImageDrawingOptions.BackgroundColor"/></item>
	/// </list>
	/// </summary>
	/// <param name="options">Indicates the options.</param>
	void DrawBackground(ImageDrawingOptions? options = null);
}
