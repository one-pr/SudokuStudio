namespace Sudoku.Graphics.ExternalConverters;

/// <summary>
/// Represents <see cref="SKColor"/> JSON converter.
/// </summary>
/// <seealso cref="SKColor"/>
public sealed class SKColorConverter : JsonConverter<SKColor>
{
	/// <inheritdoc/>
	public override SKColor Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		reader.Read(); // StartArray
		reader.Read(); // Alpha
		var a = reader.GetByte();
		reader.Read(); // Red
		var r = reader.GetByte();
		reader.Read(); // Green
		var g = reader.GetByte();
		reader.Read(); // Blue
		var b = reader.GetByte();
		reader.Read(); // EndArray
		return new(a, r, g, b);
	}

	/// <inheritdoc/>
	public override void Write(Utf8JsonWriter writer, SKColor value, JsonSerializerOptions options)
	{
		writer.WriteStartArray();
		writer.WriteNumberValue(value.Alpha);
		writer.WriteNumberValue(value.Red);
		writer.WriteNumberValue(value.Green);
		writer.WriteNumberValue(value.Blue);
		writer.WriteEndArray();
	}
}
