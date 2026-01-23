namespace Sudoku.Analytics.Ittoryu;

/// <summary>
/// Represents ittoryu-related technique sets instances.
/// </summary>
public static class TechniqueIttoryuSets
{
	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	extension(TechniqueSet)
	{
		/// <summary>
		/// Indicates the techniques that ittoryu path finder will use.
		/// </summary>
		/// <seealso cref="DisorderedIttoryuFinder"/>
		public static TechniqueSet IttoryuTechniques
			=> [Technique.FullHouse, Technique.HiddenSingleBlock, Technique.HiddenSingleRow, Technique.HiddenSingleColumn, Technique.NakedSingle];

		/// <summary>
		/// Indicates the techniques that ittoryu path finder will use, and naked singles are not included.
		/// </summary>
		/// <seealso cref="DisorderedIttoryuFinder"/>
		public static TechniqueSet IttoryuNakedSingleNotIncluded => TechniqueSet.IttoryuTechniques - Technique.NakedSingle;

		/// <summary>
		/// Indicates the techniques that ittroyu path finder will use,
		/// only containing <see cref="Technique.FullHouse"/> and <see cref="Technique.HiddenSingleBlock"/>.
		/// </summary>
		/// <seealso cref="DisorderedIttoryuFinder"/>
		/// <seealso cref="Technique.FullHouse"/>
		/// <seealso cref="Technique.HiddenSingleBlock"/>
		public static TechniqueSet IttoryuBlockHiddenSingle => [Technique.FullHouse, Technique.HiddenSingleBlock];
	}
}
