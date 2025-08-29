namespace Sudoku.Keywords;

/// <summary>
/// Represents an attribute type that describes the property is marked as special property to be filtered in reflection.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false)]
public sealed class KeywordAttribute : Attribute
{
	/// <summary>
	/// Indicates whether the range includes minimum value. By default it's <see langword="true"/>.
	/// </summary>
	public bool IncludesMinimum { get; init; } = true;

	/// <summary>
	/// Indicates whether the range includes maximum value. By default it's <see langword="false"/>.
	/// </summary>
	public bool IncludesMaximum { get; init; } = false;

	/// <summary>
	/// Indicates the minimum value. By default it's -1.
	/// </summary>
	public int Minimum { get; init; } = -1;

	/// <summary>
	/// Indicates the maximum value. By default it's -1.
	/// </summary>
	public int Maximum { get; init; } = -1;

	/// <summary>
	/// Indicates the name resource key to the property.
	/// </summary>
	public required string NameResourceKey { get; init; }

	/// <summary>
	/// Indicates the description resource key to the property. By default it's <see langword="null"/>.
	/// </summary>
	public string? DescriptionResourceKey { get; init; }

	/// <summary>
	/// Indicates the allowed verbs in runtime.
	/// </summary>
	public KeywordVerbs AllowedVerbs { get; init; }

	/// <summary>
	/// Indicates the meta type configured.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The meta type will be represented in runtime type checking and conversion,
	/// in order to achieve complex conversion behaviors from the specified value into built-in types.
	/// </para>
	/// <para>
	/// For example, if the target type mark this attribute is an enumeration type (like <see cref="Step.Code"/>),
	/// we can configure this property with value <see cref="KeywordType.String"/>,
	/// the runtime will use its string representation of target keyword property value (using <see cref="Enum.ToString()"/>)
	/// to convert the original value into built-in type <see cref="KeywordType.String"/>.
	/// </para>
	/// </remarks>
	/// <seealso cref="Step.Code"/>
	/// <seealso cref="Enum.ToString()"/>
	/// <seealso cref="KeywordType.String"/>
	public KeywordType MetaType { get; init; }

	/// <summary>
	/// Indicates keyword converter type.
	/// </summary>
	/// <value>
	/// The type to be assigned. The value shouldn't be <see langword="null"/> and must leave parameterless constructor.
	/// </value>
	/// <exception cref="InvalidKeywordConverterTypeException">
	/// Throws when the value is not derived from <see cref="KeywordValueConverter"/>,
	/// or doesn't contain a visible parameterless constructor to be invoked.
	/// </exception>
	[NotNull]
	[DisallowNull]
	public Type? KeywordConverterType
	{
		get;

		init
		{
			if (value.IsAssignableTo(typeof(KeywordValueConverter)) && value.HasParameterlessConstructor)
			{
				field = value;
				return;
			}
			throw new InvalidKeywordConverterTypeException();
		}
	}
}
