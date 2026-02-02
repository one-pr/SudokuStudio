namespace Sudoku.Graphics;

/// <summary>
/// Represents a sequence of dash intervals.
/// </summary>
[JsonConverter(typeof(Converter))]
public readonly struct LineDashSequence : IEnumerable<float>
{
	/// <summary>
	/// The backing intervals.
	/// </summary>
	private readonly List<float> _intervals;


	/// <summary>
	/// Initializes a <see cref="LineDashSequence"/>.
	/// </summary>
	public LineDashSequence() => _intervals = [];

	/// <summary>
	/// Initializes a <see cref="LineDashSequence"/> via the specified intervals.
	/// </summary>
	/// <param name="intervals">The intervals.</param>
	private LineDashSequence(params ReadOnlySpan<float> intervals) : this() => _intervals.AddRange(intervals);


	/// <summary>
	/// Indicates whether the sequence is empty.
	/// </summary>
	public bool IsEmpty => _intervals.Count == 0;

	/// <summary>
	/// Indicates interval values.
	/// </summary>
	public ReadOnlySpan<float> Intervals => _intervals.AsSpan();


	/// <summary>
	/// Returns the element at the specified index.
	/// </summary>
	/// <param name="index">The desired index.</param>
	/// <returns>The value.</returns>
	public float this[int index] => _intervals[index];


	/// <summary>
	/// Adds a new element into the collection.
	/// </summary>
	/// <param name="value">The value.</param>
	public void Add(float value) => _intervals.Add(value);

	/// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
	public AnonymousSpanEnumerator<float> GetEnumerator() => new(Intervals);


	/// <summary>
	/// Creates a <see cref="LineDashSequence"/> instance via intervals.
	/// </summary>
	/// <param name="values">The intervals.</param>
	/// <returns>The instance.</returns>
	public static LineDashSequence Create(params ReadOnlySpan<float> values) => new(values);

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator() => _intervals.GetEnumerator();

	/// <inheritdoc/>
	IEnumerator<float> IEnumerable<float>.GetEnumerator() => _intervals.AsEnumerable().GetEnumerator();


	/// <summary>
	/// Implicit cast from <see cref="LineDashSequence"/> into <see cref="SKPathEffect"/>.
	/// </summary>
	/// <param name="sequence">The sequence.</param>
	public static implicit operator SKPathEffect(LineDashSequence sequence)
		=> SKPathEffect.CreateDash([.. sequence._intervals], 0);
}

/// <summary>
/// Represents <see cref="LineDashSequence"/> JSON converter.
/// </summary>
/// <seealso cref="LineDashSequence"/>
file sealed class Converter : JsonConverter<LineDashSequence>
{
	/// <inheritdoc/>
	public override LineDashSequence Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var sequence = new List<float>();
		while (reader.Read())
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.StartArray:
				{
					break;
				}
				case JsonTokenType.EndArray:
				{
					return [.. sequence];
				}
				case JsonTokenType.Number:
				{
					sequence.Add(reader.GetSingle());
					break;
				}
				default:
				{
					throw new JsonException();
				}
			}
		}
		throw new UnreachableException();
	}

	/// <inheritdoc/>
	public override void Write(Utf8JsonWriter writer, LineDashSequence value, JsonSerializerOptions options)
	{
		writer.WriteStartArray();
		foreach (var element in value.Intervals)
		{
			writer.WriteNumberValue(element);
		}
		writer.WriteEndArray();
	}
}
