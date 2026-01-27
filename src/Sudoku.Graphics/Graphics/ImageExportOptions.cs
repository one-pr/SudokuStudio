namespace Sudoku.Graphics;

/// <summary>
/// Represents image export options.
/// </summary>
public sealed class ImageExportOptions
{
	/// <summary>
	/// Indicates the default options.
	/// </summary>
	public static readonly ImageExportOptions Default = new();


	/// <summary>
	/// Indicates the quality. Range 0..100. Default 80.
	/// </summary>
	public int Quality { get; init; } = 80;
}
