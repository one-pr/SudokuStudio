namespace Sudoku.Analytics.Dependency;

/// <summary>
/// Represents an assignment for a group of cells and the specified digit.
/// </summary>
public readonly struct DependencyAssignment :
	IEquatable<DependencyAssignment>,
	IEqualityOperators<DependencyAssignment, DependencyAssignment, bool>
{
	/// <summary>
	/// Indicates placeholder cell.
	/// </summary>
	private const int PlaceholderCell = 81;


	/// <summary>
	/// Indicates the backing mask.
	/// </summary>
	private readonly int _mask;


	/// <summary>
	/// Initializes an <see cref="DependencyAssignment"/> instance via the specified candidate.
	/// </summary>
	/// <param name="candidate">The candidate.</param>
	public DependencyAssignment(Candidate candidate)
		=> _mask = PlaceholderCell << 18 | PlaceholderCell << 11 | candidate / 9 << 4 | candidate % 9;

	/// <summary>
	/// Initializes an <see cref="DependencyAssignment"/> instance via the specified candidate.
	/// </summary>
	/// <param name="digit">The digit.</param>
	/// <param name="cells">The cells.</param>
	public DependencyAssignment(Digit digit, in CellMap cells)
		=> _mask = cells switch
		{
			[var c1, var c2, var c3] => c3 << 18 | c2 << 11 | c1 << 4 | digit,
			[var c1, var c2] => PlaceholderCell << 18 | c2 << 11 | c1 << 4 | digit,
			[var c1] => PlaceholderCell << 18 | PlaceholderCell << 11 | c1 << 4 | digit,
			_ => throw new InvalidOperationException("The maximum length of cells must be 3")
		};


	/// <summary>
	/// Indicates whether the assignment instance is for grouped set rule.
	/// </summary>
	public bool IsGrouped => (_mask >> 11 & (127 << 7 | 127)) != (PlaceholderCell << 7 | PlaceholderCell);

	/// <summary>
	/// Indicates the digit used.
	/// </summary>
	public Digit Digit => _mask & 15;

	/// <summary>
	/// Indicates the cells used.
	/// </summary>
	public CellMap Cells
	{
		get
		{
			var c3 = _mask >> 18 & 127;
			var c2 = _mask >> 11 & 127;
			var c1 = _mask >> 4 & 127;
			return (c3, c2, c1) switch
			{
				(PlaceholderCell, PlaceholderCell, _) => c1.AsCellMap(),
				(PlaceholderCell, _, _) => c1.AsCellMap() + c2,
				_ => c1.AsCellMap() + c2 + c3
			};
		}
	}


	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
	public void Deconstruct(out Digit digit, out CellMap cells) => (digit, cells) = (Digit, Cells);

	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] object? obj) => obj is DependencyAssignment comparer && Equals(comparer);

	/// <inheritdoc/>
	public bool Equals(DependencyAssignment other) => _mask == other._mask;

	/// <inheritdoc cref="object.GetHashCode"/>
	public override int GetHashCode() => _mask;

	/// <inheritdoc cref="ToCandidateFormatString(bool, CultureInfo?)"/>
	public string ToCandidateFormatString(bool enableConsoleColors) => ToCandidateFormatString(enableConsoleColors, null);

	/// <summary>
	/// Returns a string instance that displays like a candidate format.
	/// </summary>
	/// <param name="enableConsoleColors">
	/// Indicates whether the output text will include control characters like <c>\e</c>,
	/// in order to display colors in console output stream.
	/// </param>
	/// <param name="culture">The culture.</param>
	/// <returns>The string representation.</returns>
	public string ToCandidateFormatString(bool enableConsoleColors, CultureInfo? culture)
	{
		var converter = CoordinateConverter.GetInstance(culture);
		var candidates = Cells * Digit;
		return IsGrouped && enableConsoleColors
			? $"\e[38;2;255;255;0m{converter.CandidateConverter(candidates)}\e[0m"
			: converter.CandidateConverter(candidates);
	}

	/// <inheritdoc cref="object.ToString"/>
	public override string ToString()
		=> $$"""{{nameof(DependencyAssignment)}} { {{nameof(Digit)}} = {{Digit + 1}}, {{nameof(Cells)}} = {{Cells}} }""";


	/// <inheritdoc/>
	public static bool operator ==(DependencyAssignment left, DependencyAssignment right) => left.Equals(right);

	/// <inheritdoc/>
	public static bool operator !=(DependencyAssignment left, DependencyAssignment right) => !(left == right);
}
