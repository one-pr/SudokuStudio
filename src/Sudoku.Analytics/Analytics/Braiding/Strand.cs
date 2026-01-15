namespace Sudoku.Analytics.Braiding;

/// <summary>
/// Represents a strand.
/// A <b>Strand</b> is a distribution pattern for a single digit in 3 of the 9 intersections aligned in a chute.
/// There are 6 different strands in each chute.
/// </summary>
/// <param name="ChuteIndex">Indicates chute index (0..6).</param>
/// <param name="SequenceIndex">Indicates sequence index (0..3).</param>
/// <param name="Type">Indicates type of rotation.</param>
/// <remarks>For more information, please visit <see href="http://sudopedia.enjoysudoku.com/Strand.html">this link</see>.</remarks>
public readonly record struct Strand(int ChuteIndex, Digit SequenceIndex, StrandType Type) :
	IComparable<Strand>,
	IComparisonOperators<Strand, Strand, bool>,
	IEqualityOperators<Strand, Strand, bool>
{
	/// <inheritdoc cref="Strands"/>
	private static readonly Strand[] StrandsBackingField;


	/// <summary>
	/// Represents all possible strand instances.
	/// </summary>
	public static ReadOnlySpan<Strand> Strands => StrandsBackingField;


	/// <include file='../../global-doc-comments.xml' path='g/static-constructor' />
	static Strand()
	{
		StrandsBackingField = new Strand[Chute.MaxChuteIndex * 3 * 2];
		for (var (chuteIndex, i) = (0, 0); chuteIndex < Chute.MaxChuteIndex; chuteIndex++)
		{
			for (var sequenceIndex = 0; sequenceIndex < 3; sequenceIndex++)
			{
				foreach (var type in (StrandType.Downside, StrandType.Upside))
				{
					StrandsBackingField[i++] = new(chuteIndex, sequenceIndex, type);
				}
			}
		}
	}


	/// <summary>
	/// Indicates global sequence index (0..9).
	/// </summary>
	public Digit GlobalSequenceIndex => ChuteIndex % 3 * 3 + SequenceIndex;

	/// <summary>
	/// Indicates global index (0..18).
	/// </summary>
	private int GlobalIndex => (BraidAnalysis.ProjectGlobalIndex(ChuteIndex, SequenceIndex) << 1) + (int)(Type - 1);


	/// <inheritdoc/>
	public bool Equals(Strand other) => GlobalIndex == other.GlobalIndex;

	/// <inheritdoc cref="object.GetHashCode"/>
	public override int GetHashCode() => GlobalIndex;

	/// <inheritdoc/>
	public int CompareTo(Strand other) => GlobalIndex.CompareTo(other.GlobalIndex);

	/// <inheritdoc cref="object.ToString"/>
	public override string ToString()
		// 'T' -> Tower (Mega-column), 'F' -> Floor (Mega-row)
		=> $"{(ChuteIndex > 3 ? 'T' : 'F')}{GlobalSequenceIndex + 1}{(Type == StrandType.Upside ? 'Z' : 'N')}";


	/// <inheritdoc cref="IParsable{TSelf}.TryParse(string?, IFormatProvider?, out TSelf)"/>
	public static bool TryParse([NotNullWhen(true)] string? s, out Strand result)
	{
		try
		{
			if (s is null)
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

	/// <inheritdoc cref="IParsable{TSelf}.Parse(string, IFormatProvider?)"/>
	public static Strand Parse(string s)
	{
		if (s.Trim() is not [
			var floorOrTowerCh and ('T' or 't' or 'F' or 'f'),
			var sequenceIndexCh and >= '1' and <= '9',
			var typeCh and ('Z' or 'z' or 'N' or 'n')
		])
		{
			throw new FormatException();
		}

		var rawSequenceIndex = sequenceIndexCh - '1';
		return new(
			(floorOrTowerCh is 'T' or 't' ? 3 : 0) + rawSequenceIndex / 3,
			rawSequenceIndex % 3,
			typeCh is 'N' or 'n' ? StrandType.Downside : StrandType.Upside
		);
	}


	/// <inheritdoc/>
	public static bool operator >(Strand left, Strand right) => left.CompareTo(right) > 0;

	/// <inheritdoc/>
	public static bool operator >=(Strand left, Strand right) => left.CompareTo(right) >= 0;

	/// <inheritdoc/>
	public static bool operator <(Strand left, Strand right) => left.CompareTo(right) < 0;

	/// <inheritdoc/>
	public static bool operator <=(Strand left, Strand right) => left.CompareTo(right) <= 0;
}
