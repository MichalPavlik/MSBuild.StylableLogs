using System.Globalization;
using System.Runtime.Serialization;
using static MSBuild.StylableLogs.Color;

namespace MSBuild.StylableLogs
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Logger logger = new();

            // Logging interpolated string with inline styles (using WithColor extension method) using custom string interpolation handler
            // PROS:
            //      easy to use,
            //      intellisense,
            //      compile time checks,
            //      low complexity,
            //      extensible,
            //      no external dependency needed
            // CONS:
            //      doesn't support easy localization via string format
            logger.Log($"Hello, we support these colors in our logger: {nameof(Red).WithColor(Red)}, {nameof(Green).WithColor(Green)}, {"Blue".WithColor(Blue)}, {"Yellow".WithColor(Yellow)}!");

            // Logging with message builder
            // PROS,CONS: Same as the previous one, it's just a different syntax
            logger.Log(b =>
            {
                b.Append("Hello, we support these colors in our logger: ");
                b.Append(nameof(Red), Red);
                b.Append(", ");
                b.Append(nameof(Green), Green);
                b.Append(", ");
                b.Append("Blue", Blue);
                b.Append(", ");
                b.Append("Yellow", Yellow);
                b.Append("!");
            });

            // Logging string format (localization friendly) with inline styles
            // PROS:
            //      easy to use,
            //      intellisense,
            //      compile time checks,
            //      supports localization via string format
            //      extensible,
            //      no external dependency needed
            // CONS:
            //      requires custom string format implementation - not a big deal, but requires some effort
            logger.Log("Hello, we support these colors in our logger: {0}, {1}, {2}, {3}!", nameof(Red).WithColor(Red), nameof(Green).WithColor(Green), "Blue".WithColor(Blue), "Yellow".WithColor(Yellow));

            // ----- Markup based solutions -----

            // Logging with Spectre.Console markup
            // PROS:
            //      existing solution - low effort
            // CONS:
            //      no intellisense,
            //      no compile time checks,
            //      no extensibility,
            //      no support for localization via string format,
            //      markup must be removed for localization and backward compatibility - Spectre has API for markup removal, but assembly must be loaded in all environments (VS)
            logger.LogSpectreMarkup($"Hello, we support these colors in our logger: [red]{nameof(Red)}[/], [green]{nameof(Green)}[/], [blue]Blue[/], [yellow]Yellow[/]!");

            // Logging with custom markup parser
            // PROS:
            //      no external dependency needed,
            //      supports markup removal for localization and backward compatibility in single pass
            //      extensible
            // CONS:
            //      no intellisense,
            //      no compile time checks,
            //      complexity
            logger.LogCustomMarkup($"Hello, we support these colors in our logger: [red]{nameof(Red)}[/], [green]{nameof(Green)}[/], [blue]Blue[/], [yellow]Yellow[/]!");
        }
    }
}
