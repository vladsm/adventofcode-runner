namespace AdventOfCode;

public class SiteRunner
{
	private readonly HttpClient _httpClient;

	public SiteRunner(
		HttpClient httpClient,
		string sessionToken
		) : this(httpClient, null, sessionToken)
	{
	}

	public SiteRunner(
		HttpClient httpClient,
		Uri? siteUri = null,
		string ? sessionToken = null
		)
	{
		_httpClient = httpClient;
		if (sessionToken is not null)
		{
			_httpClient.DefaultRequestHeaders.Add("cookie", "session=" + sessionToken);
		}
		if (siteUri is not null)
		{
			_httpClient.BaseAddress = siteUri;
		}
	}

	public ISiteRunnerBuilder Puzzle(int year, int day, int level)
	{
		return new SiteDayLevelRunner(year, day, level, _httpClient);
	}


	public interface ISiteRunnerBuilder
	{
		IResultCorrectnessHandlerBuilder<TEntry, TResult> SolveUsing<TEntry, TResult>(
			IAsyncSolver<TEntry, TResult> solver
			);
	}

	public interface IResultCorrectnessHandlerBuilder<TEntry, out TResult> :
		AdventOfCode.IRawInputBuilder<TEntry, TResult>
	{
		AdventOfCode.IRawInputBuilder<TEntry, TResult> HandlingResultCorrectnessAsync(
			Func<TResult, bool, ValueTask> handler
			);
	}
}
