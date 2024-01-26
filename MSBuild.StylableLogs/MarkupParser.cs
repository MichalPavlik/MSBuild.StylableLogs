using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MSBuild.StylableLogs
{
    internal class MarkupParser
    {
        public static string Parse(string input, ImmutableArray<(int index, Color color)>.Builder metadataBuilder)
        {
            StringBuilder sb = new StringBuilder();
            int i = 0;

            while (i < input.Length)
            {
                if (input[i] == '[')
                {
                    int start = i;
                    int end = input.IndexOf(']', start);
                    if (end == -1)
                    {
                        break;
                    }

                    string name = input.Substring(start + 1, end - start - 1);
                    int contentStart = end + 1;
                    int contentEnd = input.IndexOf("[/]", contentStart);
                    if (contentEnd == -1) break;

                    metadataBuilder.Add((sb.Length, Color.Default));
                    sb.Append(input.Substring(contentStart, contentEnd - contentStart));
                    if (Color.Default != Enum.Parse<Color>(name, true))
                    {
                        metadataBuilder.Add((sb.Length, Enum.Parse<Color>(name, true)));
                    }

                    i = contentEnd + 3;
                }
                else
                {
                    sb.Append(input[i]);
                    i++;
                }
            }

            metadataBuilder.Add((sb.Length, Color.Default));

            return sb.ToString();
        }
    }
}