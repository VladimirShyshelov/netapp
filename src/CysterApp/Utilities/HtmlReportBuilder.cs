using System.Net;
using System.Text;
using CysterApp.Models;

namespace CysterApp.Utilities;

public static class HtmlReportBuilder
{
    private const string Css = """
                               <style>
                               tr.green td { background-color:green; }
                               tr.red   td { background-color:red;   }
                               table      { width:100%; font-size:24px; }
                               body       { font-size:24px; }
                               </style>
                               """;

    public static string Build(IEnumerable<Result> rows, string privileges)
    {
        var sb = new StringBuilder();
        sb.Append("<html><head>").Append(Css).Append("</head><body>")
            .Append($"<p><strong>User&nbsp;Privileges:</strong> {privileges}</p>")
            .Append("<table border='1' cellspacing='0' cellpadding='4'>")
            .Append("<tr><th>Pass</th><th>Step</th><th>Result</th><th>Comment</th></tr>");

        foreach (var r in rows)
        {
            var cls = r.Pass ? "green" : "red";
            sb.Append($"<tr class='{cls}'>")
                .Append($"<td>{r.Pass}</td>")
                .Append($"<td>{WebUtility.HtmlEncode(r.Step)}</td>")
                .Append($"<td>{r.Value}</td>")
                .Append($"<td>{WebUtility.HtmlEncode(r.Comment)}</td>")
                .Append("</tr>");
        }

        sb.Append("</table></body></html>");
        return sb.ToString();
    }
}