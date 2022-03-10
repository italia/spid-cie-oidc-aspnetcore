using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using Spid.Cie.OIDC.AspNetCore.Configuration;
using Spid.Cie.OIDC.AspNetCore.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Mvc;

public class SpidProvidersTagHelper : TagHelper
{
    private static string _serializedCircleImage;
    private static readonly object _lockobj = new object();

    private static readonly Dictionary<SpidButtonSize, (string ShortClassName, string LongClassName)> _classNames = new()
    {
        { SpidButtonSize.Small, ("s", "small") },
        { SpidButtonSize.Medium, ("m", "medium") },
        { SpidButtonSize.Large, ("l", "large") },
        { SpidButtonSize.ExtraLarge, ("xl", "xlarge") }
    };
    readonly SpidCieOptions _options;
    readonly IUrlHelper _urlHelper;
    private readonly IIdentityProvidersRetriever _idpRetriever;

    public SpidProvidersTagHelper(IOptionsSnapshot<SpidCieOptions> options, IUrlHelper urlHelper, IIdentityProvidersRetriever idpRetriever)
    {
        _options = options.Value;
        _urlHelper = urlHelper;
        _idpRetriever = idpRetriever;
    }

    public SpidButtonSize Size { get; set; } = SpidButtonSize.Medium;

    public string CircleImagePath { get; set; }

    public string ChallengeUrl { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.Content.AppendHtml(await CreateHeader());
        output.Content.AppendHtml(await CreateButtons());
    }

    private async Task<TagBuilder> CreateHeader()
    {
        var spanIcon = new TagBuilder("span");
        spanIcon.AddCssClass("italia-it-button-icon");

        var imgIcon = new TagBuilder("img");
        imgIcon.Attributes.Add("src", string.IsNullOrWhiteSpace(CircleImagePath) ? GetSerializedCircleImage() : _urlHelper.Content(CircleImagePath));
        imgIcon.Attributes.Add("alt", string.Empty);
        spanIcon.AddCssClass("italia-it-button-icon");
        spanIcon.InnerHtml.AppendHtml(imgIcon);

        var spanText = new TagBuilder("span");
        spanText.AddCssClass("italia-it-button-text");
        spanText.InnerHtml.Append("Entra con SPID");

        var a = new TagBuilder("a");
        a.Attributes.Add("href", "javascript:;");
        a.Attributes.Add("class", $"italia-it-button italia-it-button-size-{_classNames[Size].ShortClassName} button-spid");
        a.Attributes.Add("spid-idp-button", $"#spid-idp-button-{_classNames[Size].LongClassName}-get");
        a.Attributes.Add("aria-haspopup", "true");
        a.Attributes.Add("aria-expanded", "false");

        a.InnerHtml.AppendHtml(spanIcon).AppendHtml(spanText);
        return await Task.FromResult(a);
    }

    private async Task<TagBuilder> CreateButtons()
    {
        var container = new TagBuilder("div");
        container.Attributes.Add("id", $"spid-idp-button-{_classNames[Size].LongClassName}-get");
        container.AddCssClass("spid-idp-button spid-idp-button-tip spid-idp-button-relative");
        var listContainer = new TagBuilder("ul");
        listContainer.Attributes.Add("id", $"spid-idp-list-{_classNames[Size].LongClassName}-root-get");
        listContainer.Attributes.Add("aria-labelledby", "spid-idp");
        listContainer.AddCssClass("spid-idp-button-menu");

        var identityProviders = await _idpRetriever.GetIdentityProviders();
        foreach (var idp in identityProviders)
        {
            var itemContainer = new TagBuilder("li");
            itemContainer.AddCssClass("spid-idp-button-link");
            itemContainer.Attributes.Add("data-idp", idp.Name);

            var item = new TagBuilder("a");
            item.Attributes.Add("href", $"{ChallengeUrl}{(ChallengeUrl.Contains("?") ? "&" : "?")}idp={idp.Name}");

            var span = new TagBuilder("span");
            span.AddCssClass("spid-sr-only");
            span.InnerHtml.Append(idp.OrganizationDisplayName);

            var img = new TagBuilder("img");
            img.Attributes.Add("src", idp.OrganizationLogoUrl);
            img.Attributes.Add("alt", idp.Name);
            span.InnerHtml.Append(idp.OrganizationDisplayName);

            item.InnerHtml.AppendHtml(span).AppendHtml(img);
            itemContainer.InnerHtml.AppendHtml(item);
            listContainer.InnerHtml.AppendHtml(itemContainer);
        }
        container.InnerHtml.AppendHtml(listContainer);
        return container;
    }

    private string GetSerializedCircleImage()
    {
        if (_serializedCircleImage == null)
        {
            lock (_lockobj)
            {
                if (_serializedCircleImage == null)
                {

                    using (var resourceStream = GetType().Assembly.GetManifestResourceStream("Spid.Cie.OIDC.AspNetCore.Mvc.Resources.spid-ico-circle-bb.png"))
                    {
                        using (var writer = new MemoryStream())
                        {
                            resourceStream.CopyTo(writer);
                            writer.Seek(0, SeekOrigin.Begin);
                            _serializedCircleImage = $"data:image/png;base64,{Convert.ToBase64String(writer.ToArray())}";
                        }
                    }
                }
            }
        }
        return _serializedCircleImage;
    }

}

public enum SpidButtonSize
{
    Small,
    Medium,
    Large,
    ExtraLarge
}
