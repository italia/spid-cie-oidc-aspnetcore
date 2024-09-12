using Microsoft.AspNetCore.Razor.TagHelpers;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Mvc;

[HtmlTargetElement("style", Attributes = "spid")]
public class SpidCSSTagHelper : TagHelper
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private static string _css;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private static readonly object _lockobj = new object();

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        if (_css == null)
        {
            lock (_lockobj)
            {
                if (_css == null)
                {
                    using var resourceStream = GetType().Assembly.GetManifestResourceStream("Spid.Cie.OIDC.AspNetCore.Mvc.Resources.spid.css");
                    using var reader = new StreamReader(resourceStream!, Encoding.UTF8);
                    _css = reader.ReadToEnd();
                }
            }
        }
        output.Content.AppendHtml(_css);
        output.Attributes.Remove(output.Attributes["spid"]);

        await Task.CompletedTask;
    }
}