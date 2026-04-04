namespace System.Reflection;

/// <summary>
/// Represents a type of extension members.
/// </summary>
[Flags]
public enum ExtensionMemberTypes
{
	/// <summary>
	/// Indicates none of all members.
	/// </summary>
	None = 0,

	/// <summary>
	/// Indicates instance properties.
	/// </summary>
	Properties = 1 << 0,

	/// <summary>
	/// Indicates static properties.
	/// </summary>
	StaticProperties = 1 << 1,

	/// <summary>
	/// Indicates indexers. Not implemented in C# 14.
	/// </summary>
	Indexers = 1 << 2,

	/// <summary>
	/// Indicates instance methods.
	/// </summary>
	Methods = 1 << 3,

	/// <summary>
	/// Indicates static methods.
	/// </summary>
	StaticMethods = 1 << 4,

	/// <summary>
	/// Indicates compound assignment operators.
	/// </summary>
	CompoundAssignmentOperators = 1 << 5,

	/// <summary>
	/// Indicates operators.
	/// </summary>
	Operators = 1 << 6
}
