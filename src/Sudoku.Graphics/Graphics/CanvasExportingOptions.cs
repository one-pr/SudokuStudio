namespace Sudoku.Graphics;

/// <summary>
/// Represents exporting options.
/// </summary>
public sealed class CanvasExportingOptions
{
	/// <summary>
	/// Indicates the default options.
	/// </summary>
	public static readonly CanvasExportingOptions Default = new();


	/// <summary>
	/// Indicates the quality. Range 0..100. Default 80.
	/// </summary>
	public int Quality { get; init; } = 80;
}
