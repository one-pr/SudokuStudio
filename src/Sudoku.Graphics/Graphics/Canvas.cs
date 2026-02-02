namespace Sudoku.Graphics;

/// <summary>
/// Represents a canvas that allows you drawing sudoku-related items onto it.
/// </summary>
public sealed class Canvas : ICanvas
{
	/// <summary>
	/// Indicates the backing surface object.
	/// </summary>
	private readonly SKSurface _surface;

	/// <summary>
	/// Indicates whether the object has already been disposed.
	/// </summary>
	private bool _isDisposed;


	/// <summary>
	/// Initializes a <see cref="Canvas"/> instance via the specified picture size and margin (to the inner sudoku grid).
	/// </summary>
	/// <param name="size">The picture size.</param>
	/// <param name="margin">The margin to the inner sudoku grid.</param>
	public Canvas(int size, float margin) : this(new(size, margin))
	{
	}

	/// <summary>
	/// Initializes a <see cref="Canvas"/> instance via the specified point mapper instance.
	/// </summary>
	/// <param name="mapper">The mapper instance.</param>
	private Canvas(PointMapper mapper)
	{
		_surface = SKSurface.Create(new SKImageInfo(mapper.Size, mapper.Size));
		Mapper = mapper;
	}


	/// <inheritdoc/>
	public PointMapper Mapper { get; }

	/// <inheritdoc/>
	bool ICanvas.IsDisposed => _isDisposed;

	/// <inheritdoc/>
	SKCanvas ICanvas.BackingCanvas => BackingCanvas;

	/// <inheritdoc cref="ICanvas.BackingCanvas"/>
	private SKCanvas BackingCanvas => _surface.Canvas;


