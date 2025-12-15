namespace Sudoku.Descriptors;

/// <summary>
/// Represents a converter object that supports serialization / deserialization operation
/// (consumed by type <see cref="JsonSerializer"/>),
/// and formatting into string value (consumed by <see cref="object.ToString"/>).
/// </summary>
/// <seealso cref="JsonSerializer"/>
/// <seealso cref="object.ToString"/>
public abstract class ChunkElementConverter
{
	/// <summary>
	/// Format the current object, converting it into a string.
	/// </summary>
	/// <param name="value">The value to format.</param>
	/// <param name="formatProvider">The format provider object that can be used for formatting elements.</param>
	/// <returns>The equivalent string representation.</returns>
	public abstract string Format(object value, IFormatProvider? formatProvider);

	/// <summary>
	/// Performs JSON reading operation (deserialization) that will be called by <see cref="JsonConverter{T}.Read"/> method.
	/// </summary>
	/// <param name="reader"><inheritdoc cref="JsonConverter{T}.Read" path="/param[@name='reader']"/></param>
	/// <param name="formatProvider">The format provider (parser, culture information, etc.).</param>
	/// <param name="typeToConvert"><inheritdoc cref="JsonConverter{T}.Read" path="/param[@name='typeToConvert']"/></param>
	/// <param name="options"><inheritdoc cref="JsonConverter{T}.Read" path="/param[@name='options']"/></param>
	/// <returns>The value converted.</returns>
	public abstract object Read(ref Utf8JsonReader reader, IFormatProvider? formatProvider, Type typeToConvert, JsonSerializerOptions options);

	/// <summary>
	/// Performs JSON writting operation (serialization) that will be called by <see cref="JsonConverter{T}.Write"/> method.
	/// </summary>
	/// <param name="writer"><inheritdoc cref="JsonConverter{T}.Write" path="/param[@name='writer']"/></param>
	/// <param name="formatProvider">The format provider (converter, culture information, etc.).</param>
	/// <param name="value"><inheritdoc cref="JsonConverter{T}.Write" path="/param[@name='value']"/></param>
	/// <param name="options"><inheritdoc cref="JsonConverter{T}.Write" path="/param[@name='options']"/></param>
	/// <seealso cref="JsonConverter{T}.Write"/>
	public abstract void Write(Utf8JsonWriter writer, object value, IFormatProvider? formatProvider, JsonSerializerOptions options);
}
