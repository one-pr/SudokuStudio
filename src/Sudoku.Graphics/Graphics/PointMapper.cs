namespace Sudoku.Graphics;

/// <summary>
/// Represents a mapper instance that projects from pixel points into sudoku elements (coordinates),
/// or from sudoku elements into points.
/// </summary>
/// <param name="size"><inheritdoc cref="Size" path="/summary"/></param>
/// <param name="margin"><inheritdoc cref="Margin" path="/summary"/></param>
public sealed class PointMapper(int size, float margin)
{
	/// <summary>
	/// Indicates size of the picture.
	/// </summary>
	public int Size { get; } = size;

	/// <summary>
	/// Indicates margin of the picture.
	/// </summary>
	public float Margin { get; } = margin;
}
