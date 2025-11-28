namespace Sudoku;

/// <summary>
/// Represents a type of technique.
/// </summary>
/// <remarks><include file="../../global-doc-comments.xml" path="/g/flags-attribute"/></remarks>
/// <completionlist cref="TechniqueTypes"/>
[Flags]
public enum TechniqueType
{
	/// <summary>
	/// Indicates placeholder.
	/// </summary>
	None = 0,

	/// <summary>
	/// Indicates the technique can be applied to direct mode.
	/// </summary>
	Direct = 1 << 0,

	/// <summary>
	/// Indicates the technique can be applied to Snyder's technique mode.
	/// </summary>
	Snyder = 1 << 1,

	/// <summary>
	/// Indicates the technique can be applied to advanced mode.
	/// </summary>
	Advanced = 1 << 2
}
