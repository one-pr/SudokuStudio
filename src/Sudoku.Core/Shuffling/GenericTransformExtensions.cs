namespace Sudoku.Shuffling;

/// <summary>
/// Provides with extension methods on <see cref="Grid"/>.
/// </summary>
/// <seealso cref="Grid"/>
public static class GridGenericTransformExtensions
{
	/// <summary>
	/// Provides extension members on <see cref="Grid"/>.
	/// </summary>
	extension(Grid)
	{
#if EXTENSION_OPERATORS
		/// <summary>
		/// Applies transform with the specified <see cref="GenericTransform"/>.
		/// </summary>
		/// <param name="original">The original grid.</param>
		/// <param name="transform">The transform.</param>
		/// <returns>The grid transformed.</returns>
		public static Grid operator >>(in Grid original, in GenericTransform transform)
		{
			var @base = original.ToString("0");
			var rows = transform.RowIndicesRelabeled;
			var columns = transform.ColumnIndicesRelabeled;
			var digits = transform.DigitsRelabeled;

			var resultCharacters = (stackalloc char[81]);
			resultCharacters.Fill('0');
			for (var canonicalCell = 0; canonicalCell < 81; canonicalCell++)
			{
				var canonicalRow = canonicalCell / 9;
				var canonicalColumn = canonicalCell % 9;
				var sourceRow = rows[canonicalRow];
				var sourceColumn = columns[canonicalColumn];
				if (sourceRow < 0 || sourceColumn < 0)
				{
					continue;
				}

				var targetIndex = sourceRow * 9 + sourceColumn;
				var ch = @base[canonicalCell];
				var originalDigit = digits[ch switch { '0' or '.' => -1, >= '1' and <= '9' => ch - '1' }];
				resultCharacters[targetIndex] = originalDigit == -1 ? '0' : (char)('1' + originalDigit);
			}

			var result = Grid.Parse(resultCharacters);
			return transform.ShouldTranspose ? result.Transpose() : result;
		}
#endif
	}
}
