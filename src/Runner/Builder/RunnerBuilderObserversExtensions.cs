namespace AdventOfCode;

public static class RunnerBuilderObserversExtensions
{
	// TODO: Add ObservingRawInputWith extensions

	public static AdventOfCode.IObserversBuilder<TEntry, TResult> ObservingAsyncInputWith<TEntry, TResult>(
		this AdventOfCode.IObserversBuilder<TEntry, TResult> observersBuilder,
		Func<string, TEntry, ValueTask> observerAction
		)
	{
		var observer = new AsyncInputObserverAction<TEntry>(observerAction);
		return observersBuilder.ObservingInputWith(observer);
	}

	public static AdventOfCode.IObserversBuilder<TEntry, TResult> ObservingInputWith<TEntry, TResult>(
		this AdventOfCode.IObserversBuilder<TEntry, TResult> observersBuilder,
		Action<string, TEntry> observerAction
		)
	{
		var observer = new InputObserverAction<TEntry>(observerAction);
		return observersBuilder.ObservingInputWith(observer);
	}

	public static AdventOfCode.IObserversBuilder<TEntry, TResult> ObservingAsyncResultWith<TEntry, TResult>(
		this AdventOfCode.IObserversBuilder<TEntry, TResult> observersBuilder,
		Func<TResult, ValueTask> observerAction
		)
	{
		var observer = new AsyncResultObserverAction<TResult>(observerAction);
		return observersBuilder.ObservingResultWith(observer);
	}

	public static AdventOfCode.IObserversBuilder<TEntry, TResult> ObservingResultWith<TEntry, TResult>(
		this AdventOfCode.IObserversBuilder<TEntry, TResult> observersBuilder,
		Action<TResult> observerAction
		)
	{
		var observer = new ResultObserverAction<TResult>(observerAction);
		return observersBuilder.ObservingResultWith(observer);
	}


	private sealed class AsyncInputObserverAction<TEntry> : IInputObserver<TEntry>
	{
		private readonly Func<string, TEntry, ValueTask> _action;

		public AsyncInputObserverAction(Func<string, TEntry, ValueTask> action)
		{
			_action = action;
		}

		public async ValueTask Observe(string line, TEntry entry)
		{
			await _action(line, entry);
		}
	}

	private sealed class InputObserverAction<TEntry> : IInputObserver<TEntry>
	{
		private readonly Action<string, TEntry> _action;

		public InputObserverAction(Action<string, TEntry> action)
		{
			_action = action;
		}

#pragma warning disable CS1998
		public async ValueTask Observe(string line, TEntry entry)
#pragma warning restore CS1998
		{
			_action(line, entry);
		}
	}


	private sealed class AsyncResultObserverAction<TResult> : IResultObserver<TResult>
	{
		private readonly Func<TResult, ValueTask> _action;

		public AsyncResultObserverAction(Func<TResult, ValueTask> action)
		{
			_action = action;
		}

		public async ValueTask Observe(TResult result)
		{
			await _action(result);
		}
	}

	private sealed class ResultObserverAction<TResult> : IResultObserver<TResult>
	{
		private readonly Action<TResult> _action;

		public ResultObserverAction(Action<TResult> action)
		{
			_action = action;
		}

#pragma warning disable CS1998
		public async ValueTask Observe(TResult result)
#pragma warning restore CS1998
		{
			_action(result);
		}
	}
}
