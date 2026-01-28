namespace Sudoku.Graphics.Primitives;

/// <summary>
/// Provides line drawing method set.
/// </summary>
public interface IGridCanvasDrawLine
{
	/// <summary>
	/// Draw grid lines. Properties in <paramref name="options"/> used:
	/// <list type="bullet">
	/// <item><see cref="CanvasDrawingOptions.BlockLineStrokeThicknessRatio"/></item>
	/// <item><see cref="CanvasDrawingOptions.BlockLineStrokeColor"/></item>
	/// <item><see cref="CanvasDrawingOptions.BlockLineDashSequence"/></item>
	/// <item><see cref="CanvasDrawingOptions.GridLineStrokeThicknessRatio"/></item>
	/// <item><see cref="CanvasDrawingOptions.GridLineStrokeColor"/></item>
	/// <item><see cref="CanvasDrawingOptions.GridLineDashSequence"/></item>
	/// <item><see cref="CanvasDrawingOptions.CandidateAuxiliaryLineStrokeThicknessRatio"/></item>
	/// <item><see cref="CanvasDrawingOptions.CandidateAuxiliaryLineStrokeColor"/></item>
	/// <item><see cref="CanvasDrawingOptions.CandidateAuxiliaryLineDashSequence"/></item>
	/// </list>
	/// </summary>
	/// <param name="options">Indicates the options.</param>
	void DrawGridLine(CanvasDrawingOptions? options = null);
}
