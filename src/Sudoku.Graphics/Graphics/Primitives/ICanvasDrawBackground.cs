namespace Sudoku.Graphics.Primitives;

/// <summary>
/// Provides background drawing method set.
/// </summary>
public interface ICanvasDrawBackground
{
	/// <summary>
	/// Draw background. Properties in <paramref name="options"/> used:
	/// <list type="bullet">
	/// <item><see cref="CanvasDrawingOptions.BackgroundColor"/></item>
	/// </list>
	/// </summary>
	/// <param name="options">Indicates the options.</param>
	void DrawBackground(CanvasDrawingOptions? options = null);
}
