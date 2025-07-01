namespace Sudoku.IO;

/// <summary>
/// Provides functionality to sort very large text files that cannot fit into memory.
/// Utilizes external merge sort strategy with asynchronous and thread-safe operations.
/// </summary>
/// <param name="_inputPath">Path to the input file to be sorted.</param>
/// <param name="_outputPath">Path to the file where sorted lines will be written.</param>
/// <param name="_linesPerChunk">Maximum number of lines to load into memory per chunk.</param>
internal sealed class FileExternalSorter(string _inputPath, string _outputPath, int _linesPerChunk = 500_000)
{
	/// <summary>
	/// Sorts a large text file line by line in ascending order, using external sorting strategy.
	/// The sorted result is written to the specified output file.
	/// </summary>
	/// <returns>The number of lines of result file output.</returns>
	public async Task<int> SortLargeFileAsync()
	{
		var tempFiles = new ConcurrentBag<string>(); // Stores paths of temporary sorted chunk files.
		var chunkTasks = new List<Task>(); // Stores asynchronous chunk sort tasks.

		using (var reader = new StreamReader(_inputPath))
		{
			var chunkIndex = 0;
			while (!reader.EndOfStream)
			{
				var lines = new List<string>(_linesPerChunk);

				// Read a chunk of lines from input file.
				for (var i = 0; i < _linesPerChunk && !reader.EndOfStream; i++)
				{
					if (await reader.ReadLineAsync() is { } line && !string.IsNullOrWhiteSpace(line))
					{
						lines.Add(line);
					}
				}

				// Capture the chunk and sort it in a separate thread.
				chunkIndex++;
				var linesCopy = new List<string>(lines);

				var task = Task.Run(
					async () =>
					{
						linesCopy.Sort(StringComparer.Ordinal); // Sort using default C# string comparison.
						var tempFile = Path.GetTempFileName();
						await File.WriteAllLinesAsync(tempFile, linesCopy); // Write sorted chunk to temp file.
						tempFiles.Add(tempFile);
					}
				);
				chunkTasks.Add(task);
			}
		}

		// Wait for all chunk sort tasks to complete.
		await Task.WhenAll(chunkTasks.AsSpan());

		// Merge sorted chunks into final output.
		var resultFileLines = await MergeSortedFilesAsync([.. tempFiles]);

		// Clean up temporary files.
		foreach (var file in tempFiles)
		{
			try
			{
				File.Delete(file);
			}
			catch
			{
				// Ignore delete errors.
			}
		}

		return resultFileLines;
	}

	/// <summary>
	/// Merges multiple pre-sorted text files into a single sorted output file.
	/// </summary>
	/// <param name="sortedFiles">List of paths to sorted temporary files.</param>
	/// <returns>The number of lines of result file output.</returns>
	private async Task<int> MergeSortedFilesAsync(List<string> sortedFiles)
	{
		var readers = new List<StreamReader>();

		// Open all sorted chunk files.
		foreach (var file in sortedFiles)
		{
			readers.Add(new StreamReader(file));
		}

		var queue = new SortedDictionary<string, List<int>>(StringComparer.Ordinal);

		// Initialize priority queue with the first line from each reader.
		for (var i = 0; i < readers.Count; i++)
		{
			if (!readers[i].EndOfStream
				&& await readers[i].ReadLineAsync() is { } line
				&& !string.IsNullOrWhiteSpace(line)
				&& !queue.TryAdd(line, [i]))
			{
				queue[line].Add(i);
			}
		}

		var totalWrittenLines = 0;
		await using (var writer = new StreamWriter(_outputPath))
		{
			// Perform k-way merge.
			while (queue.Count != 0)
			{
				var kvp = queue.First();
				var minLine = kvp.Key;
				var readerIndex = kvp.Value[0];

				// Write smallest line to output.
				await writer.WriteLineAsync(minLine);
				Interlocked.Increment(ref totalWrittenLines);

				// Remove used index.
				kvp.Value.RemoveAt(0);
				if (kvp.Value.Count == 0)
				{
					// Remove line if no more sources.
					queue.Remove(minLine);
				}

				var reader = readers[readerIndex];
				if (!reader.EndOfStream
					&& await reader.ReadLineAsync() is { } nextLine
					&& !string.IsNullOrWhiteSpace(nextLine)
					&& !queue.TryAdd(nextLine, [readerIndex]))
				{
					queue[nextLine].Add(readerIndex);
				}
			}
		}

		// Dispose all readers.
		readers.ForEach(static reader => reader.Dispose());

		return totalWrittenLines;
	}
}
