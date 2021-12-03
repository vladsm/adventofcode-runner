namespace AdventOfCode;

public interface IAsyncSolver<in TEntry, TResult>
{
	ValueTask<TResult> Solve(IAsyncEnumerable<TEntry> entries);
}
