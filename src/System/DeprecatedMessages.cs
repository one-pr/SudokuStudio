namespace System;

/// <summary>
/// Provides deprecated messages.
/// </summary>
internal static class DeprecatedMessages
{
	public const string ExtensionOperator_Reverse = "Use extension operator 'operator ~' instead: '~collection'";

	public const string ExtensionOperator_Chunk = "Use extension operator 'operator /' instead: 'collection / size'";

	public const string ExtensionOperator_Subset = "Use extension operator 'operator &' instead: 'collection & size'";

	public const string ExtensionOperator_AllSubset = "Use extension operator 'operator |' instead: 'collection | size";
}
