namespace Sudoku.Graphics.Primitives;

/// <summary>
/// Provides text drawing method set.
/// </summary>
public interface IGridCanvasDrawText
{
	/// <summary>
	/// Draw grid text (given digits, modifiable digits and candidates). Properties in <paramref name="options"/> used:
	/// <list type="bullet">
	/// <item><see cref="CanvasDrawingOptions.GivenDigitsFontName"/></item>
	/// <item><see cref="CanvasDrawingOptions.GivenDigitsFontSize"/></item>
	/// <item><see cref="CanvasDrawingOptions.GivenDigitsColor"/></item>
	/// <item><see cref="CanvasDrawingOptions.ModifiableDigitsFontName"/></item>
	/// <item><see cref="CanvasDrawingOptions.ModifiableDigitsFontSize"/></item>
	/// <item><see cref="CanvasDrawingOptions.ModifiableDigitsColor"/></item>
	/// <item><see cref="CanvasDrawingOptions.CandidatesFontName"/></item>
	/// <item><see cref="CanvasDrawingOptions.CandidatesFontSize"/></item>
	/// <item><see cref="CanvasDrawingOptions.CandidatesColor"/></item>
	/// </list>
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="options">
	/// Indicates the options.
	/// </param>
	void DrawGrid(in Grid grid, CanvasDrawingOptions? options = null);
}
