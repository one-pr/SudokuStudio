namespace System.Diagnostics;

/// <summary>
/// Provides deprecated messages.
/// </summary>
public static class DeprecatedMessages
{
	/// <summary>
	/// Describes extension "<c><see langword="operator"/> ~</c>" on collections to reverse them.
	/// </summary>
	public const string ExtensionOperator_Reverse = "Use extension operator 'operator ~' instead: '~collection'";

	/// <summary>
	/// Describes extension "<c><see langword="operator"/> /</c>" on collections to get chunks of the specified collection.
	/// </summary>
	public const string ExtensionOperator_Chunk = "Use extension operator 'operator /' instead: 'collection / size'";

	/// <summary>
	/// Describes extension "<c><see langword="operator"/> &amp;</c>" on collections to get subsets from them.
	/// </summary>
	public const string ExtensionOperator_Subset = "Use extension operator 'operator &' instead: 'collection & size'";

	/// <summary>
	/// Describes extension "<c><see langword="operator"/> |</c>" on collections to get subsets from them.
	/// </summary>
	public const string ExtensionOperator_AllSubset = "Use extension operator 'operator |' instead: 'collection | size";

	/// <summary>
	/// Describes extension "<c><see langword="operator"/> +</c>" on characters to get packed string.
	/// </summary>
	public const string ExtensionOperator_Pack = "Use extension operator 'operator +' instead: '+characters'";

	/// <summary>
	/// Describes extension "<c><see langword="operator"/> -</c>" on string to get unpacked sequence.
	/// </summary>
	public const string ExtensionOperator_Unpack = "Use extension operator 'operator -' instead: '-string'";

	/// <summary>
	/// Describes extension "<c><see langword="operator"/> ==</c>" on instances to compare equality.
	/// </summary>
	public const string ExtensionOperator_Equality = "Use extension operator 'operator ==' instead: 'left == right'";

	/// <summary>
	/// Describes extension "<c><see langword="operator"/> *</c>" on collections to repeat specified times of them.
	/// </summary>
	public const string ExtensionOperator_Repeat = "Use extension operator 'operator *' instead: 'string * times'";
}
