namespace Sudoku.Descriptors;

/// <summary>
/// Represents a color identifier that defines a color that will be used in drawing.
/// </summary>
/// <param name="mask">The 64-bit signed integer as a mask.</param>
/// <remarks>
/// The type uses 5 of 8 bytes to store values.
/// </remarks>
[JsonConverter(typeof(Converter))]
public readonly struct ColorIdentifier(long mask) :
	IEquatable<ColorIdentifier>,
	IEqualityOperators<ColorIdentifier, ColorIdentifier, bool>,
	ITuple
{
	/// <summary>
	/// Indicates the shift bits amount.
	/// </summary>
	private const int TypeShift = 32;


	/// <summary>
	/// Initializes a <see cref="ColorIdentifier"/> instance via the specified an integer ID.
	/// </summary>
	/// <param name="id">The ID value.</param>
	private ColorIdentifier(int id) : this((long)ColorIdentifierType.Id << TypeShift | (long)id)
	{
	}

	/// <summary>
	/// Initializes a <see cref="ColorIdentifier"/> instance via ARGB values.
	/// </summary>
	/// <param name="a">The alpha value.</param>
	/// <param name="r">The red value.</param>
	/// <param name="g">The green value.</param>
	/// <param name="b">The blue value.</param>
	private ColorIdentifier(byte a, byte r, byte g, byte b) :
		this((long)ColorIdentifierType.Argb << TypeShift | (long)(a << 24 | r << 16 | g << 8 | b))
	{
	}

	/// <summary>
	/// Initializes a <see cref="ColorIdentifier"/> instance via well-known item.
	/// </summary>
	/// <param name="item">The well-known item.</param>
	private ColorIdentifier(ColorIdentifierAlias item) : this((long)ColorIdentifierType.Alias << TypeShift | (long)item)
	{
	}


	/// <summary>
	/// Indicates alpha value.
	/// The value becomes unsafe when <see cref="Type"/> is not <see cref="ColorIdentifierType.Argb"/> but no exceptions thrown.
	/// </summary>
	public byte Alpha => (byte)(ValueMask & byte.MaxValue);

	/// <summary>
	/// Indicates red value.
	/// The value becomes unsafe when <see cref="Type"/> is not <see cref="ColorIdentifierType.Argb"/> but no exceptions thrown.
	/// </summary>
	public byte Red => (byte)(ValueMask >> 8 & byte.MaxValue);

	/// <summary>
	/// Indicates green value.
	/// The value becomes unsafe when <see cref="Type"/> is not <see cref="ColorIdentifierType.Argb"/> but no exceptions thrown.
	/// </summary>
	public byte Green => (byte)(ValueMask >> 16 & byte.MaxValue);

	/// <summary>
	/// Indicates blue value.
	/// The value becomes unsafe when <see cref="Type"/> is not <see cref="ColorIdentifierType.Argb"/> but no exceptions thrown.
	/// </summary>
	public byte Blue => (byte)(ValueMask >> 24 & byte.MaxValue);

	/// <summary>
	/// Indicates an integer that represents ARGB values.
	/// The value becomes unsafe when <see cref="Type"/> is not <see cref="ColorIdentifierType.Argb"/> but no exceptions thrown.
	/// </summary>
	public int ArgbMask => (int)(Mask & int.MaxValue);

	/// <summary>
	/// Indicates an integer that describes the palette ID that a user has chosen.
	/// The value becomes unsafe when <see cref="Type"/> is not <see cref="ColorIdentifierType.Id"/> but no exceptions thrown.
	/// </summary>
	public int Id => (int)ValueMask;

	/// <summary>
	/// Indicates the mask that only represents color data.
	/// </summary>
	public long ValueMask => Mask & (1L << TypeShift) - 1;

	/// <summary>
	/// Indicates the type of the color identifier.
	/// </summary>
	public ColorIdentifierType Type => (ColorIdentifierType)(Mask >> TypeShift);

	/// <summary>
	/// Indicates an aliased value that directly points to an item that you want to color it to.
	/// The value becomes unsafe when <see cref="Type"/> is not <see cref="ColorIdentifierType.Alias"/> but no exceptions thrown.
	/// </summary>
	public ColorIdentifierAlias AliasedItem => (ColorIdentifierAlias)ValueMask;

	/// <summary>
	/// Indicates the whole 64-bit mask.
	/// </summary>
	public long Mask { get; } = mask;

	/// <inheritdoc/>
	int ITuple.Length => 2;


	/// <summary>
	/// Returns a 2-element array.
	/// The first element represents the type and the second element represents the data of color identifier.
	/// The second value can be:
	/// <list type="bullet">
	/// <item>A quadruple of ARGB values (if <see cref="Type"/> is <see cref="ColorIdentifierType.Argb"/>)</item>
	/// <item>An integer value (if <see cref="Type"/> is <see cref="ColorIdentifierType.Id"/>)</item>
	/// <item>
	/// An enumeration field of type <see cref="ColorIdentifierAlias"/>
	/// (if <see cref="Type"/> is <see cref="ColorIdentifierType.Alias"/>)
	/// </item>
	/// </list>
	/// </summary>
	/// <param name="index">The desired index (0 or 1).</param>
	/// <returns>The value of element at the specified index of that 2-element array.</returns>
	/// <exception cref="IndexOutOfRangeException">Throws when <paramref name="index"/> is neither 0 nor 1.</exception>
	/// <remarks>
	/// This member allows you using patterns to check values like <c>identifier is (ColorIdentifierType.Argb, (255, 0, _, _))</c>.
	/// </remarks>
	object? ITuple.this[int index]
		=> index switch
		{
			0 => Type,
			1 => Type switch
			{
				ColorIdentifierType.Argb => (Alpha, Red, Green, Blue),
				ColorIdentifierType.Alias => AliasedItem,
				ColorIdentifierType.Id => Id
			},
			_ => throw new IndexOutOfRangeException(nameof(index))
		};


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] object? obj) => obj is ColorIdentifier comparer && Equals(comparer);

	/// <inheritdoc/>
	public bool Equals(ColorIdentifier other) => Mask == other.Mask;

	/// <inheritdoc cref="object.GetHashCode"/>
	public override int GetHashCode() => Mask.GetHashCode();

	/// <inheritdoc cref="object.ToString"/>
	public override string ToString()
		=> Type switch
		{
			ColorIdentifierType.Argb => (Alpha, Red, Green, Blue).ToString(),
			ColorIdentifierType.Id => Id.ToString(),
			ColorIdentifierType.Alias => AliasedItem.ToString()
		};


	/// <inheritdoc/>
	public static bool operator ==(ColorIdentifier left, ColorIdentifier right) => left.Equals(right);

	/// <inheritdoc/>
	public static bool operator !=(ColorIdentifier left, ColorIdentifier right) => !(left == right);


	/// <summary>
	/// Implicit cast from <see cref="int"/> to <see cref="ColorIdentifier"/>.
	/// </summary>
	/// <param name="id">The ID.</param>
	public static implicit operator ColorIdentifier(int id) => new(id);

	/// <summary>
	/// Implicit cast from (<see cref="byte"/>, <see cref="byte"/>, <see cref="byte"/>, <see cref="byte"/>)
	/// to <see cref="ColorIdentifier"/>.
	/// </summary>
	/// <param name="tuple">The tuple or ARGB values.</param>
	public static implicit operator ColorIdentifier((byte Alpha, byte Red, byte Green, byte Blue) tuple)
		=> new(tuple.Alpha, tuple.Red, tuple.Green, tuple.Blue);

	/// <summary>
	/// Implicit cast from <see cref="ColorIdentifierAlias"/> to <see cref="ColorIdentifier"/>.
	/// </summary>
	/// <param name="item">The aliased item.</param>
	public static implicit operator ColorIdentifier(ColorIdentifierAlias item) => new(item);
}

