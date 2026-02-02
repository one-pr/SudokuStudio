namespace Sudoku.Graphics.Primitives;

/// <summary>
/// Provides text drawing method set.
/// </summary>
public interface ICanvasDrawText
{
	/// <summary>
	/// Draw grid text (given digits, modifiable digits and candidates). Properties in <paramref name="options"/> used:
	/// <list type="bullet">
	/// <item><see cref="CanvasDrawingOptions.GivenDigitsFontName"/></item>
	/// <item><see cref="CanvasDrawingOptions.GivenDigitsFontSizeRatio"/></item>
	/// <item><see cref="CanvasDrawingOptions.GivenDigitsColor"/></item>
	/// <item><see cref="CanvasDrawingOptions.GivenDigitsFontWeight"/></item>
	/// <item><see cref="CanvasDrawingOptions.GivenDigitsFontWidth"/></item>
	/// <item><see cref="CanvasDrawingOptions.GivenDigitsFontSlant"/></item>
	/// <item><see cref="CanvasDrawingOptions.ModifiableDigitsFontName"/></item>
	/// <item><see cref="CanvasDrawingOptions.ModifiableDigitsFontSizeRatio"/></item>
	/// <item><see cref="CanvasDrawingOptions.ModifiableDigitsColor"/></item>
	/// <item><see cref="CanvasDrawingOptions.ModifiableDigitsFontWeight"/></item>
	/// <item><see cref="CanvasDrawingOptions.ModifiableDigitsFontWidth"/></item>
	/// <item><see cref="CanvasDrawingOptions.ModifiableDigitsFontSlant"/></item>
	/// <item><see cref="CanvasDrawingOptions.CandidatesFontName"/></item>
	/// <item><see cref="CanvasDrawingOptions.CandidatesFontSizeRatio"/></item>
	/// <item><see cref="CanvasDrawingOptions.CandidatesColor"/></item>
	/// <item><see cref="CanvasDrawingOptions.CandidatesFontWeight"/></item>
	/// <item><see cref="CanvasDrawingOptions.CandidatesFontWidth"/></item>
	/// <item><see cref="CanvasDrawingOptions.CandidatesFontSlant"/></item>
	/// </list>
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="options">
	/// Indicates the options.
	/// </param>
	void DrawGrid(in Grid grid, CanvasDrawingOptions? options = null);
}
