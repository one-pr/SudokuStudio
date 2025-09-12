#pragma warning disable CS1591

namespace System;

/// <summary>
/// Provides deprecated messages.
/// </summary>
public static class DeprecatedMessages
{
	public const string ExtensionOperator_Reverse = "Use extension operator 'operator ~' instead: '~collection'";

	public const string ExtensionOperator_Chunk = "Use extension operator 'operator /' instead: 'collection / size'";

	public const string ExtensionOperator_Subset = "Use extension operator 'operator &' instead: 'collection & size'";

	public const string ExtensionOperator_AllSubset = "Use extension operator 'operator |' instead: 'collection | size";

	public const string ExtensionOperator_Pack = "Use extension operator 'operator +' instead: '+characters'";

	public const string ExtensionOperator_Unpack = "Use extension operator 'operator -' instead: '-string'";

	//public const string ExtensionOperator_Equality = "Use extension operator 'operator ==' instead: 'left == right'";

	public const string ExtensionOperator_Apply = "Use extension operator 'operator >>=' instead: 'instance >>= value'";
}
