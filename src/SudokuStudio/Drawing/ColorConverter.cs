namespace SudokuStudio.Drawing;

/// <summary>
/// Represents a converter that serialize and deserialize with a <see cref="Color"/> instance.
/// </summary>
/// <seealso cref="Color"/>
public sealed class ColorConverter : JsonConverter<Color>
{
	/// <inheritdoc/>
	public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.String)
		{
			var s = reader.GetString();
			if (string.IsNullOrWhiteSpace(s))
			{
				throw new JsonException("Empty color string.");
			}

			var parts = s / ',';
			if (parts is not [var aPart, var rPart, var gPart, var bPart])
			{
				throw new JsonException($"Color string must contain 4 comma-separated parts: {nameof(Color.A)},{nameof(Color.R)},{nameof(Color.G)},{nameof(Color.B)}.");
			}

			try
			{
				var a = ParseByte(aPart.Trim());
				var r = ParseByte(rPart.Trim());
				var g = ParseByte(gPart.Trim());
				var b = ParseByte(bPart.Trim());
				return Color.FromArgb(a, r, g, b);
			}
			catch (FormatException fe)
			{
				throw new JsonException("Invalid numeric value in color string.", fe);
			}
			catch (OverflowException oe)
			{
				throw new JsonException("Numeric value out of byte range in color string.", oe);
			}
		}
		else if (reader.TokenType == JsonTokenType.StartObject)
		{
			// Default values if missing
			var (a, r, g, b) = ((int?)null, (int?)null, (int?)null, (int?)null);
			while (reader.Read())
			{
				if (reader.TokenType == JsonTokenType.EndObject)
				{
					break;
				}

				if (reader.TokenType != JsonTokenType.PropertyName)
				{
					continue;
				}

				var propName = reader.GetString();
				if (!reader.Read())
				{
					throw new JsonException($"Unexpected end when reading {nameof(Color)} object.");
				}

				// Accept either numeric tokens or strings that contain numbers
				int value;
				switch (propName)
				{
					case "A" or "a" or "Alpha" or "alpha":
					{
						value = ReadIntValue(ref reader);
						a = value;
						break;
					}
					case "R" or "r" or "Red" or "red":
					{
						value = ReadIntValue(ref reader);
						r = value;
						break;
					}
					case "G" or "g" or "Green" or "green":
					{
						value = ReadIntValue(ref reader);
						g = value;
						break;
					}
					case "B" or "b" or "Blue" or "blue":
					{
						value = ReadIntValue(ref reader);
						b = value;
						break;
					}
					default:
					{
						// Unknown property: skip its value
						switch (options.UnmappedMemberHandling)
						{
							case JsonUnmappedMemberHandling.Skip:
							{
								reader.Skip();
								break;
							}
							case JsonUnmappedMemberHandling.Disallow:
							{
								throw new JsonException($"Property '{propName}' is invalid.");
							}
							default:
							{
								throw new JsonException($"Unexpected configured value '{options.UnmappedMemberHandling}' of type '{nameof(JsonUnmappedMemberHandling)}'.");
							}
						}
						break;
					}
				}
			}

			// If alpha is missing, assume 255 (opaque).
			var ai = a ?? byte.MaxValue;
			if (!r.HasValue || !g.HasValue || !b.HasValue)
			{
				throw new JsonException($"Color object must contain {nameof(Color.R)}, {nameof(Color.G)} and {nameof(Color.B)} properties ({nameof(Color.A)} optional).");
			}

			// Validate range 0..255
			ValidateByteRange(ai, nameof(a));
			ValidateByteRange(r.Value, nameof(r));
			ValidateByteRange(g.Value, nameof(g));
			ValidateByteRange(b.Value, nameof(b));

			return Color.FromArgb((byte)ai, (byte)r.Value, (byte)g.Value, (byte)b.Value);
		}
		else if (reader.TokenType == JsonTokenType.Null)
		{
			// Color is a non-nullable struct. Throw to indicate mismatch.
			throw new JsonException("Null is not a valid value for Color.");
		}

		throw new JsonException($"Unexpected token parsing Color. Token: {reader.TokenType}");
	}

	/// <inheritdoc/>
	public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
		=> writer.WriteStringValue($"{value.A},{value.R},{value.G},{value.B}");


	private static byte ParseByte(string s)
	{
		if (!int.TryParse(s, out var iv))
		{
			throw new FormatException("Invalid integer format.");
		}
		ValidateByteRange(iv, "value");
		return (byte)iv;
	}

	private static int ReadIntValue(ref Utf8JsonReader reader)
		=> reader.TokenType switch
		{
			JsonTokenType.Number => reader.GetInt32(),
			JsonTokenType.String => int.TryParse(reader.GetString(), out var iv)
				? iv
				: throw new JsonException("Invalid numeric string for Color component."),
			_ => throw new JsonException($"Unexpected token type for color component: {reader.TokenType}")
		};

	private static void ValidateByteRange(int v, string name)
	{
		if (v < 0 || v > 255)
		{
			throw new JsonException($"Color component {name} must be between 0 and 255. Actual: {v}");
		}
	}
}
