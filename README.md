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
