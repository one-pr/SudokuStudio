namespace Sudoku.IO;

/// <summary>
/// Represents a puzzle library.
/// </summary>
/// <param name="directoryPath"><inheritdoc cref="_directoryPath" path="/summary"/></param>
/// <param name="identifier"><inheritdoc cref="_identifier" path="/summary"/></param>
[TypeImpl(TypeImplFlags.AllObjectMethods | TypeImplFlags.Equatable | TypeImplFlags.EqualityOperators)]
public sealed partial class Library(string directoryPath, string identifier) :
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


	/// <summary>
	/// Writes the name to the library information file.
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

	/// <summary>
	/// Reads the tags of the library information file.
	/// If the information file doesn't exist, create one and return default value (empty array).
	/// </summary>
	/// <returns>The tags.</returns>
	public ReadOnlySpan<string> ReadTags() => ReadProperty(static info => info.Tags);

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
			lock (_fileLock)
			{
				File.Delete(tempFile);
			}
			return;
		}

		lock (_fileLock)
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

		lock (_fileLock)
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
	/// Writes a new grid into the target file; if the file doesn't exist, it will create a new file,
	/// and then append the puzzle into the file.
	/// </summary>
	/// <param name="grid">The grid to be appended.</param>
	/// <param name="cancellationToken">The cancellation token that can cancel the current operation.</param>
	/// <returns>A <see cref="Task"/> object that handles the asynchronous operation.</returns>
	public async Task WriteLineAsync(string grid, CancellationToken cancellationToken = default)
	{
		if (!Grid.TryParse(grid, out _))
		{
			return;
		}

		await using var writer = new LibraryFileWriter(LibraryPath, out _);
		await writer.WriteLineAsync(grid, cancellationToken);
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
	/// Totals the number of puzzles up, and return the result.
	/// </summary>
	/// <returns>
	/// A <see cref="Task{TResult}"/> object that handles the asynchronous operation;
	/// with a result type <see cref="long"/> indicating the number of lines.
	/// </returns>
	public async Task<long> GetPuzzlesCountAsync()
	{
		await using var reader = new LibraryFileReader(LibraryPath, out var exists);
		return exists ? reader.CountLines() : 0;
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
	public async IAsyncEnumerable<string> ReadRangeAsync(
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
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
		lock (_fileLock)
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
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
		lock (_fileLock)
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
	/// Creates a <see cref="Library"/> instance (and local files) via the specified information.
	/// </summary>
	/// <param name="directoryPath">The directory path.</param>
	/// <param name="identifier">The identifier.</param>
	/// <param name="libraryInfo">The library information.</param>
	/// <returns>The library instance.</returns>
	public static Library CreateLibrary(string directoryPath, string identifier, LibraryInfo libraryInfo)
	{
		var result = new Library(directoryPath, identifier);
		var info = result.LoadOrCreate();
		info.Name = libraryInfo.Name;
		info.Author = libraryInfo.Author;
		info.Description = libraryInfo.Description;
		info.Tags = libraryInfo.Tags;
		result.Save(info);
		return result;
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
