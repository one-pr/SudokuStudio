namespace Sudoku.Descriptors;

/// <summary>
/// Represents a single item or a list of items to draw.
/// </summary>
public abstract class Chunk : IEnumerable, IFormattable
{
	/// <summary>
	/// Indicates JSON property name for <see cref="Type"/>.
	/// </summary>
	private const string TypeJsonPropertyName = "$type";

	/// <summary>
	/// Indicates JSON property name for <see cref="Element"/>.
	/// </summary>
	private const string ElementJsonPropertyName = "$element";

	/// <summary>
	/// Indicates JSON property name for value.
	/// </summary>
	private const string ValueJsonPropertyName = "value";


	/// <summary>
	/// Indicates the type of chunk (cell, candidate, house, etc.).
	/// </summary>
	public ChunkType Type { get; protected internal set; }

	/// <summary>
	/// Indicates the type of elements (a single item, multiple items, etc.).
	/// </summary>
	public ChunkElement Element { get; protected internal set; }


	/// <inheritdoc/>
	public sealed override string ToString() => ToString(null);

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	public string ToString(IFormatProvider? formatProvider)
	{
		var value = GetTargetField(out var attribute).GetValue(this)!;
		var converter = (ChunkElementConverter)Activator.CreateInstance(attribute.ConverterType)!;
		return converter.Format(value, formatProvider);
	}

	/// <inheritdoc/>
	public IEnumerator GetEnumerator()
	{
		var value = GetTargetField(out _).GetValue(this)!;
		if (value is not IEnumerable enumerable)
		{
			// Single element.
			yield return value;
			yield break;
		}

		// Collection element.
		foreach (var element in enumerable)
		{
			yield return element;
		}
		yield break;
	}

	/// <summary>
	/// The core operation for reading JSON. The result instance deserialized will be assigned to the current instance.
	/// </summary>
	/// <param name="reader">The reader instance.</param>
	/// <param name="formatProvider">The format provider.</param>
	/// <param name="typeToConvert">The type to convert.</param>
	/// <param name="options">Options.</param>
	protected internal void DeserializeCore(
		ref Utf8JsonReader reader,
		IFormatProvider? formatProvider,
		Type typeToConvert,
		JsonSerializerOptions options
	)
	{
		var propertyName = default(string);
		var chunkElement = default(ChunkElement?);
		while (reader.Read())
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.StartObject:
				{
					// Start. Do nothing.
					break;
				}
				case JsonTokenType.EndObject:
				{
					Element = chunkElement ?? throw new JsonException();
					return;
				}
				case JsonTokenType.PropertyName:
				{
					propertyName = reader.GetString();
					break;
				}
				case JsonTokenType.String:
				{
					var valueString = reader.GetString()!;
					switch (propertyName)
					{
						case TypeJsonPropertyName when ChunkType.TryParse(valueString, out var t):
						{
							Type = t;
							break;
						}
						case ElementJsonPropertyName when ChunkElement.TryParse(valueString, out var e):
						{
							chunkElement = e;
							break;
						}
						case ValueJsonPropertyName when chunkElement is not null:
						{
							var fieldInfo = GetTargetField(out var attribute);
							var converter = (ChunkElementConverter)Activator.CreateInstance(attribute.ConverterType)!;
							var value = converter.Read(ref reader, formatProvider, typeToConvert, options);
							fieldInfo.SetValue(this, value);
							break;
						}
						default:
						{
							throw new JsonException("Invalid parsed string.");
						}
					}
					break;
				}
			}
		}
	}

	/// <summary>
	/// The core operation for writting JSON.
	/// </summary>
	/// <param name="writer">The writer instance.</param>
	/// <param name="formatProvider">The format provider.</param>
	/// <param name="options">Options.</param>
	protected internal void SerializeCore(
		Utf8JsonWriter writer,
		IFormatProvider? formatProvider,
		JsonSerializerOptions options
	)
	{
		writer.WriteStartObject();
		writer.WritePropertyName(TypeJsonPropertyName);
		writer.WriteStringValue(Type.ToString());
		writer.WritePropertyName(ElementJsonPropertyName);
		writer.WriteStringValue(Element.ToString());
		writer.WritePropertyName(ValueJsonPropertyName);

		var value = GetTargetField(out var attribute).GetValue(this)!;
		var converter = (ChunkElementConverter)Activator.CreateInstance(attribute.ConverterType)!;
		converter.Write(writer, value, formatProvider, options);
		writer.WriteEndObject();
	}

	/// <summary>
	/// Returns target field in reflection information with attribute.
	/// </summary>
	/// <param name="attribute">The attribute.</param>
	/// <returns>The field reflection information.</returns>
	protected FieldInfo GetTargetField(out ChunkElementAttribute attribute)
	{
		foreach (var fieldInfo in GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
		{
			if (fieldInfo.GetCustomAttribute<ChunkElementAttribute>() is not { Element: var targetElement } a)
			{
				continue;
			}

			if (targetElement != Element)
			{
				continue;
			}

			attribute = a;
			return fieldInfo;
		}

		throw new InvalidOperationException("Missing required metadata for this type.");
	}

	/// <inheritdoc/>
	string IFormattable.ToString(string? format, IFormatProvider? formatProvider) => ToString(null);
}
