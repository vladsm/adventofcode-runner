using System.Text;
using System.Text.RegularExpressions;

namespace AdventOfCode;

internal sealed class SiteDayLevelRunner :
	SiteRunner.ISiteRunnerBuilder
{
	private readonly int _year;
	private readonly int _day;
	private readonly int _level;
	private readonly HttpClient _httpClient;

	public SiteDayLevelRunner(
		int year,
		int day,
		int level,
		HttpClient httpClient
		)
	{
		_year = year;
		_day = day;
		_level = level;
		_httpClient = httpClient;
	}

	public SiteRunner.IResultCorrectnessHandlerBuilder<TEntry, TResult> SolveUsing<TEntry, TResult>(
		IAsyncSolver<TEntry, TResult> solver
		)
	{
		var rawInputBuilder = AdventOfCode.SolveUsing(solver).WithInput(LoadInput());
		return new Builder<TEntry, TResult>(_year, _day, _level, _httpClient, rawInputBuilder);
	}


	private async IAsyncEnumerable<string> LoadInput()
	{
		HttpResponseMessage response = await _httpClient.GetAsync($"/{_year}/day/{_day}/input");
		Stream stream = await response.Content.ReadAsStreamAsync();

		using var textReader = new StreamReader(stream, Encoding.UTF8, true, 8 * 1024, true);
		while (!textReader.EndOfStream)
		{
			string line = await textReader.ReadLineAsync();
			yield return line;
		}
	}


	private class Builder<TEntry, TResult> :
		SiteRunner.IResultCorrectnessHandlerBuilder<TEntry, TResult>
	{
		private readonly int _year;
		private readonly int _day;
		private readonly int _level;
		private readonly HttpClient _httpClient;
		private readonly AdventOfCode.IRawInputBuilder<TEntry, TResult> _rawInputBuilder;

		public Builder(
			int year,
			int day,
			int level,
			HttpClient httpClient,
			AdventOfCode.IRawInputBuilder<TEntry, TResult> rawInputBuilder
			)
		{
			_year = year;
			_day = day;
			_level = level;
			_httpClient = httpClient;
			_rawInputBuilder = rawInputBuilder;
		}

		public AdventOfCode.IObserversBuilder<TEntry, TResult> ParsingInputWith(
			Func<string, TEntry> lineParser
			)
		{
			return _rawInputBuilder.ParsingInputWith(lineParser);
		}

		public AdventOfCode.IRawInputBuilder<TEntry, TResult> HandlingResultCorrectnessAsync(
			Func<TResult, bool, ValueTask> handler
			)
		{
			// TODO: Allow to customize result formatting

			return new RawInputBuilderProxy<TEntry, TResult>(
				_year,
				_day,
				_level,
				result => result?.ToString() ?? string.Empty,
				_httpClient,
				handler,
				_rawInputBuilder
				);
		}
	}


	private class RawInputBuilderProxy<TEntry, TResult> : AdventOfCode.IRawInputBuilder<TEntry, TResult>
	{
		private readonly int _year;
		private readonly int _day;
		private readonly int _level;
		private readonly Func<TResult, string> _resultFormatter;
		private readonly HttpClient _httpClient;
		private readonly Func<TResult, bool, ValueTask>? _resultCorrectnessHandler;
		private readonly AdventOfCode.IRawInputBuilder<TEntry, TResult> _rawInputBuilder;
		private TResult? _result;
		private bool _resultIsCorrect;
		private AdventOfCode.IObserversBuilder<TEntry, TResult>? _observersBuilder;

		public RawInputBuilderProxy(
			int year,
			int day,
			int level,
			Func<TResult, string> resultFormatter,
			HttpClient httpClient,
			Func<TResult, bool, ValueTask>? resultCorrectnessHandler,
			AdventOfCode.IRawInputBuilder<TEntry, TResult> rawInputBuilder
			)
		{
			_year = year;
			_day = day;
			_level = level;
			_resultFormatter = resultFormatter;
			_httpClient = httpClient;
			_resultCorrectnessHandler = resultCorrectnessHandler;
			_rawInputBuilder = rawInputBuilder;
		}

		public AdventOfCode.IObserversBuilder<TEntry, TResult> ParsingInputWith(Func<string, TEntry> lineParser)
		{
			_observersBuilder = _rawInputBuilder.
				ParsingInputWith(lineParser).
				ObservingAsyncResultWith(VerifyResult).
				AfterRun(HandleResultCorrectness);
			return _observersBuilder;
		}


		private async ValueTask VerifyResult(TResult result)
		{
			var contentValues = new KeyValuePair<string, string>[]
			{
				new("level", _level.ToString()),
				new("answer", _resultFormatter(result))
			};
			var content = new FormUrlEncodedContent(contentValues);

			HttpResponseMessage response = await _httpClient.PostAsync($"/{_year}/day/{_day}/answer", content);
			string responseHtml = await response.Content.ReadAsStringAsync();

			_resultIsCorrect = await FindResultCorrectness(responseHtml, result);
			_result = result;
		}

		private async ValueTask<bool> FindResultCorrectness(string answerHtml, TResult result)
		{
			if (answerHtml.Contains("That's the right answer!")) return true;
			if (answerHtml.Contains("That's not the right answer")) return false;
			if (answerHtml.Contains("You don't seem to be solving the right level.  Did you already complete it?"))
			{
				return await FindResultCorrectnessAtPuzzlePage(result);
			}

			throw new InvalidOperationException(
				"Can not understand if the answer is correct. " +
				"Unexpected answer page content"
				);
		}

		// ReSharper disable once StaticMemberInGenericType
		private static readonly Regex _answerAtPuzzlePageRegex = new(
			@"Your puzzle answer was \<code\>(.*)\</code\>\.",
			RegexOptions.Compiled
			);

		private async ValueTask<bool> FindResultCorrectnessAtPuzzlePage(TResult result)
		{
			string puzzleHtml = await _httpClient.GetStringAsync($"{_year}/day/{_day}");

			string[] answers = _answerAtPuzzlePageRegex.
				Matches(puzzleHtml).
				Select(m => m.Groups[1].Value).
				Take(_level).
				ToArray();
			if (answers.Length < _level)
			{
				throw new InvalidOperationException(
					"Can not understand if the answer is correct. " +
					"Unexpected puzzle page content"
					);
			}

			string resultText = _resultFormatter(result);
			return answers[_level - 1].Equals(resultText, StringComparison.OrdinalIgnoreCase);
		}

		private async ValueTask HandleResultCorrectness()
		{
			if (_resultCorrectnessHandler is null) return;
			if (_result is null) throw new InvalidOperationException("Result is not available yet");
			
			await _resultCorrectnessHandler.Invoke(_result, _resultIsCorrect);
		}
	}
}
