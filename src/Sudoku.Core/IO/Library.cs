namespace Sudoku.IO;

/// <summary>
/// Represents a puzzle library.
/// </summary>
/// <param name="directoryPath"><inheritdoc cref="_directoryPath" path="/summary"/></param>
/// <param name="identifier"><inheritdoc cref="_identifier" path="/summary"/></param>
[TypeImpl(TypeImplFlags.AllObjectMethods | TypeImplFlags.Equatable | TypeImplFlags.EqualityOperators)]
public sealed partial class Library(string directoryPath, string identifier) :
	IAsyncEnumerable<Grid>,
	IEquatable<Library>,
	IEqualityOperators<Library, Library, bool>
{
	/// <summary>
	/// Indicates the JSON options to serialize or deserialize object.
	/// </summary>
	private static readonly JsonSerializerOptions DefaultSerializerOptions = new()
	{
		WriteIndented = true,
		IndentCharacter = ' ',
		IndentSize = 4,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase
	};


	/// <summary>
	/// Indicates the directory path.
	/// </summary>
	private readonly string _directoryPath = directoryPath;

	/// <summary>
	/// Indicates the library identifier.
	/// </summary>
	private readonly string _identifier = identifier;

	/// <summary>
	/// Indicates the lock object to keep operation thread-safe.
	/// </summary>
	private readonly Lock _fileLock = new();


	/// <summary>
	/// Indicates the information path.
	/// </summary>
	[HashCodeMember]
	[EquatableMember]
	[StringMember]
	public string InfoPath => $"{_directoryPath}/{_identifier}.json";

	/// <summary>
	/// Indicates the library path.
	/// </summary>
	[StringMember]
	public string LibraryPath => $"{_directoryPath}/{_identifier}.txt";
}
