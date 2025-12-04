namespace Sudoku.Solving;

/// <summary>
/// Provides with extension methods on <see cref="ISolutionEnumerableSolver{TSelf}"/>.
/// </summary>
/// <seealso cref="ISolutionEnumerableSolver{TSelf}"/>
public static class SolutionEnumerableSolverExtensions
{
	/// <summary>
	/// Provides extension members on <typeparamref name="TSolver"/>,
	/// where <typeparamref name="TSolver"/> satisfies <see cref="ISolutionEnumerableSolver{TSelf}"/> constraint.
	/// </summary>
	/// <typeparam name="TSolver">The type of solver instance.</typeparam>
	/// <param name="this">The solver instance.</param>
	extension<TSolver>(TSolver @this) where TSolver : ISolutionEnumerableSolver<TSolver>
	{
		/// <summary>
		/// Count the number of solutions can be found of a grid.
		/// </summary>
		/// <param name="grid">The grid to be solved.</param>
		/// <param name="cancellationToken">The cancellation token that can cancel the current operation.</param>
		/// <returns>
		/// A <see cref="Task{TResult}"/> instance indicating the asynchronous operation,
		/// with an <see cref="int"/> value indicating the result can be produced after the operation executed.
		/// </returns>
		public async Task<int> CountSolutionsAsync(Grid grid, CancellationToken cancellationToken = default)
		{
			var result = 0;
			await foreach (var _ in @this.EnumerateSolutionsAsync(grid, cancellationToken))
			{
				result++;
				if (cancellationToken.IsCancellationRequested)
				{
					return 0;
				}
			}
			return result;
		}

		/// <summary>
		/// Try to enumerate all possible solutions of the specified grid, by using the current solver, in asynchronous way.
		/// </summary>
		/// <param name="grid">The grid.</param>
		/// <param name="cancellationToken">The cancellation token that can cancel the current operation.</param>
		/// <returns>A sequence of <see cref="Grid"/> values indicating the raw solution text to the puzzle.</returns>
		public async IAsyncEnumerable<Grid> EnumerateSolutionsAsync(
			Grid grid,
			[EnumeratorCancellation] CancellationToken cancellationToken = default
		)
		{
			var channel = Channel.CreateUnbounded<Grid>(new() { SingleReader = true, SingleWriter = true });
			try
			{
				// Temporarily add handler.
				@this.SolutionFound += this_SolutionFound;

				// Perform adding operation.
				ThreadPool.QueueUserWorkItem(
					_ =>
					{
						try
						{
							@this.EnumerateSolutionsCore(grid, cancellationToken);
						}
						finally
						{
							channel.Writer.TryComplete();
						}
					}
				);

				// Consume the solutions concurrently.
				while (await channel.Reader.WaitToReadAsync(cancellationToken))
				{
					if (channel.Reader.TryRead(out var step))
					{
						yield return step;
					}
				}

				// Wait for completion.
				await channel.Reader.Completion;
			}
			finally
			{
				// Remove temporary handler.
				@this.SolutionFound -= this_SolutionFound;
			}


			async void this_SolutionFound(TSolver? _, SolverSolutionFoundEventArgs e)
				=> await channel.Writer.WriteAsync(e.Solution, cancellationToken);
		}
	}
}
