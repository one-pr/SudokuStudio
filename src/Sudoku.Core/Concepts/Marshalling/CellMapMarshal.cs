namespace Sudoku.Concepts.Marshalling;

/// <summary>
/// Represents extra data set defined in <see cref="CellMap"/>.
/// </summary>
/// <seealso cref="CellMap"/>
public static class CellMapMarshal
{
	/// <summary>
	/// Indicates a list of <see cref="CellMap"/> instances that are initialized as singleton element by its corresponding index.
	/// For example, <c>CellMaps[0]</c> is to <c>CellMap.Empty + 0</c>, i.e. <c>r1c1</c>.
	/// </summary>
	internal static readonly CellMap[] CellMaps;


	/// <include file="../../global-doc-comments.xml" path="g/static-constructor"/>
	static CellMapMarshal()
	{
		CellMaps = new CellMap[81];
		var span = CellMaps.AsSpan();
		var cell = 0;
		foreach (ref var map in span)
		{
			map += cell++;
		}
	}


	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	extension(CellMap)
	{
		/// <summary>
		/// Initializes an instance with two binary values.
		/// </summary>
		/// <param name="high">Higher 40 bits.</param>
		/// <param name="low">Lower 41 bits.</param>
		/// <returns>The result instance created.</returns>
		public static CellMap Create(long high, long low)
		{
			CellMap result;
			result._vector = CellMap.CV(high, low);
			return result;
		}

		/// <summary>
		/// Initializes an instance with three binary values.
		/// </summary>
		/// <param name="high">Higher 27 bits.</param>
		/// <param name="mid">Medium 27 bits.</param>
		/// <param name="low">Lower 27 bits.</param>
		/// <returns>The result instance created.</returns>
		public static CellMap Create(int high, int mid, int low)
			=> Create((high & 0x7FFFFFFL) << 13 | mid >>> 14 & 0x1FFFL, (mid & 0x3FFFL) << 27 | low & 0x7FFFFFFL);

		/// <summary>
		/// Initializes an instance with a <see cref="Vector128{T}"/> of <see cref="long"/>.
		/// </summary>
		/// <param name="vector">Two bits, represented as high 41 and low 40 bits.</param>
		/// <returns>A <see cref="CellMap"/> instance.</returns>
		public static CellMap Create(in Vector128<long> vector)
		{
			CellMap result;
			result._vector = vector;
			return result;
		}
	}
}
