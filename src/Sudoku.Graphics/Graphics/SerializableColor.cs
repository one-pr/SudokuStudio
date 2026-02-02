namespace Sudoku.Graphics;

/// <summary>
/// Represents a serializable color value.
/// </summary>
[JsonConverter(typeof(Converter))]
public readonly struct SerializableColor :
	IEquatable<SerializableColor>,
	IEqualityOperators<SerializableColor, SerializableColor, bool>
{
	/// <summary>
	/// Represents all well-known colors.
	/// </summary>
	public static readonly FrozenDictionary<string, SerializableColor> WellKnownColors;


	/// <summary>
	/// Indicates the mask.
	/// </summary>
	private readonly uint _value;


	/// <summary>
	/// Initializes a <see cref="SerializableColor"/> instance via the specified alpha, red, green and blue values.
	/// </summary>
	/// <param name="alpha">The alpha value.</param>
	/// <param name="red">The red value.</param>
	/// <param name="green">The green value.</param>
	/// <param name="blue">The blue value.</param>
	public SerializableColor(byte alpha, byte red, byte green, byte blue)
	{
		Alpha = alpha;
		Red = red;
		Green = green;
		Blue = blue;
	}

	/// <summary>
	/// Initializes a <see cref="SerializableColor"/> instance via the specified red, green and blue values;
	/// with alpha value 255 as default initialization.
	/// </summary>
	/// <param name="red">The red value.</param>
	/// <param name="green">The green value.</param>
	/// <param name="blue">The blue value.</param>
	public SerializableColor(byte red, byte green, byte blue) : this(255, red, green, blue)
	{
	}

	/// <summary>
	/// Initializes a <see cref="SerializableColor"/> instance via the specified <see cref="SKColor"/> instance.
	/// </summary>
	/// <param name="skColor">The <see cref="SKColor"/> instance.</param>
	public SerializableColor(SKColor skColor) : this(skColor.Alpha, skColor.Red, skColor.Green, skColor.Blue)
	{
	}

	/// <summary>
	/// Initializes a <see cref="SerializableColor"/> instance via the specified mask value.
	/// </summary>
	/// <param name="value">The mask value.</param>
	private SerializableColor(uint value) => _value = value;


	/// <include file='../../global-doc-comments.xml' path='g/static-constructor' />
	static SerializableColor()
	{
		var dictionary = new Dictionary<string, SerializableColor>();
		foreach (var fieldInfo in typeof(SKColors).GetFields(BindingFlags.Public | BindingFlags.Static))
		{
			var value = new SerializableColor((SKColor)fieldInfo.GetValue(null)!);
			dictionary.Add(fieldInfo.Name, value);
		}
		WellKnownColors = dictionary.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
	}


	/// <summary>
	/// Indicates alpha value.
	/// </summary>
	public byte Alpha
	{
		get => (byte)(_value >> 24 & 255);

		private init => _value |= (uint)(value << 24);
	}

	/// <summary>
	/// Indicates red value.
	/// </summary>
	public byte Red
	{
		get => (byte)(_value >> 16 & 255);

		private init => _value |= (uint)(value << 16);
	}

	/// <summary>
	/// Indicates green value.
	/// </summary>
	public byte Green
	{
		get => (byte)(_value >> 8 & 255);

		private init => _value |= (uint)(value << 8);
	}

	/// <summary>
	/// Indicates blue value.
	/// </summary>
	public byte Blue
	{
		get => (byte)(_value & 255);

		private init => _value |= value;
	}


	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
	public void Deconstruct(out byte alpha, out byte red, out byte green, out byte blue)
		=> (alpha, red, green, blue) = (Alpha, Red, Green, Blue);

	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
	public void Deconstruct(out byte red, out byte green, out byte blue) => (red, green, blue) = (Red, Green, Blue);

	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] object? obj)
		=> obj switch
		{
			SerializableColor { _value: var v } => v == _value,
			SKColor(var a, var r, var g, var b) => (Alpha, Red, Green, Blue) == (a, r, g, b),
			_ => false
		};

	/// <inheritdoc/>
	public bool Equals(SerializableColor other) => _value == other._value;

	/// <inheritdoc cref="object.GetHashCode"/>
	public override int GetHashCode() => (int)_value;

	/// <inheritdoc cref="object.ToString"/>
	public override string ToString()
	{
		// Check well-known colors, in order to get its friendly name.
		foreach (var (name, value) in WellKnownColors)
		{
			if (this == value)
			{
				return name;
			}
		}

		// Otherwise, return hex string. If alpha is 255, we'll omit it.
		var aString = Alpha.ToString("X2");
		var rString = Red.ToString("X2");
		var gString = Green.ToString("X2");
		var bString = Blue.ToString("X2");
		return Alpha == 255 ? $"#{rString}{gString}{bString}" : $"#{aString}{rString}{gString}{bString}";
	}

	/// <summary>
	/// Converts the current instance as an <see cref="SKColor"/> instance.
	/// </summary>
	/// <returns>An <see cref="SKColor"/> instance.</returns>
	public SKColor AsSKColor() => new(Red, Green, Blue, Alpha);


	/// <inheritdoc cref="IParsable{TSelf}.TryParse(string?, IFormatProvider?, out TSelf)"/>
	public static bool TryParse([NotNullWhen(true)] string? s, out SerializableColor result)
	{
		if (s is null)
		{
			goto ReturnFalse;
		}

		try
		{
			result = Parse(s);
			return true;
		}
		catch (FormatException)
		{
		}

	ReturnFalse:
		result = default;
		return false;
	}

	/// <inheritdoc cref="IParsable{TSelf}.Parse(string, IFormatProvider?)"/>
	public static SerializableColor Parse(string s)
		=> WellKnownColors.TryGetValue(s, out var wellKnownColorResult)
			? wellKnownColorResult
			: s[0] switch
			{
				'#' => (s[1..] / 2) switch
				{
					[var aString, var rString, var gString, var bString]
						when Convert.ToByte(aString, 16) is var alpha
						&& Convert.ToByte(rString, 16) is var red
						&& Convert.ToByte(gString, 16) is var green
						&& Convert.ToByte(bString, 16) is var blue
						=> new(alpha, red, green, blue),
					[var rString, var gString, var bString]
						when Convert.ToByte(rString, 16) is var red
						&& Convert.ToByte(gString, 16) is var green
						&& Convert.ToByte(bString, 16) is var blue
						=> new(red, green, blue),
				},
				_ => throw new FormatException("Color hex string prefix token '#' expected.")
			};


	/// <inheritdoc/>
	public static bool operator ==(SerializableColor left, SerializableColor right) => left.Equals(right);

	/// <inheritdoc/>
	public static bool operator !=(SerializableColor left, SerializableColor right) => !(left == right);
}

/// <summary>
/// Represents a JSON converter of type <see cref="SerializableColor"/>.
/// </summary>
/// <seealso cref="SerializableColor"/>
file sealed class Converter : JsonConverter<SerializableColor>
{
	/// <inheritdoc/>
	public override SerializableColor Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		=> SerializableColor.Parse(reader.GetString() ?? string.Empty);

	/// <inheritdoc/>
	public override void Write(Utf8JsonWriter writer, SerializableColor value, JsonSerializerOptions options)
		=> writer.WriteStringValue(value.ToString());
}
