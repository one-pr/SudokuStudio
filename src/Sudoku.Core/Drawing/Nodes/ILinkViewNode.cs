namespace Sudoku.Drawing.Nodes;

/// <summary>
/// Represents a view node kind that is represented as a link.
/// </summary>
public interface ILinkViewNode
{
	/// <summary>
	/// Indicates the start element.
	/// </summary>
	object Start { get; }

	/// <summary>
	/// Indicates the end element.
	/// </summary>
	object End { get; }

	/// <summary>
	/// Indicates the link shape.
	/// </summary>
	LinkShape Shape { get; }

	/// <summary>
	/// Indicates the color identifier.
	/// </summary>
	ColorDescriptor Identifier { get; }
}
