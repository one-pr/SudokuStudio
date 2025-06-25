namespace Sudoku.Ranking;

/// <summary>
/// Represents an option that checks for elimination zones.
/// </summary>
[Flags]
public enum EliminationZoneIgnoringOptions
{
	/// <summary>
	/// Indicates all elimination zones should be checked,
	/// including eliminations from sub-patterns and some invalid places (candidates not on links).
	/// </summary>
	None = 0,

	/// <summary>
	/// Indicates elimination zones will ignore external ones (i.e. candidates not on links).
	/// </summary>
	IgnoreExternal = 1 << 0,

	/// <summary>
	/// Indicates elimination zones will ignore ones produced by subpatterns of the pattern.
	/// For example, if a pattern contains a locked candidates, eliminations produced by this locked candidates
	/// can also be included.
	/// </summary>
	IgnoreSubpatterns = 1 << 1
}
