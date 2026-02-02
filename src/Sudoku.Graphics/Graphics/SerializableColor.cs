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
	public override string ToString() => ToString(null);

	/// <summary>
	/// Converts the current instance into string representation, using the specified format string.
	/// Supported formats:
	/// <list type="table">
	/// <listheader>
	/// <term>Format string</term>
	/// <description>Meaning</description>
	/// </listheader>
	/// <item>
	/// <term><see langword="null"/></term>
	/// <description>Same as <c>"#RRGGBB"</c></description>
	/// </item>
	/// <item>
	/// <term><c>"#RRGGBB"</c> and <c>"#rrggbb"</c></term>
	/// <description>
	/// Output string as red-green-blue hex string format (<c>"#RRGGBB"</c> is upper-cased);
	/// if alpha value is 255, it will be omitted
	/// </description>
	/// </item>
	/// <item>
	/// <term><c>"#AARRGGBB"</c> and <c>"#aarrggbb"</c></term>
	/// <description>Output string as alpha-red-green-blue hex string format (<c>"#AARRGGBB"</c> is upper-cased)</description>
	/// </item>
	/// <item>
	/// <term><c>"#RGB"</c> and <c>"#rgb"</c></term>
	/// <description>
	/// Output string as red-green blue hex string format (<c>"#RGB"</c> is upper-cased);
	/// if two characters in all red, green and blue values are same like <c>#AABBCC</c>, it will be simplified as <c>#ABC</c>
	/// </description>
	/// </item>
	/// <item>
	/// <term><c>"#ARGB"</c> and <c>"#argb"</c></term>
	/// <description>
	/// Output string as alpha-red-green blue hex string format (<c>"#ARGB"</c> is upper-cased);
	/// if two characters in all red, green and blue values are same like <c>#AABBCCDD</c>, it will be simplified as <c>#ABCD</c>
	/// </description>
	/// </item>
	/// </list>
	/// </summary>
	/// <param name="format">The format string.</param>
	/// <returns>The string representation.</returns>
	public string ToString(string? format)
	{
		const string format_rgbFullUpper = "#RRGGBB";
		const string format_rgbFullLower = "#rrggbb";
		const string format_argbFullUpper = "#AARRGGBB";
		const string format_argbFullLower = "#aarrggbb";
		const string format_rgbSimplifiedUpper = "#RGB";
		const string format_rgbSimplifiedLower = "#rgb";
		const string format_argbSimplifiedUpper = "#ARGB";
		const string format_argbSimplifiedLower = "#argb";
		var aString = Alpha.ToString("X2");
		var rString = Red.ToString("X2");
		var gString = Green.ToString("X2");
		var bString = Blue.ToString("X2");
		var isUpperCasedFormat = format is null
			or format_argbFullUpper or format_rgbFullUpper
			or format_argbSimplifiedUpper or format_rgbSimplifiedUpper;
		switch (format)
		{
			case null:
			{
				// Check well-known colors, in order to get its friendly name.
				foreach (var (name, value) in WellKnownColors)
				{
					if (this == value)
					{
						return name;
					}
				}
				goto case format_rgbFullUpper;
			}
			case format_rgbFullUpper:
			case format_rgbFullLower:
			{
				var resultOriginal = Alpha == 255 ? $"#{rString}{gString}{bString}" : $"#{aString}{rString}{gString}{bString}";
				return isUpperCasedFormat ? resultOriginal : resultOriginal.ToLower();
			}
			case format_argbFullUpper:
			case format_argbFullLower:
			{
				var resultOriginal = $"#{aString}{rString}{gString}{bString}";
				return isUpperCasedFormat ? resultOriginal : resultOriginal.ToLower();
			}
			case format_rgbSimplifiedUpper:
			case format_rgbSimplifiedLower:
			{
				var resultOriginal = Alpha == 255
					? (rString[0], gString[0], bString[0]) == (rString[1], gString[1], bString[1])
						? $"#{rString[0]}{gString[0]}{bString[0]}"
						: $"#{rString}{gString}{bString}"
					: (aString[0], rString[0], gString[0], bString[0]) == (aString[1], rString[1], gString[1], bString[1])
						? $"#{aString[0]}{rString[0]}{gString[0]}{bString[0]}"
						: $"#{aString}{rString}{gString}{bString}";
				return isUpperCasedFormat ? resultOriginal : resultOriginal.ToLower();
			}
			case format_argbSimplifiedUpper:
			case format_argbSimplifiedLower:
			{
				var resultOriginal = (aString[0], rString[0], gString[0], bString[0]) == (aString[1], rString[1], gString[1], bString[1])
					? $"#{aString[0]}{rString[0]}{gString[0]}{bString[0]}"
					: $"#{aString}{rString}{gString}{bString}";
				return isUpperCasedFormat ? resultOriginal : resultOriginal.ToLower();
			}
			default:
			{
				throw new FormatException($"Invalid format string '{format}'.");
			}
		}
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
				'#' => s[1..] switch
				{
					[var r, var g, var b] => Parse($"#FF{r}{r}{g}{g}{b}{b}"),
					[var a, var r, var g, var b] => Parse($"#{a}{a}{r}{r}{g}{g}{b}{b}"),
					{ Length: 6 or 8 } t => (t / 2) switch
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
					_ => throw new FormatException("Expected length 3, 4, 6 or 8.")
				},
				_ => throw new FormatException("Expected color hex string prefix token '#'.")
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
