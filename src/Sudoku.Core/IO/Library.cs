namespace Sudoku.IO;

/// <summary>
/// Represents a puzzle library.
/// </summary>
/// <param name="_directoryPath">Indicates the directory path.</param>
/// <param name="_identifier">Indicates the library identifier.</param>
/// <remarks>
/// <para>
/// In design, you can use constructor to create two files
/// (raw text file and information file, with extensions <c>.txt</c> and <c>.json</c>)
/// if the target directory doesn't contain such related files.
/// However, this method won't check for bound files on purpose. If the local path doesn't contain any files
/// (raw text file named <c>*.txt</c> or its information file named <c>*.json</c>), this method <b>will not</b> create files;
/// instead, you should call methods <c>Write*</c> to update information,
/// like <see cref="WriteName(string)"/> to name this library.
/// </para>
/// <para>
/// Also, if you want to create a library instance via a library raw text file (a list of puzzles stored in a file),
/// You can call <see langword="static"/> method <see cref="CreateLibrary(string, string, string)"/> to copy desired file
/// to target folder.
/// </para>
/// </remarks>
/// <seealso cref="CreateLibrary(string, string, string)"/>
[TypeImpl(TypeImplFlags.Object_Equals | TypeImplFlags.Equatable | TypeImplFlags.EqualityOperators)]
public sealed partial class Library(string _directoryPath, string _identifier) :
	IAsyncEnumerable<string>,
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
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
	};

	/// <summary>
	/// Indicates the lock object to keep operation thread-safe.
	/// </summary>
	private static readonly Lock FileLock = new();


	/// <summary>
	/// Indicates whether the file is not created, or an empty file.
	/// </summary>
	public bool IsEmpty => !File.Exists(LibraryPath) || !File.ReadLines(LibraryPath).Any();

	/// <summary>
	/// Indicates the information path.
	/// </summary>
	[EquatableMember]
	public string InfoPath => $@"{_directoryPath}\{_identifier}.json";

	/// <summary>
	/// Indicates the library path.
	/// </summary>
	public string LibraryPath => $@"{_directoryPath}\{_identifier}.txt";

	/// <summary>
	/// Indicates the last modified time of the library file.
	/// </summary>
	public DateTime LastModifiedTime => File.GetLastWriteTime(LibraryPath);


	/// <summary>
	/// Writes the name to the library information file.
	/// Please note that the name isn't related to the file name at local file path.
	/// It means the real name you want to call this library.
	/// </summary>
	/// <param name="value">The value to be set.</param>
	public void WriteName(string value) => WriteProperty(static (info, value) => info.Name = value, value);

	/// <summary>
	/// Writes the description to the library information file.
	/// </summary>
	/// <param name="value">The value to be set.</param>
	public void WriteDescription(string value) => WriteProperty(static (info, value) => info.Description = value, value);

	/// <summary>
	/// Writes the author to the library information file.
	/// </summary>
	/// <param name="value">The value to be set.</param>
	public void WriteAuthor(string value) => WriteProperty(static (info, value) => info.Author = value, value);

	/// <summary>
	/// Writes the tags to the library information file.
	/// If the original file has any tags, the current value will cover that value.
	/// </summary>
	/// <param name="value">The value to be set.</param>
	public void WriteTags(params ReadOnlySpan<string> value)
		=> WriteProperty(static (info, value) => info.Tags = DeduplicateTags(value), value);

	/// <summary>
	/// Writes a list of new tags, appending them into the last of tags array in the library information file.
	/// </summary>
	/// <param name="value">The vale to be set.</param>
	public void AppendTags(params ReadOnlySpan<string> value)
		=> WriteProperty(static (info, value) => info.Tags = DeduplicateTags([.. info.Tags, .. value]), value);

	/// <summary>
	/// Clears all puzzles in the library.
	/// </summary>
	public void Clear() => File.WriteAllText(LibraryPath, string.Empty);

	/// <summary>
	/// Deletes the library. <b>Please be careful to do this; you must discard the current instance if files are deleted.</b>
	/// </summary>
	public void Delete()
	{
		File.Delete(InfoPath);
		File.Delete(LibraryPath);
	}

	/// <inheritdoc/>
	public override int GetHashCode() => HashCode.Combine(_directoryPath, _identifier);

	/// <summary>
	/// Reads the name of the library information file.
	/// If the information file doesn't exist, create one and return default value (empty string).
	/// </summary>
	/// <returns>The name.</returns>
	public string ReadName() => ReadProperty(static info => info.Name);

	/// <summary>
	/// Reads the description of the library information file.
	/// If the information file doesn't exist, create one and return default value (empty string).
	/// </summary>
	/// <returns>The description.</returns>
	public string ReadDescription() => ReadProperty(static info => info.Description);

	/// <summary>
	/// Reads the author of the library information file.
	/// If the information file doesn't exist, create one and return default value (empty string).
	/// </summary>
	/// <returns>The author.</returns>
	public string ReadAuthor() => ReadProperty(static info => info.Author);

	/// <inheritdoc/>
	public override string ToString()
		=> string.Format(
			SR.Get("LibraryInformation"),
			ReadName() is var name and not "" ? name : SR.Get("LibraryInformation_NoName"),
			LibraryPath,
			File.Exists(LibraryPath)
				? SR.Get("LibraryInformation_FileCreated")
				: SR.Get("LibraryInformation_FileNotCreated"),
			ReadDescription() is var description and not "" ? description : SR.Get("LibraryInformation_NoDescription"),
			ReadAuthor() is var author and not "" ? author : SR.Get("LibraryInformation_NoAuthor"),
			ReadTags() is var tags and not [] ? string.Join(", ", tags) : SR.Get("LibraryInformation_NoTags")
		);

	/// <summary>
	/// Reads the tags of the library information file.
	/// If the information file doesn't exist, create one and return default value (empty array).
	/// </summary>
	/// <returns>The tags.</returns>
	public ReadOnlySpan<string> ReadTags() => ReadProperty(static info => info.Tags);

	/// <summary>
	/// Writes a new grid into the target file; if the file doesn't exist, it will create a new file,
	/// and then append the puzzle into the file.
	/// </summary>
	/// <param name="grid">The grid to be appended.</param>
	/// <param name="cancellationToken">The cancellation token that can cancel the current operation.</param>
	/// <returns>A <see cref="Task"/> object that handles the asynchronous operation.</returns>
	public async ValueTask WriteLineAsync(string grid, CancellationToken cancellationToken = default)
	{
		if (!Grid.TryParse(grid, out _))
		{
			return;
		}

		await using var writer = new LibraryFileWriter(LibraryPath, out _);
		await writer.WriteLineAsync(grid, cancellationToken);
	}

	/// <summary>
	/// Totals the number of puzzles up, and return the result.
	/// </summary>
	/// <returns>
	/// A <see cref="Task{TResult}"/> object that handles the asynchronous operation;
	/// with a result type <see cref="ulong"/> indicating the number of lines.
	/// </returns>
	public async ValueTask<ulong> GetCountAsync()
	{
		await using var reader = new LibraryFileReader(LibraryPath, out var exists);
		return exists ? reader.CountLines() : 0;
	}

	/// <summary>
	/// Gets the puzzle at the specified index.
	/// If the desired index is out of range, this method will return a <see langword="null"/> string
	/// instead of throwing any exceptions.
	/// </summary>
	/// <param name="index">The index, start at 0.</param>
	/// <param name="cancellationToken">The cancellation token that can cancel the current operation.</param>
	/// <returns>
	/// A <see cref="Task{TResult}"/> of <see cref="string"/> object that can handle the asynchronous operation;
	/// if canceled, the return value is also <see langword="null"/>.
	/// </returns>
	public async ValueTask<string?> GetIndexAtAsync(ulong index, CancellationToken cancellationToken = default)
	{
		await using var reader = new LibraryFileReader(LibraryPath, out var exists);
		if (!exists)
		{
			// If the file is created just now, nothing will be iterated.
			return null;
		}

		await foreach (var line in reader.ReadLinesRangeAsync(index + 1, index + 2, cancellationToken))
		{
			return line;
		}
		return null;
	}

	/// <summary>
	/// Randomly selects one puzzle.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token that can cancel the current operation.</param>
	/// <returns>
	/// The <see cref="ValueTask{TResult}"/> of <see cref="string"/>? object
	/// that can asynchronously handle the operation, and return the result;
	/// if canceled while searching, <see langword="null"/> will be returned.
	/// </returns>
	public async ValueTask<string?> SelectOneAsync(CancellationToken cancellationToken = default)
	{
		await foreach (var result in SelectMultipleAsync(1, cancellationToken))
		{
			return result;
		}
		return null;
	}

	/// <summary>
	/// Sort the puzzles in the library.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token that can cancel the current operation.</param>
	/// <returns>A <see cref="Task"/> object that handles the asynchronous operation.</returns>
	public async Task SortAsync(CancellationToken cancellationToken = default)
	{
		var tempFile = Path.GetTempFileName();
		await new FileExternalSorter(LibraryPath, tempFile, 50_000).SortLargeFileAsync();

		if (cancellationToken.IsCancellationRequested)
		{
			lock (FileLock)
			{
				File.Delete(tempFile);
			}
			return;
		}

		lock (FileLock)
		{
			try
			{
				File.Delete(LibraryPath);
				File.Move(tempFile, LibraryPath);
			}
			catch
			{
			}
		}
	}

	/// <summary>
	/// Deduplicate the puzzles in the library.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token that can cancel the current operation.</param>
	/// <returns>A <see cref="Task"/> object that handles the asynchronous operation.</returns>
	public async Task DeduplicateAsync(CancellationToken cancellationToken = default)
	{
		var tempFile = Path.GetTempFileName();
		await new FileDeduplicator(LibraryPath, tempFile).DeduplicateAsync(cancellationToken);

		lock (FileLock)
		{
			try
			{
				File.Delete(LibraryPath);
				File.Move(tempFile, LibraryPath);
			}
			catch
			{
			}
		}
	}

	/// <summary>
	/// Writes a list of new grids into the target file; if the file does't exist, it will create a new file,
	/// and then append the puzzle into the file.
	/// </summary>
	/// <param name="lines">The grids to be appended.</param>
	/// <param name="cancellationToken">The cancellation token that can cancel the current operation.</param>
	/// <returns>A <see cref="Task"/> object that handles the asynchronous operation.</returns>
	public async Task WriteLinesAsync(IEnumerable<string> lines, CancellationToken cancellationToken = default)
	{
		await using var writer = new LibraryFileWriter(LibraryPath, out _);
		foreach (var line in lines)
		{
			if (Grid.TryParse(line, out _))
			{
				await writer.WriteLineAsync(line, cancellationToken);
			}
		}
	}

	/// <inheritdoc cref="WriteLinesAsync(IEnumerable{string}, CancellationToken)"/>
	public async Task WriteLinesAsync(IAsyncEnumerable<string> lines, CancellationToken cancellationToken = default)
	{
		await using var writer = new LibraryFileWriter(LibraryPath, out _);
		await foreach (var line in lines)
		{
			if (Grid.TryParse(line, out _))
			{
				await writer.WriteLineAsync(line, cancellationToken);
			}
		}
	}

	/// <summary>
	/// Loads the other library, and reads for all puzzles and appends to the current library file.
	/// If any exceptions thrown or cancelled, no updates will be applied
	/// (the current file will keep the original state, rather than successfully written some puzzles before cancelled
	/// or exception encountered).
	/// </summary>
	/// <param name="other">The other library to merge into the current library.</param>
	/// <param name="cancellationToken">The cancellation token that can cancel the current operation</param>
	/// <returns>
	/// A <see cref="Task{TResult}"/> object that handles the asynchronous operation;
	/// with a <see cref="bool"/> result indicating whether the operation is failed (cancelled, exception encountered, etc.).
	/// </returns>
	public async Task<bool> TryMergeFromAsync(Library other, CancellationToken cancellationToken = default)
	{
		var tempFilePath = default(string);
		var backupFilePath = default(string);
		var originalFilePath = LibraryPath;
		await using var otherReader = new LibraryFileReader(other.LibraryPath, out var exists);
		if (!exists)
		{
			// The target file doesn't exist.
			return true;
		}

		try
		{
			// Generate a temporary file.
			tempFilePath = Path.GetTempFileName();
			backupFilePath = $"{originalFilePath}.bak";

			// Copies the current file into backup file.
			File.Copy(originalFilePath, tempFilePath, true);

			// The core operation to append lines.
			// Please note that this await foreach operation may throw OperationCanceledException,
			// so it will be catched and make a rollback instead of skipping the foreach loop.
			await using (var sw = new StreamWriter(tempFilePath, true))
			{
				await foreach (var line in otherReader.ReadAllLinesAsync(cancellationToken))
				{
					if (Grid.TryParse(line, out _))
					{
						await sw.WriteLineAsync(line);
					}
				}
			}

			// Replace with original file.
			File.Replace(tempFilePath, originalFilePath, backupFilePath, true);

			// Successful. Now delete the backup file.
			File.Delete(backupFilePath);
		}
		catch
		{
			rollback(originalFilePath, backupFilePath);
			return false;
		}
		finally
		{
			// Delete temporary file if exists.
			if (tempFilePath is not null && File.Exists(tempFilePath))
			{
				File.Delete(tempFilePath);
			}
		}
		return true;


		static void rollback(string originalFilePath, string? backupFilePath)
		{
			try
			{
				// If the backup file exists, the operation will be failed.
				if (File.Exists(backupFilePath))
				{
					// Recover the file from backup.
					File.Replace(backupFilePath, originalFilePath, null);
				}
			}
			catch
			{
			}
		}
	}

	/// <summary>
	/// Randomly selects the specified number of puzzles to be used.
	/// </summary>
	/// <param name="count">The desired number of puzzles.</param>
	/// <param name="cancellationToken">The cancellation token that can cancel the current operation.</param>
	/// <returns>An enumerable sequence that is used in asynchronous environment.</returns>
	public async IAsyncEnumerable<string> SelectMultipleAsync(
		ulong count,
		[EnumeratorCancellation] CancellationToken cancellationToken = default
	)
	{
		var rng = new IndexGenerator(0, await GetCountAsync());
		for (var i = 0UL; i < count; i++)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				yield break;
			}

			if (rng.NextUnique(cancellationToken) is { } nextIndex
				&& await GetIndexAtAsync(nextIndex, cancellationToken) is { } validNext)
			{
				yield return validNext;
			}
		}
	}

	/// <summary>
	/// Reads the specified number of puzzles from the library file
	/// without any conversions to <see cref="Grid"/> (only displays the raw text).
	/// If the file doesn't exist, nothing will be returned.
	/// </summary>
	/// <param name="start">Indicates the start index.</param>
	/// <param name="length">Indicates the desired number of puzzles.</param>
	/// <param name="cancellationToken">The cancellation token that can cancel the current operation.</param>
	/// <returns>An enumerable object that allows iterating values asynchronously.</returns>
	public async IAsyncEnumerable<string> GetRangeAsync(
		ulong start,
		ulong length,
		[EnumeratorCancellation] CancellationToken cancellationToken = default
	)
	{
		await using var reader = new LibraryFileReader(LibraryPath, out var exists);
		if (!exists)
		{
			// If the file is created just now, nothing will be iterated.
			yield break;
		}

		await foreach (var line in reader.ReadLinesRangeAsync(start, start + length, cancellationToken))
		{
			yield return line;
		}
	}

	/// <inheritdoc/>
	public async IAsyncEnumerator<string> GetAsyncEnumerator(CancellationToken cancellationToken = default)
	{
		await using var reader = new LibraryFileReader(LibraryPath, out var exists);
		if (!exists)
		{
			yield break;
		}

		await foreach (var line in reader.ReadAllLinesAsync(cancellationToken))
		{
			yield return line;
		}
	}

	/// <summary>
	/// Writes the property of type <typeparamref name="T"/> to the file.
	/// </summary>
	/// <typeparam name="T">The type of value.</typeparam>
	/// <param name="valueAssignment">The result value assigning method.</param>
	/// <param name="value">The value to be set.</param>
	private void WriteProperty<T>(Action<LibraryInfo, T> valueAssignment, T value) where T : allows ref struct
	{
		var info = LoadOrCreate();
		valueAssignment(info, value);
		Save(info);
	}

	/// <summary>
	/// Saves the file.
	/// </summary>
	/// <param name="info">The information to be saved.</param>
	private void Save(LibraryInfo info)
	{
		lock (FileLock)
		{
			var json = JsonSerializer.Serialize(info, DefaultSerializerOptions);
			File.WriteAllText(InfoPath, json);
		}
	}

	/// <summary>
	/// Reads the property of value of type <typeparamref name="T"/>.
	/// </summary>
	/// <typeparam name="T">The type of value.</typeparam>
	/// <param name="resultValueCreator">The result value creator.</param>
	/// <returns>The result value.</returns>
	private T ReadProperty<T>(Func<LibraryInfo, T> resultValueCreator) where T : allows ref struct
	{
		var info = LoadOrCreate();
		return resultValueCreator(info);
	}

	/// <summary>
	/// Loads the file, or creates the default instance if the file doesn't exist.
	/// </summary>
	/// <returns>The file loaded.</returns>
	private LibraryInfo LoadOrCreate()
	{
		lock (FileLock)
		{
			if (!File.Exists(InfoPath))
			{
				var result = new LibraryInfo();

				// Create a file and write values.
				var tempJson = JsonSerializer.Serialize(result, DefaultSerializerOptions);
				File.WriteAllText(InfoPath, tempJson);

				return result;
			}

			var json = File.ReadAllText(InfoPath);
			return JsonSerializer.Deserialize<LibraryInfo>(json, DefaultSerializerOptions) ?? new();
		}
	}


	/// <summary>
	/// Rename the library with file name changed; this method won't do anything if <paramref name="newIdentifier"/> isn't changed.
	/// </summary>
	/// <param name="library">The desired library.</param>
	/// <param name="newIdentifier">The new identifier you want to change its backing file name.</param>
	/// <returns>The renamed library instance created; if the target file exists, <see langword="null"/> will be returned.</returns>
	public static Library? RenameLibrary(Library library, string newIdentifier)
	{
		newIdentifier = newIdentifier.Trim();
		if (Path.GetFileNameWithoutExtension(library.LibraryPath) == newIdentifier)
		{
			// Do nothing.
			return new(Path.GetDirectoryName(library.LibraryPath)!, newIdentifier);
		}

		lock (FileLock)
		{
			var directory = Path.GetDirectoryName(library.LibraryPath)!;
			var oldLibraryFilePath = library.LibraryPath;
			var oldJsonFilePath = library.InfoPath;
			var newLibraryFilePath = $@"{directory}\{newIdentifier}.txt";
			var newJsonFilePath = $@"{directory}\{newIdentifier}.json";
			if (File.Exists(newLibraryFilePath) || File.Exists(newJsonFilePath))
			{
				// There's at least one bound file exists. Don't overwrite it and return null.
				return null;
			}

			File.Move(oldLibraryFilePath, newLibraryFilePath, false);
			File.Move(oldJsonFilePath, newJsonFilePath, false);
			return new(directory, newIdentifier);
		}
	}

	/// <summary>
	/// Creates a new library from the specified raw text file.
	/// After created the instance, you should use methods <c>Write*</c> to change the configuration of the library.
	/// </summary>
	/// <param name="originalFilePath">The original text file.</param>
	/// <param name="targetDirectory">The target directory.</param>
	/// <param name="identifier">The file ID, meaning the target library file name.</param>
	/// <returns>The <see cref="Library"/> instance that you can visit the backing data and update information.</returns>
	/// <remarks>
	/// In default behavior, this method will directly copy the file from path <paramref name="originalFilePath"/>
	/// to the target directory <paramref name="targetDirectory"/> with file name <paramref name="identifier"/>,
	/// and create a JSON file as the same name as the created library text file.
	/// </remarks>
	public static Library CreateLibrary(string originalFilePath, string targetDirectory, string identifier)
	{
		lock (FileLock)
		{
			var directory = Path.GetDirectoryName(targetDirectory)!;
			Directory.CreateDirectory(directory);

			// Copies the original file as the default behavior.
			File.Copy(originalFilePath, $@"{targetDirectory}\{identifier}.txt");

			var result = new Library(targetDirectory, identifier);
			result.WriteName("<Unnamed>");
			result.WriteAuthor("<Anonymous>");
			result.WriteTags();
			return result;
		}
	}

	/// <summary>
	/// Deduplicate tags.
	/// </summary>
	/// <param name="values">The value.</param>
	/// <returns>The result.</returns>
	private static string[] DeduplicateTags(params ReadOnlySpan<string> values)
	{
		var result = new SortedSet<string>();
		foreach (var value in values)
		{
			result.Add(value.Trim());
		}
		return [.. result];
	}
}

