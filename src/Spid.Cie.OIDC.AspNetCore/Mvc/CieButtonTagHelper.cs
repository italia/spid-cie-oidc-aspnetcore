using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Spid.Cie.OIDC.AspNetCore.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Mvc;

public class CieButtonTagHelper : TagHelper
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private static string _buttonImage;
    private static readonly object _lockobj = new object();
    private static readonly Dictionary<CieButtonSize, (string ShortClassName, string LongClassName)> _classNames = new()
    {
        { CieButtonSize.Small, ("s", "small") },
        { CieButtonSize.Medium, ("m", "medium") },
        { CieButtonSize.Large, ("l", "large") },
        { CieButtonSize.ExtraLarge, ("xl", "xlarge") }
    };

    private readonly IIdentityProvidersRetriever _idpRetriever;

    public CieButtonTagHelper(IIdentityProvidersRetriever idpRetriever)
    {
        _idpRetriever = idpRetriever;
    }

    public CieButtonSize Size { get; set; }

    public string ChallengeUrl { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.Content.AppendHtml(await CreateHeaderAsync());
    }

    private async Task<TagBuilder> CreateHeaderAsync()
    {
        var spanIcon = new TagBuilder("span");
        spanIcon.AddCssClass("italia-it-button-icon");

        spanIcon.InnerHtml.AppendHtml(GetSerializedButtonImage());

        var spanText = new TagBuilder("span");
        spanText.AddCssClass("italia-it-button-text");
        spanText.InnerHtml.AppendHtml("Entra con CIE");

        var identityProviders = await _idpRetriever.GetIdentityProviders();
        var idp = identityProviders.FirstOrDefault(p => p.Type == Models.IdentityProviderType.CIE);

        var a = new TagBuilder("a");
        if (idp != null)
            a.Attributes.Add("href", $"{ChallengeUrl}{(ChallengeUrl.Contains("?") ? "&" : "?")}provider={idp.Uri}");
        else
            a.Attributes.Add("style", "pointer-events: none;");
        a.AddCssClass($"italia-it-button italia-it-button-size-{_classNames[Size].ShortClassName} button-cie");

        a.InnerHtml.AppendHtml(spanIcon);
        a.InnerHtml.AppendHtml(spanText);
        return a;
    }

    private string GetSerializedButtonImage()
    {
        if (_buttonImage == null)
        {
            lock (_lockobj)
            {
                if (_buttonImage == null)
                {
                    using var resourceStream = GetType().Assembly.GetManifestResourceStream("Spid.Cie.OIDC.AspNetCore.Mvc.Resources.cie-ico-circle-bb.svg");
                    using var writer = new MemoryStream();
                    resourceStream!.CopyTo(writer);
                    writer.Seek(0, SeekOrigin.Begin);
                    _buttonImage = Encoding.UTF8.GetString(writer.ToArray());
                }
            }
        }
        return _buttonImage;
    }

}

public enum CieButtonSize
{
    Small,
    Medium,
    Large,
    ExtraLarge
}
