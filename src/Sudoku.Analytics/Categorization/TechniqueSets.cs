namespace Sudoku.Categorization;

/// <summary>
/// Predefined <see cref="TechniqueSet"/> instances.
/// </summary>
/// <seealso cref="TechniqueSet"/>
public static class TechniqueSets
{
	/// <summary>
	/// Provides extension members on <see cref="TechniqueSet"/>.
	/// </summary>
	extension(TechniqueSet)
	{
		/// <summary>
		/// Indicates all techniques are not included.
		/// </summary>
		public static TechniqueSet None => [];

		/// <summary>
		/// Indicates all <see cref="Technique"/> fields included.
		/// </summary>
		public static TechniqueSet All => [.. Technique.Values[1..]];

		/// <summary>
		/// Indicates all assignment techniques.
		/// </summary>
		public static TechniqueSet Assignments
			=> [
				Technique.FullHouse, Technique.LastDigit, Technique.HiddenSingleBlock,
				Technique.HiddenSingleRow, Technique.HiddenSingleColumn, Technique.NakedSingle
			];
	}
}
