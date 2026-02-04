namespace Sudoku.Analytics.Braiding;

/// <summary>
/// Represents a strand.
/// A <b>Strand</b> is a distribution pattern for a single digit in 3 of the 9 intersections aligned in a chute.
/// There are 6 different strands in each chute.
/// </summary>
/// <param name="chuteIndex">Indicates chute index (0..6).</param>
/// <param name="sequenceIndex">Indicates sequence index (0..3).</param>
/// <param name="type">Indicates type of rotation.</param>
/// <remarks>For more information, please visit <see href="http://sudopedia.enjoysudoku.com/Strand.html">this link</see>.</remarks>
public readonly struct Strand(int chuteIndex, Digit sequenceIndex, StrandType type) :
	IComparable<Strand>,
	IComparisonOperators<Strand, Strand, bool>,
	IEquatable<Strand>,
	IEqualityOperators<Strand, Strand, bool>
{
	/// <inheritdoc cref="Strands"/>
	private static readonly Strand[] StrandsBackingField;


	/// <summary>
	/// Indicates the backing mask.
	/// </summary>
	private readonly byte _mask = (byte)(chuteIndex << 4 | sequenceIndex << 2 | (byte)type);


	/// <summary>
	/// Indicates chute index (0..6).
	/// </summary>
	public int ChuteIndex => _mask >> 4 & 15;

	/// <summary>
	/// Indicates sequence index (0..3).
	/// </summary>
	public int SequenceIndex => _mask >> 2 & 3;

	/// <summary>
	/// Indicates type of strand.
	/// </summary>
	public StrandType Type => (StrandType)(_mask & 3);


	/// <summary>
	/// Represents all possible strand instances.
	/// </summary>
	public static ReadOnlySpan<Strand> Strands => StrandsBackingField;


	/// <include file="../../global-doc-comments.xml" path="g/static-constructor"/>
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
	/// Indicates global index (0..36).
	/// </summary>
	private int GlobalIndex => (BraidAnalysis.ProjectGlobalIndex(ChuteIndex, SequenceIndex) << 1) + (int)(Type - 1);


	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
	public void Deconstruct(out int chuteIndex, out int sequenceIndex, out StrandType type)
		=> (chuteIndex, sequenceIndex, type) = (ChuteIndex, SequenceIndex, Type);

	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] object? obj) => obj is Strand comparer && Equals(comparer);

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
	public static bool operator ==(Strand left, Strand right) => left.Equals(right);

	/// <inheritdoc/>
	public static bool operator !=(Strand left, Strand right) => !(left == right);

	/// <inheritdoc/>
	public static bool operator >(Strand left, Strand right) => left.CompareTo(right) > 0;

	/// <inheritdoc/>
	public static bool operator >=(Strand left, Strand right) => left.CompareTo(right) >= 0;

	/// <inheritdoc/>
	public static bool operator <(Strand left, Strand right) => left.CompareTo(right) < 0;

	/// <inheritdoc/>
	public static bool operator <=(Strand left, Strand right) => left.CompareTo(right) <= 0;
}