/// <summary>
/// Represents a JSON converter that can serialize and deserialize with instances of this type.
/// </summary>
file sealed class Converter : JsonConverter<ColorIdentifier>
{
	/// <summary>
	/// Indicates whether we ignore case parsing on enumeration fields.
	/// </summary>
	public bool IgnoreCase { get; init; } = true;


	/// <inheritdoc/>
	public override ColorIdentifier Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		switch (reader.TokenType)
		{
			case JsonTokenType.StartArray:
			{
				var argb = (stackalloc byte[4]);
				var index = 0;
				reader.Read();
				while (reader.TokenType != JsonTokenType.EndArray)
				{
					if (index >= 4)
					{
						throw new JsonException("Expected array of length 4 for bytes variant.");
					}

					if (reader.TokenType != JsonTokenType.Number || !reader.TryGetByte(out var value))
					{
						throw new JsonException("Bytes array must contain numbers 0..255.");
					}

					argb[index++] = value;
					reader.Read();
				}
				if (index != 4)
				{
					throw new JsonException("Expected exactly 4 bytes.");
				}

				return (argb[0], argb[1], argb[2], argb[3]);
			}

			case JsonTokenType.Number:
			{
				if (!reader.TryGetInt32(out var id))
				{
					throw new JsonException("Invalid integer value.");
				}
				return id;
			}

			case JsonTokenType.String:
			{
				var s = reader.GetString();
				if (ColorIdentifierAlias.TryParse(s, IgnoreCase, out var e))
				{
					return e;
				}
				if (int.TryParse(s, out var numeric))
				{
					return (ColorIdentifierAlias)numeric;
				}
				throw new JsonException($"Invalid enum value: '{s}'.");
			}

			default:
			{
				throw new JsonException($"Unexpected JSON token for this type: {reader.TokenType}.");
			}
		}
	}

	/// <inheritdoc/>
	public override void Write(Utf8JsonWriter writer, ColorIdentifier value, JsonSerializerOptions options)
	{
		switch (value.Type)
		{
			case ColorIdentifierType.Argb:
			{
				writer.WriteStartArray();
				writer.WriteNumberValue(value.Alpha);
				writer.WriteNumberValue(value.Red);
				writer.WriteNumberValue(value.Green);
				writer.WriteNumberValue(value.Blue);
				writer.WriteEndArray();
				break;
			}

			case ColorIdentifierType.Id:
			{
				writer.WriteNumberValue(value.Id);
				break;
			}

			case ColorIdentifierType.Alias:
			{
				writer.WriteStringValue(value.AliasedItem.ToString());
				break;
			}

			default:
			{
				throw new JsonException("Invalid format.");
			}
		}
	}
}
