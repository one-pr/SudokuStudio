namespace Sudoku.Concepts;

/// <summary>
/// Represents a grid instance that supports inline-array-related members.
/// </summary>
/// <typeparam name="TSelf"><include file="../../global-doc-comments.xml" path="/g/self-type-constraint"/></typeparam>
public interface IInlineArrayGrid<TSelf> : IGrid<TSelf> where TSelf : unmanaged, IInlineArrayGrid<TSelf>
{
	/// <summary>
	/// Indicates the elements.
	/// </summary>
	[UnscopedRef]
	ReadOnlySpan<Mask> Elements { get; }

	/// <summary>
	/// Indicates the inner array that stores the masks of the sudoku grid, which stores the in-time sudoku grid inner information.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The field uses the mask table of length 81 to indicate the state and all possible candidates
	/// holding for each cell. Each mask uses a <see cref="Mask"/> value, but only uses 12 of 16 bits.
	/// <code>
	/// .---------------.-----------.-----------------------------------.
	/// | 15  14  13  12| 11  10  9 | 8   7   6   5   4   3   2   1   0 |
	/// |---------------+-----------+-----------------------------------|
	/// |   |   |   |   | 0 | 0 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 |
	/// '---------------+-----------+-----------------------------------'
	///  \_____________/ \_________/ \_________________________________/
	///        (3)           (2)                     (1)
	/// </code>
	/// Here the 9 bits in (1) indicate whether each digit is possible candidate in the current cell for each bit respectively,
	/// and the higher 3 bits in (2) indicate the cell state. The possible cell state are:
	/// <list type="table">
	/// <listheader>
	/// <term>State name</term>
	/// <description>Description</description>
	/// </listheader>
	/// <item>
	/// <term>Empty cell (flag: <see cref="CellState.Empty"/>)</term>
	/// <description>The cell is currently empty, and wait for being filled.</description>
	/// </item>
	/// <item>
	/// <term>Modifiable cell (flag: <see cref="CellState.Modifiable"/>)</term>
	/// <description>The cell is filled by a digit, but the digit isn't the given by the initial grid.</description>
	/// </item>
	/// <item>
	/// <term>Given cell (flag: <see cref="CellState.Given"/>)</term>
	/// <description>The cell is filled by a digit, which is given by the initial grid and can't be modified.</description>
	/// </item>
	/// </list>
	/// </para>
	/// <para>Part (3) is for reserved cases. Such bits may not be used.</para>
	/// </remarks>
	/// <seealso cref="CellState"/>
	[UnscopedRef]
	protected ref readonly Mask FirstMaskRef { get; }


	/// <summary>
	/// Indicates the default mask of a cell (an empty cell, with all 9 candidates left).
	/// </summary>
	static virtual Mask DefaultMask => (Mask)(TSelf.EmptyMask | TSelf.MaxCandidatesMask);

	/// <summary>
	/// Indicates the empty mask, modifiable mask and given mask.
	/// </summary>
	static abstract Mask EmptyMask { get; }

	/// <summary>
	/// Indicates the modifiable mask.
	/// </summary>
	static abstract Mask ModifiableMask { get; }

	/// <summary>
	/// Indicates the given mask.
	/// </summary>
	static abstract Mask GivenMask { get; }

	/// <summary>
	/// Indicates the maximum candidate mask that used.
	/// </summary>
	static abstract Mask MaxCandidatesMask { get; }


	/// <summary>
	/// Returns the reference to the element at the specified index.
	/// </summary>
	/// <param name="index">The desired index.</param>
	/// <returns>The reference to the element.</returns>
	/// <exception cref="IndexOutOfRangeException">Throws when the index is out of range.</exception>
	[UnscopedRef]
	ref Mask this[Cell index] { get; }
}
