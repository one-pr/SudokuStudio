namespace Sudoku.Concepts;

using Type_Candidate = Candidate;

public partial class Limits
{
	/// <summary>
	/// Indicates candidate-related limits.
	/// </summary>
	public static class Candidate
	{
		/// <summary>
		/// Indicates min value of candidate.
		/// </summary>
		public const Type_Candidate Min = 0;

		/// <summary>
		/// Indicates max value of candidate.
		/// </summary>
		public const Type_Candidate Max = 728;
	}
}
