using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Spid.Cie.OIDC.AspNetCore.Tests;

public class CieButtonTests
{
    [Fact]
    public async Task CieButtonTagHelper()
    {
        var context = new TagHelperContext(
            new TagHelperAttributeList(new List<TagHelperAttribute>() { new TagHelperAttribute("spid") }),
            new Dictionary<object, object>(),
            Guid.NewGuid().ToString("N"));

        var tagHelperOutput = new TagHelperOutput("markdown",
            new TagHelperAttributeList(new List<TagHelperAttribute>() { new TagHelperAttribute("spid") }),
        (result, encoder) =>
        {
            var tagHelperContent = new DefaultTagHelperContent();
            tagHelperContent.SetHtmlContent(string.Empty);
            return Task.FromResult<TagHelperContent>(tagHelperContent);
        });

        var tagHelper = new Mvc.CieButtonTagHelper();
        tagHelper.ChallengeUrl = "http://127.0.0.1:8002/";
        await tagHelper.ProcessAsync(context, tagHelperOutput);
    }

    [Fact]
    public async Task CieButtonTagHelperEmptyCollection()
    {
        var context = new TagHelperContext(
            new TagHelperAttributeList(new List<TagHelperAttribute>() { new TagHelperAttribute("spid") }),
            new Dictionary<object, object>(),
            Guid.NewGuid().ToString("N"));

        var tagHelperOutput = new TagHelperOutput("markdown",
            new TagHelperAttributeList(new List<TagHelperAttribute>() { new TagHelperAttribute("spid") }),
        (result, encoder) =>
        {
            var tagHelperContent = new DefaultTagHelperContent();
            tagHelperContent.SetHtmlContent(string.Empty);
            return Task.FromResult<TagHelperContent>(tagHelperContent);
        });

        var tagHelper = new Mvc.CieButtonTagHelper();
        tagHelper.ChallengeUrl = "http://127.0.0.1:8002/";
        await tagHelper.ProcessAsync(context, tagHelperOutput);
    }

}
