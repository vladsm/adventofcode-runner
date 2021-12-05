namespace AdventOfCode;

public static class AdventOfCode
{
	public static IInputBuilder<TEntry, TResult> SolveUsing<TEntry, TResult>(
		IAsyncSolver<TEntry, TResult> solver
		)
	{
		var builder = new RunnerBuilder<TEntry, TResult>();
		builder.SetSolver(solver);
		return builder;
	}

	public static IInputBuilder<TEntry, TResult> SolveUsing<TEntry, TResult>(
		Func<IAsyncEnumerable<TEntry>, ValueTask<TResult>> solver
		)
	{
		return SolveUsing(new SolverFunc<TEntry, TResult>(solver));
	}


	public interface IInputBuilder<TEntry, out TResult>
	{ 
		IObserversBuilder<TEntry, TResult> WithInput(IAsyncEnumerable<TEntry> input);
		IRawInputBuilder<TEntry, TResult> WithInput(IAsyncEnumerable<string> input);
	}


	public interface IRawInputBuilder<TEntry, out TResult>
	{
		IObserversBuilder<TEntry, TResult> ParsingInputWith(Func<string, TEntry> lineParser);
	}


	public interface IObserversBuilder<out TEntry, out TResult> : IRunLauncher
	{
		IObserversBuilder<TEntry, TResult> ObservingRawInputWith(IRawInputObserver inputObserver);
		IObserversBuilder<TEntry, TResult> ObservingInputWith(IInputObserver<TEntry> inputObserver);
		IObserversBuilder<TEntry, TResult> ObservingResultWith(IResultObserver<TResult> resultObserver);
		IObserversBuilder<TEntry, TResult> AfterRun(Func<ValueTask> action);
	}


	public interface IRunLauncher
	{
		ValueTask Run();
	}


	private sealed class RunnerBuilder<TEntry, TResult> :
		IInputBuilder<TEntry, TResult>,
		IRawInputBuilder<TEntry, TResult>,
		IObserversBuilder<TEntry, TResult>
	{
		private IAsyncSolver<TEntry, TResult>? _solver;
		private IAsyncEnumerable<TEntry>? _inputEntries;
		private IAsyncEnumerable<string>? _inputLines;
		private Func<string, TEntry>? _lineParser;
		private readonly List<IRawInputObserver> _rawInputObservers = new();
		private readonly List<IInputObserver<TEntry>> _inputObservers = new();
		private readonly List<IResultObserver<TResult>> _resultObservers = new();
		private readonly List<Func<ValueTask>> _afterRunHandlers = new();


		public void SetSolver(IAsyncSolver<TEntry, TResult> solver)
		{
			_solver = solver;
		}

		IObserversBuilder<TEntry, TResult> IInputBuilder<TEntry, TResult>.WithInput(
			IAsyncEnumerable<TEntry> input
			)
		{
			_inputEntries = input;
			return this;
		}

		IRawInputBuilder<TEntry, TResult> IInputBuilder<TEntry, TResult>.WithInput(
			IAsyncEnumerable<string> input
			)
		{
			_inputLines = input;
			return this;
		}

		IObserversBuilder<TEntry, TResult> IRawInputBuilder<TEntry, TResult>.ParsingInputWith(
			Func<string, TEntry> lineParser
			)
		{
			_lineParser = lineParser;
			return this;
		}

		IObserversBuilder<TEntry, TResult> IObserversBuilder<TEntry, TResult>.ObservingRawInputWith(
			IRawInputObserver inputObserver
			)
		{
			_rawInputObservers.Add(inputObserver);
			return this;
		}

		IObserversBuilder<TEntry, TResult> IObserversBuilder<TEntry, TResult>.ObservingInputWith(
			IInputObserver<TEntry> inputObserver
			)
		{
			_inputObservers.Add(inputObserver);
			return this;
		}

		IObserversBuilder<TEntry, TResult> IObserversBuilder<TEntry, TResult>.ObservingResultWith(
			IResultObserver<TResult> resultObserver
			)
		{
			_resultObservers.Add(resultObserver);
			return this;
		}

		IObserversBuilder<TEntry, TResult> IObserversBuilder<TEntry, TResult>.AfterRun(
			Func<ValueTask> action
			)
		{
			_afterRunHandlers.Add(action);
			return this;
		}

		async ValueTask IRunLauncher.Run()
		{
			Runner<TEntry, TResult> runner = Build();
			await runner.Run();
		}

		private Runner<TEntry, TResult> Build()
		{
			if (_solver is null)
			{
				throw new InvalidOperationException("Can not build runner: solver is not specified");
			}

			if (_inputEntries is not null)
			{
				return new Runner<TEntry, TResult>(
					_inputEntries,
					_solver,
					_rawInputObservers,
					_inputObservers,
					_resultObservers,
					_afterRunHandlers
					);
			}

			if (_inputLines is null)
			{
				throw new InvalidOperationException(
					"Can not build runner: neither typed input nor raw input is specified"
					);
			}
			if (_lineParser is null)
			{
				throw new InvalidOperationException(
					"Can not build runner: line parser is not specified for the raw input"
					);
			}

			return new Runner<TEntry, TResult>(
				_inputLines,
				_lineParser,
				_solver,
				_rawInputObservers,
				_inputObservers,
				_resultObservers,
				_afterRunHandlers
				);
		}
	}
}
