namespace AdventOfCode;

internal sealed class SolverFunc<TEntry, TResult> : IAsyncSolver<TEntry, TResult>
{
	private readonly Func<IAsyncEnumerable<TEntry>, ValueTask<TResult>> _solver;

	public SolverFunc(Func<IAsyncEnumerable<TEntry>, ValueTask<TResult>> solver)
	{
		_solver = solver;
	}

	public async ValueTask<TResult> Solve(IAsyncEnumerable<TEntry> entries)
	{
		return await _solver(entries);
	}
}
