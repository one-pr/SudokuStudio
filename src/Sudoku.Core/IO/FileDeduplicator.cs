namespace Sudoku.IO;

/// <summary>
/// Deduplicates lines in a large text file using multithreading and concurrent collections.
/// This class is optimized for performance and scalability, especially for very large input files.
/// </summary>
/// <param name="_inputPath">Path to the input text file.</param>
/// <param name="_outputPath">Path to the output file containing deduplicated lines.</param>
/// <param name="_workerCount">Number of worker threads for parallel deduplication. The default value is 4.</param>
/// <param name="_boundedCapacity">The bounded size of the collection while processing. The default value is 10000.</param>
internal sealed class FileDeduplicator(string _inputPath, string _outputPath, int _workerCount = 4, int _boundedCapacity = 10_000)
{
	/// <summary>
	/// Asynchronously performs line deduplication using a producer-consumer model.
	/// </summary>
	/// <param name="cancellationToken">Cancellation token to abort the operation.</param>
	public async Task DeduplicateAsync(CancellationToken cancellationToken = default)
	{
		// Thread-safe set to track seen lines.
		var seen = new ConcurrentDictionary<string, byte>();

		// Bounded buffer for passing lines from producer to consumers.
		var lines = new BlockingCollection<string>(_boundedCapacity);

		// Producer task: reads lines from the file and adds them to the line buffer.
		var producer = Task.Run(
			() =>
			{
				using var reader = new StreamReader(_inputPath);
				string? line;
				while ((line = reader.ReadLine()) is not null)
				{
					cancellationToken.ThrowIfCancellationRequested();
					lines.Add(line, cancellationToken);
				}
				lines.CompleteAdding(); // Signal that no more items will be added.
			},
			cancellationToken
		);

		// Bounded buffer for lines that have passed deduplication.
		var outputQueue = new BlockingCollection<string>(_boundedCapacity);

		// Create multiple consumer tasks to deduplicate lines.
		var workers = new List<Task>();
		for (var i = 0; i < _workerCount; i++)
		{
			workers.Add(
				Task.Run(
					() =>
					{
						foreach (var line in lines.GetConsumingEnumerable(cancellationToken))
						{
							// Only forward the line if it's the first time we've seen it.
							if (seen.TryAdd(line, 0))
							{
								outputQueue.Add(line, cancellationToken);
							}
						}
					},
					cancellationToken
				)
			);
		}

		// Writer task: writes deduplicated lines to the output file.
		var writerTask = Task.Run(
			() =>
			{
				using var writer = new StreamWriter(_outputPath);
				foreach (var line in outputQueue.GetConsumingEnumerable(cancellationToken))
				{
					writer.WriteLine(line);
				}
			},
			cancellationToken
		);

		// Await all tasks to complete.
		await producer;
		await Task.WhenAll(workers);
		outputQueue.CompleteAdding(); // Signal writer no more output.
		await writerTask;
	}
}
