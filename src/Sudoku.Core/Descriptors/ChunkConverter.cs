namespace Sudoku.Descriptors;

/// <summary>
/// Provides a JSON converter of <see cref="Chunk"/>.
/// </summary>
/// <typeparam name="TChunk">The type of chunk.</typeparam>
/// <seealso cref="Chunk"/>
public sealed class ChunkConverter<TChunk> : JsonConverter<TChunk> where TChunk : Chunk, new()
{
	/// <inheritdoc/>
	public override TChunk Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var @default = new TChunk();
		@default.DeserializeCore(ref reader, null, typeToConvert, options);
		return @default;
	}

	/// <inheritdoc/>
	public override void Write(Utf8JsonWriter writer, TChunk value, JsonSerializerOptions options)
		=> value.SerializeCore(writer, null, options);
}
