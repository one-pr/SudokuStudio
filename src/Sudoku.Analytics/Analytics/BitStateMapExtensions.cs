namespace Sudoku.Analytics;

/// <summary>
/// Provides extension methods on <see cref="CellMap"/> and <see cref="CandidateMap"/> instances.
/// </summary>
/// <seealso cref="CellMap"/>
/// <seealso cref="CandidateMap"/>
public static class BitStateMapExtensions
{
	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	extension(CellMap)
	{
		/// <summary>
		/// Expands the operator to <c><![CDATA[(a & b).PeerIntersection & b]]></c>.
		/// </summary>
		/// <param name="base">The base map.</param>
		/// <param name="template">The template map that the base map to check and cover.</param>
		/// <returns>The result map.</returns>
		/// <remarks>
		/// <para>
		/// The operator is commonly used for checking eliminations, especially in type 2 of deadly patterns. 
		/// </para>
		/// <para>
		/// For example, if we should check the eliminations
		/// of digit <c>d</c>, we may use the expression
		/// <code><![CDATA[
		/// (urCells & grid.CandidatesMap[d]).PeerIntersection & grid.CandidatesMap[d]
		/// ]]></code>
		/// to express the eliminations are the peer intersection of cells of digit <c>d</c>
		/// appeared in <c>urCells</c>. This expression can be simplified to
		/// <code><![CDATA[
		/// urCells % grid.CandidatesMap[d]
		/// ]]></code>
		/// </para>
		/// </remarks>
		public static CellMap operator %(in CellMap @base, in CellMap template) => (@base & template).PeerIntersection & template;
	}

	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	extension(CandidateMap)
	{
		/// <inheritdoc cref="extension(CellMap).op_Modulus(in CellMap, in CellMap)"/>
		public static CandidateMap operator %(in CandidateMap @base, in CandidateMap template)
			=> (@base & template).PeerIntersection & template;
	}
}
