namespace System.IO;

/// <summary>
/// Represents a way to split files.
/// </summary>
public static class FileSplitter
{
	/// <summary>
	/// Split file into multiple parts. Output file names will be <c>&lt;file-name&gt;_&lt;index&gt;.&lt;extension&gt;</c>.
	/// </summary>
	/// <param name="path">Indicates input file path.</param>
	/// <param name="outputDir">Indicates output folder path.</param>
	/// <param name="parts">Indicates the desired number of parts.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A task that can handle asynchronous operation.</returns>
	/// <exception cref="ArgumentException">Throws when <paramref name="parts"/> is an negative number.</exception>
	/// <exception cref="OperationCanceledException">Throws when <paramref name="cancellationToken"/> is requested.</exception>
	public static async Task SplitFileByPartsAsync(string path, string outputDir, int parts, CancellationToken cancellationToken = default)
	{
		ArgumentOutOfRangeException.Assert(parts >= 0);
		if (!Directory.Exists(outputDir))
		{
			Directory.CreateDirectory(outputDir);
		}

		// Totals up the number of lines.
		var totalLines = 0L;
		using (var sr1 = new StreamReader(path))
		{
			while (await sr1.ReadLineAsync(cancellationToken) is not null)
			{
				totalLines++;
			}
		}
		if (totalLines == 0)
		{
			return;
		}

		// Calculates lines for each file.
		var baseLines = totalLines / parts;
		var remainder = totalLines % parts;

		// Write out text.
		using var sr2 = new StreamReader(path);
		var (p, q) = Path.GetFileNameAndExtension(path);
		for (var i = 0; i < parts; i++)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var outputPath = Path.Combine(outputDir, $"{p}_{i + 1}{q}");
			var targetLines = baseLines + (i < remainder ? 1 : 0);
			if (targetLines == 0)
			{
				break;
			}

			await using var sw = new StreamWriter(outputPath);
			for (var j = 0L; j < targetLines; j++)
			{
				var line = await sr2.ReadLineAsync(cancellationToken);
				if (line is null)
				{
					break;
				}
				await sw.WriteLineAsync(line);
			}
		}
	}
}
