# Advent of Code Solvers Runner

## Overview

This library provides helper API to write and run [Advent of Code](https://adventofcode.com)
puzzles solving code for the .NET platform.

Here is the sample how one can write puzzle solver runner which will 
1. Automatically **load input** from the [adventofcode.com](https://adventofcode.com) site.
2. **Run solver** on the **parsed input**.
3. Automatically **verify at the site the answer** provided by solver.
4. Run assert method to make test green or red.

```c#
[Fact(DisplayName = "Verifies that the puzzle answer is correct")]
public async Task Verifies_that_the_puzzle_answer_is_correct()
{
    var httpClient = new HttpClient();
    var sessionToken = "<your adventofcode.com session token>";

    var runner = new SiteRunner(httpClient, new Uri("https://adventofcode.com"), sessionToken);
    await runner
        .Puzzle(2021, day: 1, level: 1)
        .SolveUsing<Day01Level1Solver>()
        .HandlingResultCorrectness(Assert.True)
        .ParsingInputWith(int.Parse)
        .Run();
}
```

## Installation

Add NuGet package **[AdventOfCode.Runner](https://www.nuget.org/packages/AdventOfCode.Runner)** to your project project:
```
dotnet add package AdventOfCode.Runner
```

## Solver

Solver is the type implementing `AdventOfCode.IAsyncSolver<TEntry, TResult>` interface.

```c#
public interface IAsyncSolver<in TEntry, TResult>
{
    ValueTask<TResult> Solve(IAsyncEnumerable<TEntry> entries);
}
```
The type arguments are described in the next two sections.

### TEntry type parameter

`TEntry` is the type of the input entry which is **parsed from the single line** of the puzzle text input provided by site at the `https://adventofcode.com/<year>/day/<day>/input` page.

*E.g. for the year 2021 day 1 puzzles the site provides the input similar to this:*

```
176
184
188
142
...
```
*It makes sense to parse each line into the `int` value and provide solver with `int` values instead of strings. So the `TEntry` here will be `int`.*

`TEntry` is not necessary has to be the simple type like `int`. It can be any type that you can parse from the single line of the input and that is more appropriate for your solver code.

### TResult type parameter

`TResult` is the type of the answer your solver is calculated. In the vast majority of the cases it will be `int`.

### Solve method

The only method declared by the `IAsyncSolver<in TEntry, TResult>` interface is the `Solve` method. It takes the typed input passed through the argument and calculates and returns answer.

The method is designed as *asynchronous* (returns `ValueTask<TResult>`) to do not restrict you in methods of puzzle solving.

It also takes the `IAsyncEnumerable<TEntry>` of input entries allowing you to deal with any kind of input. Either with one buffered in the memory or with one streamed from the any source. 

Sometimes it is inconvenient to work with streamed input or with asynchronous result. In this case you can add the new base class implementing `IAsyncSolver` and declaring more convenient *Solve*-like method to implement. There is an abstract class [SolverWithArrayInput](https://github.com/vladsm/adventofcode-runner/blob/main/src/Solvers/SolverWithArrayInput.cs) derived from the `IAsyncSolver` which you can use as a base type for your solvers when you don't want deal with asynchronous input and result.

### Sample

Here is the sample of the solver implementation for the fake puzzle which requires to calculate sum of the input numbers:

```c#
public class SumSolver : IAsyncSolver<int, int>
{
    public async ValueTask<int> Solve(IAsyncEnumerable<int> entries)
    {
        return await entries.SumAsync();
    }
}
```

## Solvers Runner concept

The API is based on the concept of a solvers runner represented by `AdventOfCode.Runner<TEntry, TResult>` type.

When created, the runner takes as input and encapsulates a number of services that are called as the runner is running. In this way, the runner can be considered as an orchestrator of the services passed to it.

The runner is started by the Run method:

```c#
public class Runner<TEntry, TResult>
{
...
    public async ValueTask Run() {...}
...
}
```

### Orchestrated services

Services are passed to the runner constructor:

```c#
public class Runner<TEntry, TResult>
{
...
    public Runner(
        IAsyncEnumerable<string> inputLines,
        Func<string, int, TEntry> lineParser,
        IAsyncSolver<TEntry, TResult> solver,
        IEnumerable<IRawInputObserver> rawInputObservers,
        IEnumerable<IInputObserver<TEntry>> inputObservers,
        IEnumerable<IResultObserver<TResult>> resultObservers,
        IEnumerable<Func<ValueTask>> afterRunHandlers
        ) {...}

    public Runner(
        IAsyncEnumerable<TEntry> inputEntries,
        IAsyncSolver<TEntry, TResult> solver,
        IEnumerable<IRawInputObserver> rawInputObservers,
        IEnumerable<IInputObserver<TEntry>> inputObservers,
        IEnumerable<IResultObserver<TResult>> resultObservers,
        IEnumerable<Func<ValueTask>> afterRunHandlers
        ) {...}
...
}
```

The following describes the services that are passed to the runner when it is created, and that are invoked while it is running.

#### Input

The puzzle input is represented in the runner as an async enumerable of typed entries

```c#
IAsyncEnumerable<TEntry> inputEntries
```

or as an async enumerable of raw lines

```c#
IAsyncEnumerable<string> inputLines
```

Only one type of the input (typed or raw) can be passed to the runner. When the raw input is passed, then the *line parser* is required to pass also so that the typed input could be created:

```c#
Func<string, int, TEntry> lineParser
```

- The first argument of the `lineParser` is raw input line to be parsed.
- The second argument is zero-based index of the line in the input lines enumerable.
- Parser returns typed input entry.

#### Solver

```c#
IAsyncSolver<TEntry, TResult> solver
```

Runner calls the solver passing the typed input entries to it and gets the calculated result from it.

#### Input observers

Input observers accept each input entry and handle (one of the task of the input observing is a logging).

There are **raw input observers**:

```c#
IEnumerable<IRawInputObserver> rawInputObservers

public interface IRawInputObserver
{
	ValueTask Observe(string line);
}
```

or **typed input observers**:

```c#
IEnumerable<IInputObserver<TEntry>> inputObservers

public interface IInputObserver<in TEntry>
{
	ValueTask Observe(string line, TEntry entry);
}
```

The observer will be called in an order they follow in the observers enumerable before the solver starts calculating. The raw input observers are called before each line parsing. 


#### Result observers

Result observer is called once the solver completes calculation. Observers will be called in an order they follow in the observers enumerable and will accept the result. 

```c#
IEnumerable<IResultObserver<TResult>> resultObservers

public interface IResultObserver<in TResult>
{
	ValueTask Observe(TResult result);
}
```

#### After-Run handlers

To run something at the end of the processing one can use *after-run* handlers:

```c#
IEnumerable<Func<ValueTask>> afterRunHandlers
```

### Processing

Method `Run` starts asynchronous process of the orchestrated services executing:

```c#
public class Runner<TEntry, TResult>
{
    public async ValueTask Run() {...}
}
```

Providing the input services and observers one can create specific process of obtaining puzzle input and handling the calculated result.

Here is the *lifecycle* of the the `Run` execution:

```
            OPTION 1                        OPTION 2
______________________________     ______________________________

------------------------------     ------------------------------
| Asynchronously enumerating |     | Asynchronously enumerating |
|          raw input         |     |         typed input        |
------------------------------     ------------------------------
              |                                    |
-------------------------------                    |
| Calling raw input observers |                    |
-------------------------------                    |
              |                                    |
  ----------------------------                     |
  | Parsing each line during |                     |
  |     input enumerating    |                     |
  ----------------------------                     |
              |                                    |
--------------------------------                   |
| Calling typed input observer |                   |
--------------------------------                   |
              |____________________________________|
                                |
             -----------------------------------------
             | Passing typed input enumerable to the |
             |             solver.Solve()            |
             -----------------------------------------
                                |
                  -----------------------------
                  | Solver calculating result |
                  -----------------------------
                                |
                   ----------------------------
                   | Calling result observers |
                   ----------------------------
                                |
                  ------------------------------
                  | Calling after-run handlers |
                  ------------------------------
```

## Runner Builder API

To simplify *solver runner* creation and passing services to it there is a fluent API which guides you to build runner.

Look at the following sample:

```c#

static IAsyncEnumerable<string> LoadInputLinesAsync()
{
    // loads input from somewhere...
}

...

private class Year2021Day1Level1Solver : SolverWithArrayInput<int, long>
{
    protected override long Solve(int[] entries)
    {
        return entries.Sum();
    }
}

...

// Build runner and start it
await AdventOfCode
    .SolveUsing(new Year2021Day1Level1Solver())
    .WithInput(LoadInputLinesAsync())
    .ParsingInputWith(int.Parse)
    .ObservingResultWith(result => Console.WriteLine($"Result: {result}"))
    .Run();

```

The fluent API of the builder which is used here is self-explained. Methods of the builder have overrides to be convenient in different scenarios.

Follow the auto-completion in your IDE to explore more building options. 


## Automatic solution verification

Normally to check your solution you have to deal with [adventofcode.com](https://adventofcode.com/):

- first, to **load the input** to provide it to you solving code (most of the time solving code is copying into file and solving code reads from it),
- second, to **submit your answer**.

The library provides extensions of the runner and it's builder to do all these communications with the site automatically.

To use it, you create the instance of the `SiteRunner` type and write the code similar to the following:

```c#
var siteRunner = new SiteRunner(...);

await siteRunner
    .Puzzle(2021, 1, level: 1)
    .SolveUsing<int, int>(typeof(Day1Level1Resolver))
    .HandlingResultCorrectness((result, isResultCorrect) =>
            Console.WriteLine($"Your result {result} is{(isResultCorrect ? "" : " not")} correct"))
    .ParsingInputWith(int.Parse)
    .Run();
```

As you can expect there are overrides and extensions over the `SiteRunner` API.

### Creating site runner instance

To create the instance of the `SiteRunner` type you need to call it's constructor providing the following objects to it:

- Instance of the `System.Net.Http.HttpClient`;
- Site url (*adventofcode.com*);
- Your personal session token to authorized runner calls to the site.

```c#
var siteRunner = new SiteRunner(
    new HttpClient(),
    new Uri("https://adventofcode.com"),
    "<your session token>"
    );
```

#### Session Token

Session token is used to authorize you on [adventofcode.com](https://adventofcode.com) site.

To get it:

- login to the [adventofcode.com](https://adventofcode.com),
- find it in the cookie under the key `session`

```
session=111222...999
```

Do not share your token!

One of the way to keep it secret is to use .net secret manager (see, [Safe storage of app secrets in development in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-6.0&tabs=windows)).

#### HttpClient

Your can configure and create http client in any way that works for you.

You can pre-configure your http client instance to set it's base address to the site url (`https://adventofcode.com`) and to set cookie with session token. In this case you don't need to pass url and session token to the `SiteRunner` constructor.

#### Site Runner lifetime

You can use single instance of the `SiteRunner` over your application.

### Further extensions

`SiteRunner` API can be easily extended to meet your needs.

For example, have a look at the [vladsm/adventofcode-2021-dotnet](https://github.com/vladsm/adventofcode-2021-dotnet) repository where the solution is verifying with the *site runner* and makes unit test green or red ([PuzzlesTestsExtensions.cs](https://github.com/vladsm/adventofcode-2021-dotnet/blob/main/src/AdventOfCode2021.Runner/Helpers/PuzzlesTestsExtensions.cs)).

---
<br/>

_**Happy solving!**_
