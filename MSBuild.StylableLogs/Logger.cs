using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MSBuild.StylableLogs
{
    internal static class Extensions
    {
        public static ColoredTextSegment WithColor<T>(this T input, Color color)
           => new ColoredTextSegment(color, input == null ? string.Empty : input.ToString());
    }

    internal enum Color
    {
        Default,
        Red,
        Green,
        Blue,
        Yellow
    }

    internal readonly struct ColoredTextSegment
    {
        public Color Color { get; }

        public string? Text { get; }

        public ColoredTextSegment(Color color, string? text)
        {
            Color = color;
            Text = text;
        }

        override public string ToString() => Text ?? string.Empty;
    }

    internal class Logger
    {
        private readonly Dictionary<Color, string> colorToVT100 = new Dictionary<Color, string>
        {
            { Color.Default, "\x1b[39m" },
            { Color.Red, "\x1b[31m" },
            { Color.Green, "\x1b[32m" },
            { Color.Blue, "\x1b[34m" },
            { Color.Yellow, "\x1b[33m" }
        };

        public void Log(LogStringMetadataHandler message)
        {
            ImmutableArray<(int, Color)> indexes = message.GetMetadata();
            Write(message.GetText(), indexes);
        }

        public void Log(Action<StringMetadataBuilder> messageBuilder)
        {
            StringMetadataBuilder builder = new StringMetadataBuilder();
            messageBuilder(builder);
            Write(builder.GetText(), builder.GetMetadata());
        }

        public void Log(string format, params object[] args)
        {
            ImmutableArray<(int index, Color color)>.Builder metadataBuilder = ImmutableArray.CreateBuilder<(int, Color)>();
            StringMetadataFormatter formatter = new StringMetadataFormatter();

            string formattedString = formatter.Format(CultureInfo.InvariantCulture, format, args, metadataBuilder);

            Write(formattedString, metadataBuilder.ToImmutableArray());
        }

        public void LogSpectreMarkup(string content)
        {
            AnsiConsole.Write(new Markup(content));
            Console.WriteLine();
        }

        public void LogCustomMarkup(string content)
        {
            ImmutableArray<(int index, Color color)>.Builder metadataBuilder = ImmutableArray.CreateBuilder<(int, Color)>();
            string text = MarkupParser.Parse(content, metadataBuilder);

            Write(text, metadataBuilder.ToImmutableArray());
        }

        private void Write(string text, ImmutableArray<(int, Color)> metadata)
        {
            ReadOnlySpan<char> textSpan = text.AsSpan();
            int currentPosition = 0;

            foreach ((int Index, Color Color) entry in metadata)
            {
                Console.Write(colorToVT100[entry.Color]);

                ReadOnlySpan<char> span = textSpan.Slice(currentPosition, entry.Index - currentPosition);
                Console.Out.Write(span);

                currentPosition = entry.Index;
            }

            Console.WriteLine();
        }
    }

    internal readonly struct StringMetadataBuilder
    {
        private readonly ImmutableArray<(int length, Color color)>.Builder metadataBuilder;
        private readonly StringBuilder stringBuilder = new StringBuilder();

        public StringMetadataBuilder()
        {
            metadataBuilder = ImmutableArray.CreateBuilder<(int length, Color color)>();
        }

        public StringMetadataBuilder Append<T>(T content, Color color = Color.Default)
        {
            stringBuilder.Append(content);
            metadataBuilder.Add((stringBuilder.Length, color));

            return this;
        }

        internal ImmutableArray<(int length, Color color)> GetMetadata() => metadataBuilder.ToImmutableArray();

        internal string GetText() => stringBuilder.ToString();
    }

    [InterpolatedStringHandler]
    internal ref struct LogStringMetadataHandler
    {
        private readonly ImmutableArray<(int length, Color color)>.Builder metadataBuilder;
        private readonly StringBuilder stringBuilder = new StringBuilder();

        public LogStringMetadataHandler(int literalLength, int formattedCount)
        {
            metadataBuilder = ImmutableArray.CreateBuilder<(int length, Color color)>();
        }

        public void AppendLiteral(string s)
        {
            stringBuilder.Append(s);
            metadataBuilder.Add((stringBuilder.Length, Color.Default));
        }

        public void AppendFormatted<T>(T t)
        {
            Color color = t is ColoredTextSegment coloredTextSegment
                ? coloredTextSegment.Color
                : Color.Default;

            stringBuilder.Append(t);
            metadataBuilder.Add((stringBuilder.Length, color));
        }

        internal ImmutableArray<(int length, Color color)> GetMetadata() => metadataBuilder.ToImmutableArray();

        internal string GetText() => stringBuilder.ToString();
    }
}
