namespace Sudoku.Concepts.Supersymmetry;

/// <summary>
/// Represents a supersymmetric space. This type can also be used as representation for truth or link concept
/// defined in another project called <see href="https://sudoku.allanbarker.com/index.html">XSudo</see>.
/// </summary>
/// <param name="mask"><inheritdoc cref="_mask" path="/summary"/></param>
public readonly struct Space(Mask mask) :
	IComparable<Space>,
	IComparisonOperators<Space, Space, bool>,
	IEquatable<Space>,
	IEqualityOperators<Space, Space, bool>
{
	/// <summary>
	/// Represents invalid space.
	/// </summary>
	public static readonly Space InvalidSpace = new(-1);


	/// <summary>
	/// Indicates the backing mask.
	/// </summary>
	private readonly Mask _mask = mask;


	/// <summary>
	/// Indicates whether the space is house-related.
	/// </summary>
	public bool IsHouseRelated => Type is SpaceType.RowNumber or SpaceType.ColumnNumber or SpaceType.BlockNumber;

	/// <summary>
	/// Indicates whether the space is cell-related.
	/// </summary>
	public bool IsCellRelated => Type == SpaceType.RowColumn;

	/// <summary>
	/// Indicates the space type.
	/// </summary>
	public SpaceType Type => (SpaceType)(_mask >> 8 & 3);

	/// <summary>
	/// Indicates the row value,
	/// or -1 if <see cref="Type"/> is not <see cref="SpaceType.RowColumn"/> or <see cref="SpaceType.RowNumber"/>.
	/// </summary>
	public RowIndex Row => Type switch { SpaceType.RowColumn => Secondary, SpaceType.RowNumber => Primary, _ => -1 };

	/// <summary>
	/// Indicates the column value,
	/// or -1 if <see cref="Type"/> is not <see cref="SpaceType.RowColumn"/> or <see cref="SpaceType.ColumnNumber"/>.
	/// </summary>
	public ColumnIndex Column => Type switch { SpaceType.ColumnNumber => Primary, _ => -1 };

	/// <summary>
	/// Indicates the block value, or -1 if <see cref="Type"/> is not <see cref="SpaceType.BlockNumber"/>.
	/// </summary>
	public BlockIndex Block => Type switch { SpaceType.BlockNumber => Primary, _ => -1 };

	/// <summary>
	/// Indicates the target cell, or -1 if <see cref="Type"/> is not <see cref="SpaceType.RowColumn"/>.
	/// </summary>
	public Cell Cell => Type switch { SpaceType.RowColumn => Secondary * 9 + Primary, _ => -1 };

	/// <summary>
	/// Indicates the target digit, or -1 if <see cref="Type"/> is <see cref="SpaceType.RowColumn"/>.
	/// </summary>
	public Digit Digit => Type switch { not SpaceType.RowColumn => Secondary, _ => -1 };

	/// <summary>
	/// Indicates the target house, or -1 if <see cref="Type"/> is <see cref="SpaceType.RowColumn"/>.
	/// </summary>
	public House House
		=> Type switch
		{
			SpaceType.RowNumber => Row + 9,
			SpaceType.ColumnNumber => Column + 18,
			SpaceType.BlockNumber => Block,
			_ => -1
		};

	/// <summary>
	/// Indicates identifier for house-digit pair. If eiither <see cref="House"/> or <see cref="Digit"/> returns -1,
	/// the return value will be <see langword="default"/>(<see cref="HouseDigitIdentifier"/>).
	/// </summary>
	public HouseDigitIdentifier HouseDigit => House == -1 || Digit == -1 ? default : new(House, Digit);

	/// <summary>
	/// Returns a list of candidates that are in the current set.
	/// </summary>
	/// <returns>The candidates.</returns>
	public CandidateMap Range
	{
		get
		{
			switch (this)
			{
				case { Cell: var cell and not -1 }:
				{
					var result = CandidateMap.Empty;
					for (var digit = 0; digit < 9; digit++)
					{
						result += cell * 9 + digit;
					}
					return result;
				}
				case { HouseDigit: var (house, digit) }:
				{
					var result = CandidateMap.Empty;
					foreach (var cell in HousesMap[house])
					{
						result += cell * 9 + digit;
					}
					return result;
				}
			}
		}
	}

	/// <summary>
	/// Indicates the represented letter.
	/// </summary>
	private char Letter
		=> Type switch
		{
			SpaceType.RowColumn => 'n',
			SpaceType.RowNumber => 'r',
			SpaceType.ColumnNumber => 'c',
			SpaceType.BlockNumber => 'b'
		};

	/// <summary>
	/// Indicates the primary value, written after letter.
	/// </summary>
	private Digit Primary => _mask & 15;

	/// <summary>
	/// Indicates the secondary value, written before letter.
	/// </summary>
	private Digit Secondary => _mask >> 4 & 15;


	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
	public void Deconstruct(out int primary, out int secondary) => (primary, secondary) = (Primary, Secondary);

	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
	public void Deconstruct(out SpaceType type, out int primary, out int secondary) => (type, (primary, secondary)) = (Type, this);

	/// <inheritdoc/>
	public int CompareTo(Space other)
		=> Type.CompareTo(other.Type) is var r1 and not 0
			? r1
			: IsCellRelated
				? Secondary.CompareTo(other.Secondary) is var r2 ? r2 : Primary.CompareTo(other.Primary)
				: Primary.CompareTo(other.Primary) is var r3 ? r3 : Secondary.CompareTo(other.Secondary);

	/// <summary>
	/// Determine whether the specified assignment is inside the range of candidates of the current space.
	/// </summary>
	/// <param name="candidate">The candidate.</param>
	/// <returns>A <see cref="bool"/> result.</returns>
	public bool Contains(Candidate candidate)
		=> this switch
		{
			{ Cell: var cell and not -1 } => candidate / 9 == cell,
			{ HouseDigit: var (house, digit) } => candidate % 9 == digit && HousesMap[house].Contains(candidate / 9)
		};

	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] object? obj) => obj is Space comparer && Equals(comparer);

	/// <inheritdoc/>
	public bool Equals(Space other) => _mask == other._mask;

	/// <inheritdoc/>
	public override int GetHashCode() => _mask;

	/// <inheritdoc/>
	public override string ToString() => _mask == -1 ? "<invalid>" : $"{Secondary + 1}{Letter}{Primary + 1}";

	/// <summary>
	/// Try to find all possible candidates in the current set.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <returns>The candidates.</returns>
	public CandidateMap GetAvailableRange(in Grid grid)
	{
		switch (this)
		{
			case { Cell: var cell and not -1 }:
			{
				var result = CandidateMap.Empty;
				foreach (var digit in grid.GetCandidates(cell))
				{
					result += cell * 9 + digit;
				}
				return result;
			}
			case { HouseDigit: var (house, digit) }:
			{
				var result = CandidateMap.Empty;
				foreach (var cell in HousesMap[house])
				{
					if (grid.Exists(cell, digit) is true)
					{
						result += cell * 9 + digit;
					}
				}
				return result;
			}
		}
	}


	/// <summary>
	/// Try to parse the specified string into target instance.
	/// </summary>
	/// <param name="s">The string.</param>
	/// <param name="result">The result.</param>
	/// <returns>A <see cref="bool"/> result.</returns>
	public static bool TryParse([NotNullWhen(true)] string? s, out Space result) => TryParse(s.AsSpan(), out result);

	/// <inheritdoc cref="TryParse(string?, out Space)"/>
	public static bool TryParse(ReadOnlySpan<char> s, out Space result)
	{
		try
		{
			if (s.IsEmpty)
			{
				goto ReturnFalse;
			}
			result = Parse(s);
			return true;
		}
		catch (FormatException)
		{
		}

	ReturnFalse:
		result = default;
		return false;
	}

	/// <summary>
	/// Parses the specified string into target instance.
	/// </summary>
	/// <param name="s">The string.</param>
	/// <returns>The result instance.</returns>
	public static Space Parse(string s) => Parse(s.Span);

	/// <inheritdoc cref="Parse(string)"/>
	public static Space Parse(ReadOnlySpan<char> s)
		=> s switch
		{
			[
				var secondaryChar and >= '1' and <= '9',
				var typeChar and ('R' or 'r' or 'C' or 'c' or 'B' or 'b' or 'N' or 'n'),
				var primaryChar and >= '1' and <= '9'
			] => typeChar switch
			{
				'N' or 'n' => RowColumn(secondaryChar - '1', primaryChar - '1'),
				'B' or 'b' => BlockDigit(primaryChar - '1', secondaryChar - '1'),
				'R' or 'r' => RowDigit(primaryChar - '1', secondaryChar - '1'),
				_ => ColumnDigit(primaryChar - '1', secondaryChar - '1')
			},
			_ => throw new FormatException()
		};

	/// <summary>
	/// Creates a <see cref="Space"/> for row-digit space.
	/// </summary>
	/// <param name="row">Indicates the row index. Range [0, 9).</param>
	/// <param name="digit">Indicates the digit. Range [0, 9).</param>
	/// <returns>The <see cref="Space"/> instance created.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Throws when argument is greater than 9.</exception>
	public static Space RowDigit(RowIndex row, Digit digit)
	{
		ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(row, 9);
		ArgumentOutOfRangeException.ThrowIfLessThan(row, 0);
		ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(digit, 9);
		ArgumentOutOfRangeException.ThrowIfLessThan(digit, 0);
		return new((Mask)(row | digit << 4 | (int)SpaceType.RowNumber << 8));
	}

	/// <summary>
	/// Creates a <see cref="Space"/> for column-digit space.
	/// </summary>
	/// <param name="column">Indicates the column index. Range [0, 9).</param>
	/// <param name="digit">Indicates the digit. Range [0, 9).</param>
	/// <returns>The <see cref="Space"/> instance created.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Throws when argument is greater than 9.</exception>
	public static Space ColumnDigit(ColumnIndex column, Digit digit)
	{
		ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(column, 9);
		ArgumentOutOfRangeException.ThrowIfLessThan(column, 0);
		ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(digit, 9);
		ArgumentOutOfRangeException.ThrowIfLessThan(digit, 0);
		return new((Mask)(column | digit << 4 | (int)SpaceType.ColumnNumber << 8));
	}

	/// <summary>
	/// Creates a <see cref="Space"/> for block-digit space.
	/// </summary>
	/// <param name="block">Indicates the block index. Range [0, 9).</param>
	/// <param name="digit">Indicates the digit. Range [0, 9).</param>
	/// <returns>The <see cref="Space"/> instance created.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Throws when argument is greater than 9.</exception>
	public static Space BlockDigit(BlockIndex block, Digit digit)
	{
		ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(block, 9);
		ArgumentOutOfRangeException.ThrowIfLessThan(block, 0);
		ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(digit, 9);
		ArgumentOutOfRangeException.ThrowIfLessThan(digit, 0);
		return new((Mask)(block | digit << 4 | (int)SpaceType.BlockNumber << 8));
	}

	/// <summary>
	/// Creates a <see cref="Space"/> for row-column space.
	/// </summary>
	/// <param name="row">Indicates the row index. Range [0, 9).</param>
	/// <param name="column">Indicates the column index. Range [0, 9).</param>
	/// <returns>The <see cref="Space"/> instance created.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Throws when argument is greater than 9.</exception>
	public static Space RowColumn(RowIndex row, ColumnIndex column)
	{
		ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(row, 9);
		ArgumentOutOfRangeException.ThrowIfLessThan(row, 0);
		ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(column, 9);
		ArgumentOutOfRangeException.ThrowIfLessThan(column, 0);
		return new((Mask)(column | row << 4 | (int)SpaceType.RowColumn << 8));
	}


	/// <inheritdoc/>
	public static bool operator ==(Space left, Space right) => left.Equals(right);

	/// <inheritdoc/>
	public static bool operator !=(Space left, Space right) => !(left == right);

	/// <inheritdoc/>
	public static bool operator >(Space left, Space right) => left.CompareTo(right) > 0;

	/// <inheritdoc/>
	public static bool operator <(Space left, Space right) => left.CompareTo(right) < 0;

	/// <inheritdoc/>
	public static bool operator >=(Space left, Space right) => left.CompareTo(right) >= 0;

	/// <inheritdoc/>
	public static bool operator <=(Space left, Space right) => left.CompareTo(right) <= 0;
}
