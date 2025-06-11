namespace Sudoku.Analytics;

public partial class Hub
{
	public partial class DifficultyCalculator
	{
		/// <summary>
		/// Represents difficulty calculator for complex single.
		/// </summary>
		public static class ComplexSingle
		{
			/// <summary>
			/// Gets the complexity difficulty for complex single techniques usages.
			/// </summary>
			/// <param name="techniques">The techniques used.</param>
			/// <returns>The result.</returns>
			public static int GetComplexityDifficulty(Technique[][] techniques)
			{
				var result = 0;
				var p = 1;
				foreach (var technique in
					from techniqueGroup in techniques
					from technique in techniqueGroup.AsReadOnlySpan()
					select technique)
				{
					technique.GetDefaultRating(out var directRatingValue);
					result += (int)(directRatingValue / (double)p);
					p *= 3;
				}
				return result;
			}
		}
	}
}
