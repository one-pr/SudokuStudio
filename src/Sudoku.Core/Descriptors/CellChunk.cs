namespace Sudoku.Descriptors;

/// <summary>
/// Represents a cell chunk.
/// </summary>
//[StructLayout(LayoutKind.Explicit)]
[JsonConverter(typeof(ChunkConverter<CellChunk>))]
public sealed class CellChunk : Chunk, IChunkCreator<CellChunk, CellMap, Cell>
{
	/// <summary>
	/// Indicates the backing single element.
	/// </summary>
	[ChunkElement(ChunkElement.Single, ConverterType = typeof(SingleConverter))]
	//[FieldOffset(0)]
	internal Cell _element;

	/// <summary>
	/// Indicates the backing cell map.
	/// </summary>
	[ChunkElement(ChunkElement.BitStateMap, ConverterType = typeof(CellMapConverter))]
	//[FieldOffset(0)]
	internal CellMap _map;

	/// <summary>
	/// Indicates the backing array.
	/// </summary>
	[ChunkElement(ChunkElement.Array, ConverterType = typeof(CollectionConverter<Cell[]>))]
	//[FieldOffset(0)]
	internal Cell[]? _array;

	/// <summary>
	/// Indicates the backing list.
	/// </summary>
	[ChunkElement(ChunkElement.List, ConverterType = typeof(CollectionConverter<List<Cell>>))]
	//[FieldOffset(0)]
	internal List<Cell>? _list;

	/// <summary>
	/// Indicates the backing hash set.
	/// </summary>
	[ChunkElement(ChunkElement.HashSet, ConverterType = typeof(CollectionConverter<HashSet<Cell>>))]
	//[FieldOffset(0)]
	internal HashSet<Cell>? _hashSet;


	/// <inheritdoc/>
	public static CellChunk Create(in CellMap map)
		=> new() { _map = map, Element = ChunkElement.BitStateMap, Type = ChunkType.Cell };

	/// <inheritdoc/>
	public static CellChunk Create(Cell element)
		=> new() { _element = element, Element = ChunkElement.Single, Type = ChunkType.Cell };

	/// <inheritdoc/>
	public static CellChunk Create(Cell[] array)
		=> new() { _array = array, Element = ChunkElement.Array, Type = ChunkType.Cell };

	/// <inheritdoc/>
	public static CellChunk Create(List<Cell> list)
		=> new() { _list = list, Element = ChunkElement.List, Type = ChunkType.Cell };

	/// <inheritdoc/>
	public static CellChunk Create(HashSet<Cell> hashSet)
		=> new() { _hashSet = hashSet, Element = ChunkElement.HashSet, Type = ChunkType.Cell };
}

/// <summary>
/// Represents a single element converter.
/// </summary>
file sealed class SingleConverter : ChunkElementConverter
{
	/// <inheritdoc/>
	public override string Format(object value, IFormatProvider? formatProvider)
	{
		var converter = CoordinateConverter.GetInstance(formatProvider);
		return converter.CellConverter(((Cell)value).AsCellMap());
	}

	/// <inheritdoc/>
	public override object Read(ref Utf8JsonReader reader, IFormatProvider? formatProvider, Type typeToConvert, JsonSerializerOptions options)
	{
		reader.Read();
		return /*(Cell)*/reader.GetInt32();
	}

	/// <inheritdoc/>
	public override void Write(Utf8JsonWriter writer, object value, IFormatProvider? formatProvider, JsonSerializerOptions options)
		=> writer.WriteNumberValue((Cell)value);
}

/// <summary>
/// Represents a cell map converter.
/// </summary>
file sealed class CellMapConverter : ChunkElementConverter
{
	/// <inheritdoc/>
	public override string Format(object value, IFormatProvider? formatProvider)
	{
		var converter = CoordinateConverter.GetInstance(formatProvider);
		return converter.CellConverter((CellMap)value);
	}

	/// <inheritdoc/>
	public override object Read(ref Utf8JsonReader reader, IFormatProvider? formatProvider, Type typeToConvert, JsonSerializerOptions options)
	{
		reader.Read();
		var str = reader.GetString()!;
		var parser = CoordinateParser.GetInstance(formatProvider);
		return parser.CellParser(str);
	}

	/// <inheritdoc/>
	public override void Write(Utf8JsonWriter writer, object value, IFormatProvider? formatProvider, JsonSerializerOptions options)
	{
		var converter = CoordinateConverter.GetInstance(formatProvider);
		var str = converter.CellConverter((CellMap)value);
		writer.WriteStringValue(str);
	}
}

/// <summary>
/// Represents collection converter.
/// </summary>
/// <typeparam name="T">The type of collection.</typeparam>
file sealed class CollectionConverter<T> : ChunkElementConverter where T : IEnumerable<Cell>
{
	/// <summary>
	/// Indicates the backing converter.
	/// </summary>
	private readonly CellMapConverter _backingConverter = new();


	/// <inheritdoc/>
	public override string Format(object value, IFormatProvider? formatProvider)
	{
		var map = CellMap.Empty;
		foreach (var cell in (T)value)
		{
			map += cell;
		}
		return _backingConverter.Format(map, formatProvider);
	}

	/// <inheritdoc/>
	public override object Read(ref Utf8JsonReader reader, IFormatProvider? formatProvider, Type typeToConvert, JsonSerializerOptions options)
	{
		var cells = (CellMap)_backingConverter.Read(ref reader, formatProvider, typeToConvert, options);
		if (typeof(T) == typeof(Cell[]))
		{
			// Array. We cannot add them into collection.
			return cells.ToArray();
		}

		if (typeof(T).IsAssignableTo(typeof(ICollection<Cell>)))
		{
			// Mutable collection.
			var result = (ICollection<Cell>)Activator.CreateInstance<T>();
			foreach (var cell in cells)
			{
				result.Add(cell);
			}
			return (T)result;
		}

		throw new InvalidOperationException("Invalid type.");
	}

	/// <inheritdoc/>
	public override void Write(Utf8JsonWriter writer, object value, IFormatProvider? formatProvider, JsonSerializerOptions options)
	{
		var map = CellMap.Empty;
		foreach (var cell in (T)value)
		{
			map += cell;
		}
		_backingConverter.Write(writer, map, formatProvider, options);
	}
}