	/// <inheritdoc/>
	public void Dispose()
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);

		_surface.Dispose();
		_isDisposed = true;
	}

	/// <inheritdoc/>
	public void DrawBackground(CanvasDrawingOptions? options = null)
		=> BackingCanvas.Clear((options ?? CanvasDrawingOptions.Default).BackgroundColor.AsSKColor());

	/// <inheritdoc/>
	public void DrawGrid(in Grid grid, CanvasDrawingOptions? options = null)
	{
		options ??= CanvasDrawingOptions.Default;

		using var givenDigitsTypeface = SKTypeface.FromFamilyName(
			options.GivenDigitsFontName,
			options.GivenDigitsFontWeight,
			options.GivenDigitsFontWidth,
			options.GivenDigitsFontSlant
		);
		using var modifiableDigitsTypeface = SKTypeface.FromFamilyName(
			options.ModifiableDigitsFontName,
			options.ModifiableDigitsFontWeight,
			options.ModifiableDigitsFontWidth,
			options.ModifiableDigitsFontSlant
		);
		using var candidatesTypeface = SKTypeface.FromFamilyName(
			options.CandidatesFontName,
			options.CandidatesFontWeight,
			options.CandidatesFontWidth,
			options.CandidatesFontSlant
		);
		var factGivenDigitsSize = options.GivenDigitsFontSizeRatio.Measure(Mapper.CellSize);
		var factModifiableDigitsSize = options.ModifiableDigitsFontSizeRatio.Measure(Mapper.CellSize);
		var factCandidatesSize = options.CandidatesFontSizeRatio.Measure(Mapper.CellSize);
		using var givenDigitsFont = new SKFont(givenDigitsTypeface, factGivenDigitsSize) { Subpixel = true };
		using var givenDigitsPaint = new SKPaint { Color = options.GivenDigitsColor.AsSKColor() };
		using var modifiableDigitsFont = new SKFont(modifiableDigitsTypeface, factModifiableDigitsSize) { Subpixel = true };
		using var modifiableDigitsPaint = new SKPaint { Color = options.ModifiableDigitsColor.AsSKColor() };
		using var candidatesFont = new SKFont(candidatesTypeface, factCandidatesSize) { Subpixel = true };
		using var candidatesPaint = new SKPaint { Color = options.CandidatesColor.AsSKColor() };

		for (var cell = 0; cell < 81; cell++)
		{
			switch (grid.GetState(cell))
			{
				case CellState.Empty:
				{
					foreach (var digit in grid.GetCandidates(cell))
					{
						var text = (digit + 1).ToString();
						var offset = candidatesFont.MeasureText(text, candidatesPaint);
						BackingCanvas.DrawText(
							text,
							Mapper.GetCandidateCenterPoint(cell * 9 + digit)
								+ new SKPoint(0, offset / 2) // Offset adjustment
								+ new SKPoint(0, Mapper.CandidateSize / 12), // Manual adjustment
							SKTextAlign.Center,
							candidatesFont,
							candidatesPaint
						);
					}
					break;
				}
				case var state and (CellState.Modifiable or CellState.Given):
				{
					var text = (grid.GetDigit(cell) + 1).ToString();
					var targetFont = state == CellState.Given ? givenDigitsFont : modifiableDigitsFont;
					var targetPaint = state == CellState.Given ? givenDigitsPaint : modifiableDigitsPaint;
					var offset = targetFont.MeasureText(text, targetPaint);
					BackingCanvas.DrawText(
						text,
						Mapper.GetCellCenterPoint(cell)
							+ new SKPoint(0, offset / 2) // Offset adjustment
							+ new SKPoint(0, Mapper.CellSize / 12), // Manual adjustment
						SKTextAlign.Center,
						targetFont,
						targetPaint
					);
					break;
				}
			}
		}
	}

	/// <inheritdoc/>
	public void DrawGridLine(CanvasDrawingOptions? options = null)
	{
		options ??= CanvasDrawingOptions.Default;

		using var blockLinePaint = new SKPaint
		{
			Style = SKPaintStyle.Stroke,
			StrokeWidth = options.BlockLineStrokeThicknessRatio.Measure(Mapper.CandidateSize),
			Color = options.BlockLineStrokeColor.AsSKColor(),
			IsAntialias = true,
			PathEffect = options.BlockLineDashSequence.IsEmpty ? null : options.BlockLineDashSequence
		};
		using var cellLinePaint = new SKPaint
		{
			Style = SKPaintStyle.Stroke,
			StrokeWidth = options.GridLineStrokeThicknessRatio.Measure(Mapper.CandidateSize),
			Color = options.GridLineStrokeColor.AsSKColor(),
			IsAntialias = true,
			PathEffect = options.GridLineDashSequence.IsEmpty ? null : options.GridLineDashSequence
		};
		using var candidateAuxiliaryLinePaint = options.DrawCandidateAuxiliaryLines
			? new SKPaint
			{
				Style = SKPaintStyle.Stroke,
				StrokeWidth = options.CandidateAuxiliaryLineStrokeThicknessRatio.Measure(Mapper.CandidateSize),
				Color = options.CandidateAuxiliaryLineStrokeColor.AsSKColor(),
				IsAntialias = true,
				PathEffect = options.CandidateAuxiliaryLineDashSequence.IsEmpty ? null : options.CandidateAuxiliaryLineDashSequence
			}
			: null;
		for (var i = 0; i <= 27; i++)
		{
			if ((i % 9 == 0 ? blockLinePaint : i % 3 == 0 ? cellLinePaint : candidateAuxiliaryLinePaint) is not { } paintChosen)
			{
				continue;
			}

			BackingCanvas.DrawLine(Mapper.GetCandidateTopLeftPoint(i, 0), Mapper.GetCandidateTopLeftPoint(i, 27), paintChosen);
			BackingCanvas.DrawLine(Mapper.GetCandidateTopLeftPoint(0, i), Mapper.GetCandidateTopLeftPoint(27, i), paintChosen);
		}
	}

	/// <inheritdoc/>
	public void Export(string path, CanvasExportingOptions? options = null)
	{
		options ??= CanvasExportingOptions.Default;

		var extension = Path.GetExtension(path);
		using var image = _surface.Snapshot();
		using var data = image.Encode(GetFormatFromExtension(extension), options.Quality);
		using var stream = new MemoryStream(data.ToArray());
		using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
		stream.CopyTo(fileStream);
	}

	/// <summary>
	/// Export image into target file, asynchronously.
	/// </summary>
	/// <param name="path">The file path.</param>
	/// <param name="options">The exporting options.</param>
	/// <param name="cancellationToken">The cancellation token that allows you cancelling the operation if necessary.</param>
	/// <returns>A <see cref="Task"/> object that allows you visiting states on asynchronous task.</returns>
	/// <exception cref="NotSupportedException">Throws when the target file extension is not supported.</exception>
	/// <exception cref="OperationCanceledException">Throws when task is canceled.</exception>
	public async Task ExportAsync(string path, CanvasExportingOptions? options = null, CancellationToken cancellationToken = default)
	{
		options ??= CanvasExportingOptions.Default;

		var extension = Path.GetExtension(path);
		using var image = _surface.Snapshot();
		using var data = image.Encode(GetFormatFromExtension(extension), options.Quality);
		await using var stream = new MemoryStream(data.ToArray());
		await using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
		await stream.CopyToAsync(fileStream, cancellationToken);
	}

	/// <summary>
	/// Returns <see cref="SKEncodedImageFormat"/> from extension string.
	/// </summary>
	/// <param name="extension">The file extesnsion.</param>
	/// <returns>The target format.</returns>
	/// <exception cref="NotSupportedException">Throws when the target format is not supported.</exception>
	private SKEncodedImageFormat GetFormatFromExtension(string extension)
		=> extension switch
		{
			".jpg" => SKEncodedImageFormat.Jpeg,
			".png" => SKEncodedImageFormat.Png,
			".gif" => SKEncodedImageFormat.Gif,
			".bmp" => SKEncodedImageFormat.Bmp,
			".webp" => SKEncodedImageFormat.Webp,
			_ => throw new NotSupportedException()
		};
}
