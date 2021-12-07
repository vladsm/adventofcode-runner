namespace AdventOfCode;

public static class SiteRunnerExtensions
{
	public static SiteRunner.ISiteRunnerBuilder PuzzleFirstLevel(this SiteRunner runner, int year, int day)
	{
		return runner.Puzzle(year, day, 1);
	}

	public static SiteRunner.ISiteRunnerBuilder PuzzleSecondLevel(this SiteRunner runner, int year, int day)
	{
		return runner.Puzzle(year, day, 2);
	}

	public static SiteRunner.IResultCorrectnessHandlerBuilder<TEntry, TResult> SolveUsing<TEntry, TResult>(
		this SiteRunner.ISiteRunnerBuilder builder,
		Type solverType
		)
	{
		return builder.SolveUsing((IAsyncSolver < TEntry, TResult >)Activator.CreateInstance(solverType));
	}

	public static SiteRunner.IResultCorrectnessHandlerBuilder<TEntry, TResult> SolveUsing<TEntry, TResult, TSolver>(
		this SiteRunner.ISiteRunnerBuilder builder
		)
		where TSolver : IAsyncSolver<TEntry, TResult>
	{
		return builder.SolveUsing(Activator.CreateInstance<TSolver>());
	}

	public static SiteRunner.IResultCorrectnessHandlerBuilder<int, int> SolveUsing<TSolver>(
		this SiteRunner.ISiteRunnerBuilder builder
		)
		where TSolver : IAsyncSolver<int, int>
	{
		return builder.SolveUsing(Activator.CreateInstance<TSolver>());
	}

	public static SiteRunner.ISiteRunnerBuilder SolveWithMatching(this SiteRunner runner, int year, int day)
	{
		return runner.Puzzle(year, day, 1);
	}

	public static AdventOfCode.IRawInputBuilder<TEntry, TResult> HandlingResultCorrectness<TEntry, TResult>(
		this SiteRunner.IResultCorrectnessHandlerBuilder<TEntry, TResult> builder,
		Action<bool> action
		)
	{
		return builder.HandlingResultCorrectness((_, isCorrect) => action(isCorrect));
	}

	public static AdventOfCode.IRawInputBuilder<TEntry, TResult> HandlingResultCorrectness<TEntry, TResult>(
		this SiteRunner.IResultCorrectnessHandlerBuilder<TEntry, TResult> builder,
		Action<TResult, bool> action
		)
	{
		return builder.HandlingResultCorrectnessAsync(handle);

#pragma warning disable CS1998
		async ValueTask handle(TResult result, bool isCorrect)
#pragma warning restore CS1998
		{
			action(result, isCorrect);
		}
	}
}
