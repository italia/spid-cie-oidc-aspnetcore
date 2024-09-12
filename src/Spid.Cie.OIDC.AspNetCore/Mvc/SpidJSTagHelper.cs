using Microsoft.AspNetCore.Razor.TagHelpers;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Mvc;

[HtmlTargetElement("script", Attributes = "spid")]
public class SpidJSTagHelper : TagHelper
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private static string _js;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private static readonly object _lockobj = new object();

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        if (_js == null)
        {
            lock (_lockobj)
            {
                if (_js == null)
                {
                    using var resourceStream = GetType().Assembly.GetManifestResourceStream("Spid.Cie.OIDC.AspNetCore.Mvc.Resources.spid.js");
                    using var reader = new StreamReader(resourceStream!, Encoding.UTF8);
                    _js = reader.ReadToEnd();
                }
            }
        }
        output.Attributes.Remove(output.Attributes["spid"]);
        output.Content.AppendHtml(_js);

        await Task.CompletedTask;
    }
}