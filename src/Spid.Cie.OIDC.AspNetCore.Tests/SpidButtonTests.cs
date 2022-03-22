using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Spid.Cie.OIDC.AspNetCore.Tests;

public class SpidButtonTests
{
    [Fact]
    public async Task SpidCSSTagHelper()
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

        await new Mvc.SpidCSSTagHelper().ProcessAsync(context, tagHelperOutput);
        Assert.NotNull(tagHelperOutput.Content.GetContent());
    }

    [Fact]
    public async Task SpidJSTagHelper()
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

        await new Mvc.SpidJSTagHelper().ProcessAsync(context, tagHelperOutput);
        Assert.NotNull(tagHelperOutput.Content.GetContent());
    }

    [Fact]
    public async Task SpidButtonTagHelper()
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

        var tagHelper = new Mvc.SpidButtonTagHelper(new Mocks.MockIdentityProvidersHandler(false));
        tagHelper.ChallengeUrl = "http://127.0.0.1/";
        await tagHelper.ProcessAsync(context, tagHelperOutput);
        Assert.NotNull(tagHelperOutput.Content.GetContent());
    }

}
