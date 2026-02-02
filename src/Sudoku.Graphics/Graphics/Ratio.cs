namespace Sudoku.Graphics;

/// <summary>
/// Represents a ratio value.
/// </summary>
/// <param name="value">The value.</param>
[JsonConverter(typeof(Converter))]
public readonly struct Ratio(float value) :
	IComparable<Ratio>,
	IComparisonOperators<Ratio, Ratio, bool>,
	IEquatable<Ratio>,
	IEqualityOperators<Ratio, Ratio, bool>
{
	/// <summary>
	/// Indicates the value.
	/// </summary>
	public float Value { get; } = value;


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] object? obj) => obj is Ratio comparer && Equals(comparer);

	/// <inheritdoc/>
	public bool Equals(Ratio other) => Value.NearlyEquals(other.Value, float.Epsilon);

	/// <summary>
	/// Measure the fact value
	/// (i.e. performs the formula <c><paramref name="value"/> * <see langword="this"/>.RatioValue</c>).
	/// </summary>
	/// <param name="value">The original value.</param>
	/// <returns>The target value measured.</returns>
	public float Measure(float value) => value * Value;

	/// <inheritdoc/>
	public int CompareTo(Ratio other)
	{
		var left = (int)Math.Round(Value * 1E5);
		var right = (int)Math.Round(other.Value * 1E5);
		return left.CompareTo(right);
	}

	/// <inheritdoc/>
	public override int GetHashCode() => Value.GetHashCode();

	/// <inheritdoc cref="object.ToString"/>
	public override string ToString() => Value.ToString("P1");

	/// <inheritdoc cref="double.ToString(string?)"/>
	public string ToString(string? format) => Value.ToString(format);


	/// <inheritdoc/>
	public static bool operator ==(Ratio left, Ratio right) => left.Equals(right);

	/// <inheritdoc/>
	public static bool operator !=(Ratio left, Ratio right) => !(left == right);

	/// <inheritdoc/>
	public static bool operator >(Ratio left, Ratio right) => left.CompareTo(right) > 0;

	/// <inheritdoc/>
	public static bool operator >=(Ratio left, Ratio right) => left.CompareTo(right) >= 0;

	/// <inheritdoc/>
	public static bool operator <(Ratio left, Ratio right) => left.CompareTo(right) < 0;

	/// <inheritdoc/>
	public static bool operator <=(Ratio left, Ratio right) => left.CompareTo(right) <= 0;


	/// <summary>
	/// Implicit cast from <see cref="float"/> into <see cref="Ratio"/> value.
	/// </summary>
	/// <param name="value">The value.</param>
	public static implicit operator Ratio(float value) => new(value);


	/// <summary>
	/// Explicit cast from <see cref="Ratio"/> into <see cref="float"/> value.
	/// </summary>
	/// <param name="value">The value.</param>
	public static explicit operator float(Ratio value) => value.Value;
}

/// <summary>
/// Represents a JSON converter for <see cref="Ratio"/> instances.
/// </summary>
/// <seealso cref="Ratio"/>
file sealed class Converter : JsonConverter<Ratio>
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
