namespace Sudoku.Transformations;

/// <summary>
/// Provides with extension methods on <see cref="Grid"/>.
/// </summary>
/// <seealso cref="Grid"/>
public static class GridTransformExtensions
{
	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="this">The current instance.</param>
	extension(ref Grid @this)
	{
		/// <summary>
		/// Applies transform with the specified <see cref="Transform"/>.
		/// </summary>
		/// <param name="transform">The transform.</param>
		public void Apply(in Transform transform)
		{
			var @base = @this.ToString("0");
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
			@this = transform.ShouldTranspose ? result.Transpose() : result;
		}
	}
}
