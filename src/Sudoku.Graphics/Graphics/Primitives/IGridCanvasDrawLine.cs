namespace Sudoku.Graphics.Primitives;

/// <summary>
/// Provides line drawing method set.
/// </summary>
public interface IGridCanvasDrawLine
{
	/// <summary>
	/// Draw grid lines. Properties in <paramref name="options"/> used:
	/// <list type="bullet">
	/// <item><see cref="ImageDrawingOptions.BlockLineStrokeThickness"/></item>
	/// <item><see cref="ImageDrawingOptions.BlockLineStrokeColor"/></item>
	/// <item><see cref="ImageDrawingOptions.BlockLineDashSequence"/></item>
	/// <item><see cref="ImageDrawingOptions.GridLineStrokeThickness"/></item>
	/// <item><see cref="ImageDrawingOptions.GridLineStrokeColor"/></item>
	/// <item><see cref="ImageDrawingOptions.GridLineDashSequence"/></item>
	/// <item><see cref="ImageDrawingOptions.CandidateAuxiliaryLineStrokeThickness"/></item>
	/// <item><see cref="ImageDrawingOptions.CandidateAuxiliaryLineStrokeColor"/></item>
	/// <item><see cref="ImageDrawingOptions.CandidateAuxiliaryLineDashSequence"/></item>
	/// </list>
	/// </summary>
	/// <param name="options">Indicates the options.</param>
	void DrawGridLine(ImageDrawingOptions? options = null);
}
