namespace Sudoku.Analytics.Ranking;

/// <summary>
/// Represents an option that checks for elimination zones.
/// </summary>
[Flags]
public enum EliminationZoneOptions
{
	/// <summary>
	/// Indicates the placeholder of the pattern.
	/// </summary>
	None = 0,

	/// <summary>
	/// Indicates all elimination zones should be checked,
	/// including eliminations from sub-patterns and some invalid places (candidates not on links).
	/// </summary>
	All = 1 << 0,

	/// <summary>
	/// <para>
	/// Indicates elimination zones will ignore external ones (i.e. candidates not on links).
	/// </para>
	/// <para>If this option is specified, it'll be okay to ignore specifying field <see cref="All"/>.</para>
	/// </summary>
	IgnoreExternal = 1 << 1,

	/// <summary>
	/// <para>
	/// Indicates elimination zones will ignore ones produced by subpatterns of the pattern.
	/// For example, if a pattern contains a locked candidates, eliminations produced by this locked candidates
	/// can also be included.
	/// </para>
	/// <para>If this option is specified, it'll be okay to ignore specifying field <see cref="All"/>.</para>
	/// </summary>
	IgnoreSubpatterns = 1 << 2
}
