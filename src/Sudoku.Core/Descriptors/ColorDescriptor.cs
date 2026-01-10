namespace Sudoku.Descriptors;

/// <summary>
/// Represents a descriptor that describes for a color that will be used in drawing.
/// There're 3 possible kinds of colors:
/// <list type="number">
/// <item>
/// <b>Alias</b>: Define an enumeration field of type <see cref="ColorDescriptorAlias"/>
/// to describe an item should be colored as the specified kind of items predefined in the coloring system.
/// </item>
/// <item>
/// <b>ID</b>: Define an integer value indicating the desired ID defined in a palette (a global color pool)
/// predefined in the coloring system.
/// </item>
/// <item><b>ARGB</b>: Define a quadruple of bytes indicating alpha, red, green and blue values as an ARGB color.</item>
/// </list>
/// </summary>
/// <param name="mask">The 64-bit signed integer as a mask.</param>
/// <remarks>
/// The type uses 5 of 8 bytes, 34 of 64 bits.
/// </remarks>
/// <seealso cref="ColorDescriptorAlias"/>
[JsonConverter(typeof(Converter))]
public readonly struct ColorDescriptor(long mask) :
	IEquatable<ColorDescriptor>,
	IEqualityOperators<ColorDescriptor, ColorDescriptor, bool>,
	ITuple
{
	/// <summary>
	/// Indicates the shift bits amount.
	/// </summary>
	private const int TypeShift = 32;


	/// <summary>
	/// Initializes a <see cref="ColorDescriptor"/> instance via the specified an integer ID.
	/// </summary>
	/// <param name="id">The ID value.</param>
	private ColorDescriptor(int id) : this((long)ColorDescriptorType.Id << TypeShift | (long)id)
	{
	}

	/// <summary>
	/// Initializes a <see cref="ColorDescriptor"/> instance via ARGB values.
	/// </summary>
	/// <param name="a">The alpha value.</param>
	/// <param name="r">The red value.</param>
	/// <param name="g">The green value.</param>
	/// <param name="b">The blue value.</param>
	private ColorDescriptor(byte a, byte r, byte g, byte b) :
		this((long)ColorDescriptorType.Argb << TypeShift | (long)(a << 24 | r << 16 | g << 8 | b))
	{
	}

	/// <summary>
	/// Initializes a <see cref="ColorDescriptor"/> instance via well-known item.
	/// </summary>
	/// <param name="item">The well-known item.</param>
	private ColorDescriptor(ColorDescriptorAlias item) : this((long)ColorDescriptorType.Alias << TypeShift | (long)item)
	{
	}


	/// <summary>
	/// Indicates alpha value.
	/// The value becomes unsafe when <see cref="Type"/> is not <see cref="ColorDescriptorType.Argb"/> but no exceptions thrown.
	/// </summary>
	public byte Alpha => (byte)(ValueMask & byte.MaxValue);

	/// <summary>
	/// Indicates red value.
	/// The value becomes unsafe when <see cref="Type"/> is not <see cref="ColorDescriptorType.Argb"/> but no exceptions thrown.
	/// </summary>
	public byte Red => (byte)(ValueMask >> 8 & byte.MaxValue);

	/// <summary>
	/// Indicates green value.
	/// The value becomes unsafe when <see cref="Type"/> is not <see cref="ColorDescriptorType.Argb"/> but no exceptions thrown.
	/// </summary>
	public byte Green => (byte)(ValueMask >> 16 & byte.MaxValue);

	/// <summary>
	/// Indicates blue value.
	/// The value becomes unsafe when <see cref="Type"/> is not <see cref="ColorDescriptorType.Argb"/> but no exceptions thrown.
	/// </summary>
	public byte Blue => (byte)(ValueMask >> 24 & byte.MaxValue);

	/// <summary>
	/// Indicates an integer that represents ARGB values.
	/// The value becomes unsafe when <see cref="Type"/> is not <see cref="ColorDescriptorType.Argb"/> but no exceptions thrown.
	/// </summary>
	public int ArgbMask => (int)(Mask & int.MaxValue);

	/// <summary>
	/// Indicates an integer that describes the palette ID that a user has chosen.
	/// The value becomes unsafe when <see cref="Type"/> is not <see cref="ColorDescriptorType.Id"/> but no exceptions thrown.
	/// </summary>
	public int Id => (int)ValueMask;

	/// <summary>
	/// Indicates the mask that only represents color data.
	/// </summary>
	public long ValueMask => Mask & (1L << TypeShift) - 1;

	/// <summary>
	/// Indicates the type of the color identifier.
	/// </summary>
	public ColorDescriptorType Type => (ColorDescriptorType)(Mask >> TypeShift);

	/// <summary>
	/// Indicates an aliased value that directly points to an item that you want to color it to.
	/// The value becomes unsafe when <see cref="Type"/> is not <see cref="ColorDescriptorType.Alias"/> but no exceptions thrown.
	/// </summary>
	public ColorDescriptorAlias AliasedItem => (ColorDescriptorAlias)ValueMask;

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
	/// <item>A quadruple of ARGB values (if <see cref="Type"/> is <see cref="ColorDescriptorType.Argb"/>)</item>
	/// <item>An integer value (if <see cref="Type"/> is <see cref="ColorDescriptorType.Id"/>)</item>
	/// <item>
	/// An enumeration field of type <see cref="ColorDescriptorAlias"/>
	/// (if <see cref="Type"/> is <see cref="ColorDescriptorType.Alias"/>)
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
				ColorDescriptorType.Argb => (Alpha, Red, Green, Blue),
				ColorDescriptorType.Alias => AliasedItem,
				ColorDescriptorType.Id => Id
			},
			_ => throw new IndexOutOfRangeException(nameof(index))
		};


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] object? obj) => obj is ColorDescriptor comparer && Equals(comparer);

	/// <inheritdoc/>
	public bool Equals(ColorDescriptor other) => Mask == other.Mask;

	/// <inheritdoc cref="object.GetHashCode"/>
	public override int GetHashCode() => Mask.GetHashCode();

	/// <inheritdoc cref="object.ToString"/>
	public override string ToString()
		=> Type switch
		{
			ColorDescriptorType.Argb => (Alpha, Red, Green, Blue).ToString(),
			ColorDescriptorType.Id => Id.ToString(),
			ColorDescriptorType.Alias => AliasedItem.ToString()
		};


	/// <inheritdoc/>
	public static bool operator ==(ColorDescriptor left, ColorDescriptor right) => left.Equals(right);

	/// <inheritdoc/>
	public static bool operator !=(ColorDescriptor left, ColorDescriptor right) => !(left == right);


	/// <summary>
	/// Implicit cast from <see cref="int"/> to <see cref="ColorDescriptor"/>.
	/// </summary>
	/// <param name="id">The ID.</param>
	public static implicit operator ColorDescriptor(int id) => new(id);

	/// <summary>
	/// Implicit cast from (<see cref="byte"/>, <see cref="byte"/>, <see cref="byte"/>, <see cref="byte"/>)
	/// to <see cref="ColorDescriptor"/>.
	/// </summary>
	/// <param name="tuple">The tuple or ARGB values.</param>
	public static implicit operator ColorDescriptor((byte Alpha, byte Red, byte Green, byte Blue) tuple)
		=> new(tuple.Alpha, tuple.Red, tuple.Green, tuple.Blue);

	/// <summary>
	/// Implicit cast from <see cref="ColorDescriptorAlias"/> to <see cref="ColorDescriptor"/>.
	/// </summary>
	/// <param name="item">The aliased item.</param>
	public static implicit operator ColorDescriptor(ColorDescriptorAlias item) => new(item);

	/// <summary>
	/// Explicit cast from <see cref="ColorDescriptor"/> into <see cref="int"/> ID.
	/// </summary>
	/// <param name="descriptor">The descriptor.</param>
	public static explicit operator int(ColorDescriptor descriptor)
		=> descriptor.Type == ColorDescriptorType.Id ? descriptor.Id : throw new InvalidCastException();

	/// <summary>
	/// Explicit cast from <see cref="ColorDescriptor"/> into ARGB quadruple.
	/// </summary>
	/// <param name="descriptor">The descriptor.</param>
	public static explicit operator (byte Alpha, byte Red, byte Green, byte Blue)(ColorDescriptor descriptor)
		=> descriptor.Type == ColorDescriptorType.Argb
			? (descriptor.Alpha, descriptor.Red, descriptor.Green, descriptor.Blue)
			: throw new InvalidCastException();

	/// <summary>
	/// Explicit cast from <see cref="ColorDescriptor"/> into <see cref="ColorDescriptorAlias"/> field.
	/// </summary>
	/// <param name="descriptor">The descriptor.</param>
	public static explicit operator ColorDescriptorAlias(ColorDescriptor descriptor)
		=> descriptor.Type == ColorDescriptorType.Alias ? descriptor.AliasedItem : throw new InvalidCastException();
}

/// <summary>
/// Represents a JSON converter that can serialize and deserialize with instances of this type.
/// </summary>
file sealed class Converter : JsonConverter<ColorDescriptor>
{
	/// <summary>
	/// Indicates whether we ignore case parsing on enumeration fields.
	/// </summary>
	public bool IgnoreCase { get; init; } = true;


	/// <inheritdoc/>
	public override ColorDescriptor Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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
				if (ColorDescriptorAlias.TryParse(s, IgnoreCase, out var e))
				{
					return e;
				}
				if (int.TryParse(s, out var numeric))
				{
					return (ColorDescriptorAlias)numeric;
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
	public override void Write(Utf8JsonWriter writer, ColorDescriptor value, JsonSerializerOptions options)
	{
		switch (value.Type)
		{
			case ColorDescriptorType.Argb:
			{
				writer.WriteStartArray();
				writer.WriteNumberValue(value.Alpha);
				writer.WriteNumberValue(value.Red);
				writer.WriteNumberValue(value.Green);
				writer.WriteNumberValue(value.Blue);
				writer.WriteEndArray();
				break;
			}

			case ColorDescriptorType.Id:
			{
				writer.WriteNumberValue(value.Id);
				break;
			}

			case ColorDescriptorType.Alias:
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
