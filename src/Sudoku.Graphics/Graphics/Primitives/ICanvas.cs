namespace Sudoku.Graphics.Primitives;

/// <summary>
/// Represents required members on a canvas.
/// </summary>
public interface ICanvas :
	IDisposable,
	ICanvasDrawBackground,
	ICanvasDrawLine,
	ICanvasDrawText,
	ICanvasExport
{
	/// <summary>
	/// Indicates whether the object is disposed or not. This property should be explicitly-implemented.
	/// </summary>
	bool IsDisposed { get; }

	/// <summary>
	/// Indicates the mapper.
	/// </summary>
	PointMapper Mapper { get; }

	/// <summary>
	/// Indicates the canvas that allows you drawing on it.
	/// </summary>
	SKCanvas BackingCanvas { get; }
}
