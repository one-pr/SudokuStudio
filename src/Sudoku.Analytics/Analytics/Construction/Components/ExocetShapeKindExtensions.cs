namespace Sudoku.Analytics.Construction.Components;

/// <summary>
/// Provides with extension methods around enumeration type <see cref="ExocetShapeKind"/>.
/// </summary>
internal static class ExocetShapeKindExtensions
{
	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="this">The current instance.</param>
	extension(IComplexSeniorExocet @this)
	{
		/// <summary>
		/// Indicates the shape kind of the current exocet pattern.
		/// </summary>
		public ExocetShapeKind ShapeKind
		{
			get
			{
				var finalMask = @this.CrosslineHousesMask | @this.ExtraHousesMask;
				return (finalMask & AllBlocksMask, finalMask & AllRowsMask, finalMask & AllColumnsMask) switch
				{
					(_, not 0, not 0) => ExocetShapeKind.Mutant,
					(not 0, _, _) => ExocetShapeKind.Franken,
					_ => ExocetShapeKind.Basic
				};
			}
		}
	}
}
