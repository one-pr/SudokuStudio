namespace Sudoku.Graphics;

public sealed partial class CanvasDrawingOptions
{
	/// <summary>
	/// Represents a JSON converter for <see cref="Ratio"/> instances.
	/// </summary>
	/// <seealso cref="Ratio"/>
	private sealed class RatioConverter : JsonConverter<Ratio>
	{
		/// <inheritdoc/>
		public override Ratio Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			reader.Read();
			return reader.GetSingle();
		}

		/// <inheritdoc/>
		public override void Write(Utf8JsonWriter writer, Ratio value, JsonSerializerOptions options)
			=> writer.WriteNumberValue(value.Value);
	}
}
