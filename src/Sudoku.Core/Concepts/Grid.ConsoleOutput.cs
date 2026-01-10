namespace Sudoku.Concepts;

public partial struct Grid
{
	/// <summary>
	/// Provides a way to output an instance of type <see cref="Grid"/>, for bits.
	/// </summary>
	public static class ConsoleOutput
	{
		/// <summary>
		/// Returns the binary text that represents an instance, with text colorized.
		/// </summary>
		/// <param name="this">The instance.</param>
		/// <returns>The string.</returns>
		public static string GetBitsViewTextColorized(ref readonly Grid @this) => GetBitsViewTextCore(in @this, "\e[90m", "\e[33m", null);

		/// <summary>
		/// Returns the binary text that represents an instance, without colorized.
		/// </summary>
		/// <param name="this">The instance.</param>
		/// <returns>The string.</returns>
		public static string GetBitsViewText(ref readonly Grid @this) => GetBitsViewTextCore(in @this, null, null, null);

		/// <summary>
		/// The core method to output text.
		/// </summary>
		private static string GetBitsViewTextCore<TGrid>(ref readonly TGrid @this, string? part1, string? part2, string? part3)
			where TGrid : unmanaged, IGrid<TGrid>, IInlineArrayGrid<TGrid>
		{
			var masks = @this.Elements;
			var sb = new StringBuilder();

			var part1End = part1 is null ? null : "\e[0m";
			var part2End = part2 is null ? null : "\e[0m";
			var part3End = part3 is null ? null : "\e[0m";
			for (var i = 0; i < 81; i++)
			{
				var bits = Convert.ToString(masks[i], 2).PadLeft(16, '0');
				sb.Append($"{part1}{bits[..4]}{part1End}{part2}{bits[4..7]}{part2End}{part3}{bits[7..]}{part3End} ");
				if ((i + 1) % 9 == 0)
				{
					sb.AppendLine();
				}
			}
			return sb.ToString();
		}
	}
}
