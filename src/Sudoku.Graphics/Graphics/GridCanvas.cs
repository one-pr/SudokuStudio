namespace Sudoku.Graphics;

/// <summary>
/// Represents a canvas that allows you drawing sudoku-related items onto it.
/// </summary>
/// <param name="grid"><inheritdoc cref="Grid" path="/summary"/></param>
/// <param name="mapper"><inheritdoc cref="Mapper" path="/summary"/></param>
public sealed class GridCanvas(in Grid grid, PointMapper mapper) : IGridCanvas
{
	/// <summary>
	/// The backing field of property <see cref="Grid"/>.
	/// </summary>
	/// <seealso cref="Grid"/>
	private readonly Grid _grid = grid;

	/// <summary>
	/// Indicates the backing surface object.
	/// </summary>
	private readonly SKSurface _surface = SKSurface.Create(new SKImageInfo(mapper.Size, mapper.Size));

	/// <summary>
	/// Indicates whether the object has already been disposed.
	/// </summary>
	private bool _isDisposed;


	/// <inheritdoc/>
	public ref readonly Grid Grid => ref _grid;

	/// <inheritdoc/>
	public PointMapper Mapper { get; } = mapper;

	/// <inheritdoc/>
	public SKCanvas Canvas => _surface.Canvas;

	/// <inheritdoc/>
	bool IGridCanvas.IsDisposed => _isDisposed;


	/// <inheritdoc/>
	public void Dispose()
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);

		_surface.Dispose();
		_isDisposed = true;
	}

	/// <inheritdoc/>
	public void DrawBackground(ImageDrawingOptions? options = null)
		=> Canvas.Clear((options ?? new()).BackgroundColor);

	/// <inheritdoc/>
	public void DrawGridLine(ImageDrawingOptions? options = null)
	{
		options ??= new();

		using var blockLinePaint = new SKPaint
		{
			Style = SKPaintStyle.Stroke,
			StrokeWidth = options.BlockLineStrokeThickness,
			Color = options.BlockLineStrokeColor,
			IsAntialias = true,
			PathEffect = options.BlockLineDashSequence.IsEmpty ? null : options.BlockLineDashSequence
		};
		using var cellLinePaint = new SKPaint
		{
			Style = SKPaintStyle.Stroke,
			StrokeWidth = options.GridLineStrokeThickness,
			Color = options.GridLineStrokeColor,
			IsAntialias = true,
			PathEffect = options.GridLineDashSequence.IsEmpty ? null : options.GridLineDashSequence
		};
		using var candidateAuxiliaryLinePaint = options.DrawCandidateAuxiliaryLines
			? new SKPaint
			{
				Style = SKPaintStyle.Stroke,
				StrokeWidth = options.CandidateAuxiliaryLineStrokeThickness,
				Color = options.CandidateAuxiliaryLineStrokeColor,
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

			Canvas.DrawLine(Mapper.GetCandidateAnchor(i, 0), Mapper.GetCandidateAnchor(i, 27), paintChosen);
			Canvas.DrawLine(Mapper.GetCandidateAnchor(0, i), Mapper.GetCandidateAnchor(27, i), paintChosen);
		}
	}

	/// <inheritdoc/>
	public void Export(string path, ImageExportingOptions? options = null)
	{
		options ??= new();

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
	public async Task ExportAsync(string path, ImageExportingOptions? options = null, CancellationToken cancellationToken = default)
	{
		options ??= new();

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
