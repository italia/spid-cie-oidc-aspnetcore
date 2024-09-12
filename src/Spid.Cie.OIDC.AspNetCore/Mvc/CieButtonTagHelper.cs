using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.IO;

namespace Spid.Cie.OIDC.AspNetCore.Mvc;

public class CieButtonTagHelper : TagHelper
{
    static string? _buttonImage;
    readonly static object _lockobj = new();
    readonly static Dictionary<CieButtonSize, string> _classNames = new()
        {
            { CieButtonSize.Small, "150" },
            { CieButtonSize.Medium, "220" },
            { CieButtonSize.Large, "280" },
            { CieButtonSize.ExtraLarge, "340" }
        };

    public CieButtonSize Size { get; set; }

    public string ChallengeUrl { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.Content.AppendHtml(CreateHeader());
    }

    private TagBuilder CreateHeader()
    {
        var spanIcon = new TagBuilder("span");

        var imgIcon = new TagBuilder("img");
        imgIcon.Attributes.Add("src", GetSerializedButtonImage());
        imgIcon.Attributes.Add("alt", "Entra con CIE");
        imgIcon.Attributes.Add("style", $"width: {_classNames[Size]}px;");
        spanIcon.InnerHtml.AppendHtml(imgIcon);

        var a = new TagBuilder("a");
        a.Attributes.Add("href", ChallengeUrl);

        a.InnerHtml.AppendHtml(spanIcon);
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

                    using var resourceStream = GetType().Assembly.GetManifestResourceStream("Spid.Cie.OIDC.AspNetCore.Mvc.Resources.cie-button.png");
                    using var writer = new MemoryStream();
                    resourceStream.CopyTo(writer);
                    writer.Seek(0, SeekOrigin.Begin);
                    _buttonImage = $"data:image/png;base64,{Convert.ToBase64String(writer.ToArray())}";
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