/// <summary>
/// Generates unique random unsigned long integers within a specified range <c>[<paramref name="_a"/>, <paramref name="_b"/>)</c>.
/// Designed for large ranges without allocating or shuffling large arrays.
/// </summary>
/// <param name="_a">Inclusive lower bound of the range.</param>
/// <param name="_b">Exclusive upper bound of the range.</param>
/// <param name="seed">Optional seed for reproducible randomness.</param>
/// <exception cref="ArgumentException">Thrown if b is not greater than a.</exception>
file sealed class IndexGenerator(ulong _a, ulong _b, int? seed = null)
{
	/// <summary>
	/// Indicates the backing random number generator.
	/// </summary>
	private readonly Random _random = seed is { } seedValue ? new(seedValue) : new();

	/// <summary>
	/// Set of already generated values.
	/// </summary>
	private readonly HashSet<ulong> _generated = [];


	/// <summary>
	/// Returns the next unique random number in the range [a, b), or null if all values have been exhausted.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token that can cancel the current operation.</param>
	/// <returns>
	/// A unique random <see cref="ulong"/>, or <see langword="null"/> if all possible values have been generated,
	/// or canceled.
	/// </returns>
	public ulong? NextUnique(CancellationToken cancellationToken = default)
	{
		if ((ulong)_generated.Count >= _b - _a)
		{
			// All possible unique values are exhausted.
			return null;
		}

		while (true)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return null;
			}

			var candidate = _a + NextUInt64(_b - _a);
			if (_generated.Add(candidate))
			{
				// Return only if not already generated.
				return candidate;
			}
		}
	}

	/// <summary>
	/// Generates a random <see cref="ulong"/> in the range [0, range).
	/// </summary>
	/// <param name="range">The upper bound (exclusive) for the generated value.</param>
	/// <returns>A random <see cref="ulong"/> in [0, range).</returns>
	private ulong NextUInt64(ulong range)
	{
		ulong ulongRand;
		do
		{
			var buf = new byte[8];
			_random.NextBytes(buf); // Fill with random bytes.
			ulongRand = BitConverter.ToUInt64(buf, 0);
		}
		while (ulongRand >= ulong.MaxValue - (ulong.MaxValue % range + 1) % range); // Avoid modulo bias.

		return ulongRand % range;
	}
}
