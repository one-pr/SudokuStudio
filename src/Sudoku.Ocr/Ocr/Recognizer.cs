namespace Sudoku.Ocr;

/// <summary>
/// Define a sudoku recognition service provider.
/// </summary>
public sealed class Recognizer : IDisposable
{
	/// <summary>
	/// Indicates the internal recognition service provider.
	/// </summary>
	private readonly InternalServiceProvider _recognizingServiceProvider;

	/// <summary>
	/// Indicates whether the object is disposed.
	/// </summary>
	private bool _isDisposed;


	/// <summary>
	/// Initializes a default <see cref="Recognizer"/> instance.
	/// </summary>
	public Recognizer()
	{
		_recognizingServiceProvider = new();
		_recognizingServiceProvider.InitTesseract($@"{Directory.GetCurrentDirectory()}\tessdata");
	}


	/// <inheritdoc/>
	public void Dispose()
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);

		_recognizingServiceProvider.Dispose();

		_isDisposed = true;
	}

	/// <summary>
	/// Recognize the image.
	/// </summary>
	/// <param name="image">The image.</param>
	/// <param name="result">The result grid recognized.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	public bool TryRecognize(Bitmap image, out Grid result)
	{
		try
		{
			result = Recognize(image);
			return true;
		}
		catch (RecognizerNotInitializedException)
		{
		}
		catch (FailedToFillValueException)
		{
		}

		result = Grid.Undefined;
		return false;
	}

	/// <summary>
	/// Recognize the image.
	/// </summary>
	/// <param name="image">The image.</param>
	/// <returns>The grid.</returns>
	/// <exception cref="RecognizerNotInitializedException">Throws when the tool has not initialized yet.</exception>
	public Grid Recognize(Bitmap image)
	{
		if (!_recognizingServiceProvider.Initialized)
		{
			throw new RecognizerNotInitializedException();
		}

		using var gridRecognizer = new GridRecognizer(image);
		return _recognizingServiceProvider.Recognize(gridRecognizer.Recognize()).FixedGrid;
	}

	/// <summary>
	/// Performs asynchronous operation to recognize image.
	/// </summary>
	/// <param name="image">The image.</param>
	/// <returns>The grid.</returns>
	public async Task<Grid> RecognizeAsync(Bitmap image) => await Task.Run(() => Recognize(image));
}
