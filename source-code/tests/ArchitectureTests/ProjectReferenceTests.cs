using Xunit;
using System.Text.RegularExpressions;
using FluentAssertions;

/// <summary>
/// Reference-graph rules from the dotnet-ddd-architecture skill, checked on the .csproj files.
/// Contexts are discovered from the folder layout (a folder holding &lt;Ctx&gt;.Domain), so a new
/// context is covered the moment it is created — there is no list to forget to update.
/// </summary>
public class ProjectReferenceTests
{
    private static readonly string SolutionRoot = FindSolutionRoot();

    private static readonly string[] Contexts = Directory
        .GetDirectories(SolutionRoot)
        .Where(dir => Path.GetFileName(dir) != "Common") // the shared kernel is not a context
        .Where(dir => Directory.Exists(Path.Combine(dir, $"{Path.GetFileName(dir)}.Domain")))
        .Select(dir => Path.GetFileName(dir))
        .ToArray();

    [Fact]
    public void No_Project_Of_A_Context_References_Another_Context()
    {
        var offenders = new List<string>();

        foreach (var context in Contexts)
        {
            foreach (var csproj in ContextProjects(context))
            {
                foreach (var reference in References(csproj))
                {
                    var foreign = Contexts.FirstOrDefault(other =>
                        other != context && reference.StartsWith($"{other}.", StringComparison.Ordinal));
                    if (foreign is not null)
                    {
                        offenders.Add($"{Path.GetFileName(csproj)} -> {reference}");
                    }
                }
            }
        }

        offenders.Should().BeEmpty("a context may never reference another context");
    }

    [Fact]
    public void Domain_Projects_Reference_Only_Common_Domain()
    {
        var offenders = Contexts
            .SelectMany(c => References(ProjectFile(c, "Domain")).Select(r => (c, r)))
            .Where(x => x.r != "Common.Domain")
            .Select(x => $"{x.c}.Domain -> {x.r}");

        offenders.Should().BeEmpty();
    }

    [Fact]
    public void Application_Projects_Never_Reference_Infrastructure_Or_Web()
    {
        var offenders = Contexts
            .SelectMany(c => References(ProjectFile(c, "Application")).Select(r => (c, r)))
            .Where(x => x.r.EndsWith(".Infrastructure") || x.r.EndsWith(".Web"))
            .Select(x => $"{x.c}.Application -> {x.r}");

        offenders.Should().BeEmpty();
    }

    [Fact]
    public void Web_Projects_Never_Reference_Infrastructure()
    {
        var offenders = Contexts
            .SelectMany(c => References(ProjectFile(c, "Web")).Select(r => (c, r)))
            .Where(x => x.r.EndsWith(".Infrastructure"))
            .Select(x => $"{x.c}.Web -> {x.r}");

        offenders.Should().BeEmpty("the host wires Infrastructure; controllers only know MediatR requests");
    }

    private static IEnumerable<string> ContextProjects(string context)
        => new[] { "Domain", "Application", "Infrastructure", "Web" }
            .Select(layer => ProjectFile(context, layer))
            .Where(File.Exists);

    private static string ProjectFile(string context, string layer)
        => Path.Combine(SolutionRoot, context, $"{context}.{layer}", $"{context}.{layer}.csproj");

    private static IEnumerable<string> References(string csproj)
        => !File.Exists(csproj)
            ? []
            : Regex.Matches(File.ReadAllText(csproj), @"<ProjectReference\s+Include=""[^""]*?([\w.]+)\.csproj""")
                .Select(m => m.Groups[1].Value);

    private static string FindSolutionRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !dir.GetFiles("*.sln").Any())
        {
            dir = dir.Parent;
        }

        return dir?.FullName ?? throw new InvalidOperationException("Solution root not found.");
    }
}
