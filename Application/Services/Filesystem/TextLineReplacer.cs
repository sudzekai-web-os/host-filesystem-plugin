using System.Net;
using System.Text;

namespace Application.Services.Filesystem
{
    internal static class TextLineReplacer
    {
        public static string Replace(string content, IReadOnlyDictionary<int, string> replacements)
        {
            if (replacements.Any(pair => pair.Key < 1 || pair.Value == null || pair.Value.AsSpan().IndexOfAny('\r', '\n') >= 0))
                throw new FilesystemException(HttpStatusCode.BadRequest, "Номера строк начинаются с 1, замена не может содержать перевод строки.");

            var result = new StringBuilder(content.Length);
            var line = 0;
            var position = 0;
            while (position < content.Length)
            {
                var end = content.AsSpan(position).IndexOfAny('\r', '\n');
                end = end < 0 ? content.Length : position + end;

                if (replacements.TryGetValue(++line, out var replacement))
                    result.Append(replacement);
                else
                    result.Append(content.AsSpan(position, end - position));

                var next = end;
                if (next < content.Length && content[next++] == '\r' && next < content.Length && content[next] == '\n')
                    next++;

                result.Append(content.AsSpan(end, next - end));
                position = next;
            }

            if (replacements.Keys.Any(number => number > line))
                throw new FilesystemException(HttpStatusCode.BadRequest, "Указанной строки в файле нет.");

            return result.ToString();
        }
    }
}
