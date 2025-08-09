namespace Sudoku.Drawing.Ocr;

/// <summary>
/// Define a sudoku recognition service provider.
/// </summary>
public sealed class RecognitionServiceProvider : IDisposable
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
	/// Initializes a default <see cref="RecognitionServiceProvider"/> instance.
	/// </summary>
	public RecognitionServiceProvider()
		=> (_recognizingServiceProvider = new()).InitTesseract($@"{Directory.GetCurrentDirectory()}\tessdata");


	/// <summary>
	/// Indicates whether the OCR tool has already initialized.
	/// </summary>
	public bool IsInitialized => _recognizingServiceProvider.Initialized;


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
	/// <param name="ignoreConflicts">Indicates whether this method ignores any conflicts on sudoku basic rules.</param>
	/// <returns>The grid.</returns>
	/// <exception cref="RecognizerNotInitializedException">Throws when the tool has not initialized yet.</exception>
	public Grid Recognize(Bitmap image, bool ignoreConflicts)
	{
		if (IsInitialized)
		{
			using var gridRecognizer = new GridRecognizer(image);
			return _recognizingServiceProvider.RecognizeDigits(gridRecognizer.Recognize(), ignoreConflicts);
		}

		throw new RecognizerNotInitializedException();
	}
}
