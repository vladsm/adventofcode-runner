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
	
	public static AdventOfCode.IObserversBuilder<TEntry, TResult> ParsingInputWith<TEntry, TResult>(
		this AdventOfCode.IRawInputBuilder<TEntry, TResult> inputBuilder,
		Func<string, TEntry> lineParser
		)
	{
		return inputBuilder.ParsingInputWith((line, _) => lineParser(line));
	}
}
