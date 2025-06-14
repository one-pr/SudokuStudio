namespace Sudoku.Concepts.Supersymmetry;

/// <summary>
/// Provides with extension methods on <see cref="Space"/>.
/// </summary>
/// <seealso cref="Space"/>
public static class SpaceExtensions
{
	/// <summary>
	/// Provides extension members on <see cref="Space"/>.
	/// </summary>
	extension(Space @this)
	{
		/// <summary>
		/// Determine whether the specified assignment combination can satisfy the current rank set.
		/// </summary>
		/// <param name="assignments">The assignments.</param>
		/// <param name="isTruth">Indicates whether the current space is as a truth.</param>
		/// <returns>A <see cref="bool"/> result.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsSatisfied(in CandidateMap assignments, bool isTruth)
			=> (isTruth, @this) switch
			{
				(true, { Cell: var cell and not -1 })
					=> BitOperations.IsPow2(assignments.GetDigitsFor(cell)),
				(true, { House: var house, Digit: var digit })
					=> BitOperations.IsPow2(assignments.GetPositionsFor(house, digit)),
				(_, { Cell: var cell and not -1 })
					=> BitOperations.PopCount(assignments.GetDigitsFor(cell)) <= 1,
				(_, { House: var house, Digit: var digit })
					=> BitOperations.PopCount(assignments.GetPositionsFor(house, digit)) <= 1
			};

		/// <summary>
		/// Determine whether the specified assignment is inside the set.
		/// </summary>
		/// <param name="assignment">The assignment.</param>
		/// <returns>A <see cref="bool"/> result.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool ContainsAssignment(Candidate assignment)
			=> @this switch
			{
				{ Cell: var cell and not -1 }
					=> assignment / 9 == cell,
				{ House: var house, Digit: var digit }
					=> assignment % 9 == digit && HousesMap[house].Contains(assignment / 9)
			};

		/// <summary>
		/// Try to find all possible candidates in the current rank set.
		/// </summary>
		/// <param name="grid">The grid.</param>
		/// <returns>The candidates.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public CandidateMap GetAvailableRange(in Grid grid)
		{
			switch (@this)
			{
				case { Cell: var cell and not -1 }:
				{
					var result = CandidateMap.Empty;
					foreach (var digit in grid.GetCandidates(cell))
					{
						result.Add(cell * 9 + digit);
					}
					return result;
				}
				case { House: var house, Digit: var digit }:
				{
					var result = CandidateMap.Empty;
					foreach (var cell in HousesMap[house])
					{
						if (grid.Exists(cell, digit) is true)
						{
							result.Add(cell * 9 + digit);
						}
					}
					return result;
				}
			}
		}
	}
}
