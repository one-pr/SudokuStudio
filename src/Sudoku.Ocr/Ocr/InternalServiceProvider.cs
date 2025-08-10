namespace Sudoku.Ocr;

/// <summary>
/// Define a recognizer.
/// </summary>
internal sealed class InternalServiceProvider : IDisposable
{
	/// <summary>
	/// Indicates the min value of threshold.
	/// </summary>
	private const int ThresholdMinValue = 120;

	/// <summary>
	/// Indicates the max value of threshold.
	/// </summary>
	private const int ThresholdMaxValue = 255;


	/// <summary>
	/// Indicates whether the object is disposed.
	/// </summary>
	private bool _isDisposed;

	/// <summary>
	/// The internal <see cref="Tesseract"/> instance.
	/// </summary>
	private Tesseract? _ocr;


	/// <summary>
	/// Indicates whether the current recognizer has already initialized.
	/// </summary>
	public bool Initialized => _ocr is not null;


	/// <inheritdoc/>
	public void Dispose()
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);

		_ocr?.Dispose();

		_isDisposed = true;
	}

	/// <summary>
	/// Recognizes digits.
	/// </summary>
	/// <param name="field">
	/// <para>The field indicating the whole outline of a grid.</para>
	/// <para>
	/// It doesn't mean C# concept "field"; instead, this parameter represents the range of border of the grid
	/// to be recognized encapsulated by API.
	/// </para>
	/// </param>
	/// <returns>The grid.</returns>
	/// <exception cref="FailedToFillValueException">Throws when any conflicts are found in target grid.</exception>
	public Grid Recognize(Image<Bgr, byte> field)
	{
		var result = Grid.Empty;
		var cellWidth = field.Width / 9;
		var offset = cellWidth / 6;
		for (var column = 0; column < 9; column++)
		{
			for (var row = 0; row < 9; row++)
			{
				// Recognize digit from cell.
				var recognizedResult = RecognizeCellNumber(
					field.GetSubRect(
						new(
							offset + cellWidth * column,
							offset + cellWidth * row,
							cellWidth - offset * 2,
							cellWidth - offset * 2
						)
					)
				);
				if (recognizedResult == -1)
				{
					continue;
				}

				// Check validity on value.
				if ((row * 9 + column, recognizedResult - 1) is var (cell, digit) && !result.GetExistence(cell, digit))
				{
					throw new FailedToFillValueException(cell, digit);
				}

				// Set value to grid.
				result.SetDigit(cell, digit);
			}
		}

		// The result will be transposed.
		return result;
	}

	/// <summary>
	/// Recognize the number of a cell.
	/// </summary>
	/// <param name="cellImg">The image of a cell.</param>
	/// <returns>
	/// The result value (must be between 1 and 9). If the recognition is failed,
	/// the value will be <c>0</c>.
	/// </returns>
	/// <exception cref="ArgumentNullException">Throws when the inner tool isn't been initialized.</exception>
	/// <exception cref="TesseractException">Throws when the OCR engine error.</exception>
	private Digit RecognizeCellNumber(Image<Bgr, byte> cellImg)
	{
		ArgumentNullException.ThrowIfNull(_ocr);

		// Convert the image to gray-scale and filter out the noisy points.
		var imgGray = new Mat();
		CvInvoke.CvtColor(cellImg, imgGray, ColorConversion.Bgr2Gray);

		var imgThresholds = new Mat();
		CvInvoke.Threshold(imgGray, imgThresholds, ThresholdMinValue, ThresholdMaxValue, ThresholdType.Binary);

		_ocr.SetImage(imgThresholds);
		if (_ocr.Recognize() != 0)
		{
			throw new TesseractException(SR.ExceptionMessage("CannotRecognizeCellImage"));
		}

		var characters = _ocr.GetWords();
		var numberText = string.Empty;
		foreach (var c in characters)
		{
			if (c.Text is var t and not " ")
			{
				numberText += t;
			}
		}
		return numberText.Length > 1 ? -1 : Digit.TryParse(numberText, out var resultValue) ? resultValue : -1;
	}

	/// <summary>
	/// Initializes <see cref="Tesseract"/> instance.
	/// </summary>
	/// <param name="dir">The directory.</param>
	/// <param name="lang">The language. The default value is <c>"eng"</c>.</param>
	/// <returns>The <see cref="bool"/> result.</returns>
	/// <exception cref="FileNotFoundException">Throws when the file doesn't found.</exception>
	[MemberNotNullWhen(true, nameof(_ocr))]
	public bool InitTesseract(string dir, string lang = "eng")
	{
		try
		{
			var filePath = $@"{dir}\{lang}.traineddata";
			if (!File.Exists(filePath))
			{
				throw new FileNotFoundException(SR.ExceptionMessage("MissingTrainedDataFile"), filePath);
			}

			_ocr = new(dir, lang, OcrEngineMode.TesseractOnly, "123456789");
			return true;
		}
		catch
		{
			return false;
		}
	}
}
