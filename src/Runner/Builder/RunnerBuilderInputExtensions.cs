namespace AdventOfCode;

public static class RunnerBuilderInputExtensions
{
	public static AdventOfCode.IObserversBuilder<TEntry, TResult> WithInput<TEntry, TResult>(
		this AdventOfCode.IInputBuilder<TEntry, TResult> inputBuilder,
		IEnumerable<TEntry> input
		)
	{
		return inputBuilder.WithInput(input.ToAsyncEnumerable());
	}
}
