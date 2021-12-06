namespace AdventOfCode;

public abstract class SolverWithArrayInput<TEntry, TResult> : IAsyncSolver<TEntry, TResult>
{
	public async ValueTask<TResult> Solve(IAsyncEnumerable<TEntry> entries)
	{
		TEntry[] entriesArray = await entries.ToArrayAsync();
		return Solve(entriesArray);
	}

	protected abstract TResult Solve(TEntry[] entries);
}
