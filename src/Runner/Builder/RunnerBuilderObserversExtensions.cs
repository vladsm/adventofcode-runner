namespace AdventOfCode;

public static class RunnerBuilderObserversExtensions
{
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
