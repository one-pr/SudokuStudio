namespace Sudoku.Concepts;

/// <summary>
/// Represents a list of creating methods.
/// </summary>
public static class CellMapCreator
{
	/// <summary>
	/// Provides extension members on <see cref="CellMap"/>.
	/// </summary>
	extension(CellMap)
	{
		/// <summary>
		/// Initializes an instance with two binary values.
		/// </summary>
		/// <param name="high">Higher 40 bits.</param>
		/// <param name="low">Lower 41 bits.</param>
		/// <returns>The result instance created.</returns>
		public static CellMap Create(ulong high, ulong low)
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
			=> Create(
				((ulong)high & 0x7FFFFFFUL) << 13 | (ulong)mid >> 14 & 0x1FFFUL,
				((ulong)mid & 0x3FFFL) << 27 | (ulong)low & 0x7FFFFFFUL
			);

		/// <summary>
		/// Initializes an instance with a <see cref="Vector128{T}"/> of <see cref="long"/>.
		/// </summary>
		/// <param name="vector">Two bits, represented as high 41 and low 40 bits.</param>
		/// <returns>A <see cref="CellMap"/> instance.</returns>
		public static CellMap Create(Vector128<ulong> vector)
		{
			CellMap result;
			result._vector = vector;
			return result;
		}
	}
}
