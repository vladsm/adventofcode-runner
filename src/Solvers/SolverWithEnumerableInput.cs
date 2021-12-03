namespace AdventOfCode;

public abstract class SolverWithEnumerableInput<TEntry, TResult> : IAsyncSolver<TEntry, TResult>
{
	public async ValueTask<TResult> Solve(IAsyncEnumerable<TEntry> entries)
	{
		TEntry[] entriesArray = await entries.ToArrayAsync();
		return Solve(entriesArray);
	}

	protected abstract TResult Solve(IEnumerable<TEntry> entries);
}
