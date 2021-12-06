namespace AdventOfCode;

public interface IRawInputObserver
{
	ValueTask Observe(string line);
}


public interface IInputObserver<in TEntry>
{
	ValueTask Observe(string line, TEntry entry);
}


public interface IResultObserver<in TResult>
{
	ValueTask Observe(TResult result);
}


public class Runner<TEntry, TResult>
{
	private readonly IAsyncEnumerable<TEntry>? _inputEntries;
	private readonly IAsyncEnumerable<string>? _inputLines;
	private readonly Func<string, int, TEntry>? _lineParser;

	private readonly IAsyncSolver<TEntry, TResult> _solver;

	private readonly IRawInputObserver[] _rawInputObservers;
	private readonly IInputObserver<TEntry>[] _inputObservers;
	private readonly IResultObserver<TResult>[] _resultObservers;
	private readonly Func<ValueTask>[] _afterRunHandlers;


	public Runner(
		IAsyncEnumerable<string> inputLines,
		Func<string, int, TEntry> lineParser,
		IAsyncSolver<TEntry, TResult> solver,
		IEnumerable<IRawInputObserver> rawInputObservers,
		IEnumerable<IInputObserver<TEntry>> inputObservers,
		IEnumerable<IResultObserver<TResult>> resultObservers,
		IEnumerable<Func<ValueTask>> afterRunHandlers
		)
	{
		_inputLines = inputLines;
		_lineParser = lineParser;
		_solver = solver;
		_rawInputObservers = rawInputObservers.AsArray();
		_inputObservers = inputObservers.AsArray();
		_resultObservers = resultObservers.AsArray();
		_afterRunHandlers = afterRunHandlers.AsArray();
	}

	public Runner(
		IAsyncEnumerable<TEntry> inputEntries,
		IAsyncSolver<TEntry, TResult> solver,
		IEnumerable<IRawInputObserver> rawInputObservers,
		IEnumerable<IInputObserver<TEntry>> inputObservers,
		IEnumerable<IResultObserver<TResult>> resultObservers,
		IEnumerable<Func<ValueTask>> afterRunHandlers
		)
	{
		_inputEntries = inputEntries;
		_solver = solver;
		_rawInputObservers = rawInputObservers.AsArray();
		_inputObservers = inputObservers.AsArray();
		_resultObservers = resultObservers.AsArray();
		_afterRunHandlers = afterRunHandlers.AsArray();
	}

	public async ValueTask Run()
	{
		TResult result = await _solver.Solve(_inputEntries ?? parseInput());
		await ObserveResult(result);
		await HandleAfterRun();

		async IAsyncEnumerable<TEntry> parseInput()
		{
			if (_inputLines is null)
			{
				throw new InvalidOperationException("Input is not provider");
			}
			if (_lineParser is null)
			{
				throw new InvalidOperationException("Input line parser is not provided");
			}

			int lineIndex = 0;
			await foreach (string line in _inputLines)
			{
				await ObserveInput(line);

				TEntry entry = _lineParser(line, lineIndex++);
				await ObserveInput(line, entry);

				yield return entry;
			}
		}
	}

	private async ValueTask ObserveInput(string line)
	{
		foreach (IRawInputObserver observer in _rawInputObservers)
		{
			await observer.Observe(line);
		}
	}

	private async ValueTask ObserveInput(string line, TEntry entry)
	{
		foreach (IInputObserver<TEntry> observer in _inputObservers)
		{
			await observer.Observe(line, entry);
		}
	}

	private async ValueTask ObserveResult(TResult result)
	{
		foreach (IResultObserver<TResult> observer in _resultObservers)
		{
			await observer.Observe(result);
		}
	}

	private async ValueTask HandleAfterRun()
	{
		foreach (Func<ValueTask> handler in _afterRunHandlers)
		{
			await handler.Invoke();
		}
	}
}
