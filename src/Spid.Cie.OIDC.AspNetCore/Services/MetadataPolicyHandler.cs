using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using Spid.Cie.OIDC.AspNetCore.Helpers;
using System;
using System.Linq;

namespace Spid.Cie.OIDC.AspNetCore.Services;

internal class MetadataPolicyHandler : IMetadataPolicyHandler
{
    private readonly ILogger<MetadataPolicyHandler> _logger;

    public MetadataPolicyHandler(ILogger<MetadataPolicyHandler> logger)
    {
        _logger = logger;
    }

    public OpenIdConnectConfiguration? ApplyMetadataPolicy(string opDecodedJwt, string metadataPolicy)
    {
        try
        {
            var openIdConfigurationObj = JObject.Parse(opDecodedJwt)["metadata"]["openid_provider"];
            var metadataPolicyObj = JObject.Parse(metadataPolicy);
            foreach (var property in metadataPolicyObj.Properties())
            {
                var propertyInConf = GetPropertyByName(openIdConfigurationObj, property.Name);
                if (propertyInConf is not null)
                {
                    var policyClauses = property.Values();

                    var valueClause = GetTokenByName(policyClauses, "value");
                    if (valueClause != null)
                    {
                        propertyInConf.Value = valueClause.First();
                        continue;
                    }

                    var addClause = GetTokenByName(policyClauses, "add");
                    if (addClause != null && propertyInConf.Value is JArray)
                    {
                        var propertyInConfValues = (JArray)propertyInConf.Value;
                        var addClauseValue = addClause.First();
                        propertyInConfValues.Merge(addClauseValue);
                    }

                    var defaultClause = GetTokenByName(policyClauses, "default");
                    if (propertyInConf.Value.IsNullOrEmpty() && defaultClause is not null)
                    {
                        propertyInConf.Value = defaultClause.First();
                    }

                    var essentialClause = GetTokenByName(policyClauses, "essential");
                    Throw<InvalidOperationException>.If(essentialClause != null
                            && (bool)essentialClause.First()
                            && propertyInConf.Value.IsNullOrEmpty(),
                        $"Metadata Policy 'essential' clause failed for property '{property.Name}'");

                    var oneOfClause = GetTokenByName(policyClauses, "one_of");
                    var subsetOfClause = GetTokenByName(policyClauses, "subset_of");
                    var supersetOfClause = GetTokenByName(policyClauses, "superset_of");

                    Throw<InvalidOperationException>.If(subsetOfClause is not null && defaultClause is not null
                        && !((JArray)subsetOfClause.First())
                                .Intersect((JArray)defaultClause.First())
                                .SequenceEqual((JArray)defaultClause.First()),
                        "Metadata Policy 'subset_of' clause and 'default' clause mismatch");

                    Throw<InvalidOperationException>.If(supersetOfClause is not null && defaultClause is not null
                        && !((JArray)supersetOfClause.First())
                                .Intersect((JArray)defaultClause.First())
                                .SequenceEqual((JArray)supersetOfClause.First()),
                        "Metadata Policy 'superset_of' clause and 'default' clause mismatch");

                    Throw<InvalidOperationException>.If(subsetOfClause is not null && addClause is not null
                        && !((JArray)subsetOfClause.First())
                                .Intersect((JArray)addClause.First())
                                .SequenceEqual((JArray)addClause.First()),
                        "Metadata Policy 'superset_of' clause and 'add' clause mismatch");

                    Throw<InvalidOperationException>.If(subsetOfClause is not null && supersetOfClause is not null
                        && !((JArray)supersetOfClause.First())
                                .Intersect((JArray)subsetOfClause.First())
                                .SequenceEqual((JArray)supersetOfClause.First()),
                        "Metadata Policy 'superset_of' clause and 'subset_of' clause mismatch");

                    Throw<InvalidOperationException>.If(oneOfClause is not null && addClause is not null
                        && addClause.First() is JArray
                        && !((JArray)oneOfClause.First())
                                .Intersect((JArray)addClause.First())
                                .SequenceEqual((JArray)addClause.First()),
                        "Metadata Policy 'one_of' clause and 'add' clause mismatch");

                    Throw<InvalidOperationException>.If(oneOfClause is not null && addClause is not null
                        && addClause.First() is not JArray
                        && !((JArray)oneOfClause.First())
                                .Contains(addClause.First()),
                        "Metadata Policy 'one_of' clause and 'add' clause mismatch");

                    Throw<InvalidOperationException>.If(oneOfClause is not null && defaultClause is not null
                        && defaultClause.First() is JArray
                        && !((JArray)oneOfClause.First())
                                .Intersect((JArray)defaultClause.First())
                                .SequenceEqual((JArray)defaultClause.First()),
                        "Metadata Policy 'one_of' clause and 'default' clause mismatch");

                    Throw<InvalidOperationException>.If(oneOfClause is not null && defaultClause is not null
                        && defaultClause.First() is not JArray
                        && !((JArray)oneOfClause.First())
                                .Contains(defaultClause.First()),
                        "Metadata Policy 'one_of' clause and 'default' clause mismatch");

                    Throw<InvalidOperationException>.If(oneOfClause is not null && oneOfClause.First() is not JArray,
                        "Metadata Policy 'one_of' clause value is not an array");
                    Throw<InvalidOperationException>.If(oneOfClause is not null
                            && propertyInConf.Value is JArray,
                        $"Metadata Policy 'one_of' clause cannot be applied to property '{property.Name}' since its value is an array");

                    Throw<InvalidOperationException>.If(oneOfClause is not null
                            && !((JArray)oneOfClause.First()).Contains(propertyInConf.Value),
                        $"Metadata Policy 'one_of' clause failed for '{property.Name}'");



                    Throw<InvalidOperationException>.If(subsetOfClause is not null && subsetOfClause.First() is not JArray,
                        "Metadata Policy 'subset_of' clause value is not an array");
                    Throw<InvalidOperationException>.If(subsetOfClause is not null
                            && propertyInConf.Value is not JArray,
                        $"Metadata Policy 'subset_of' clause cannot be applied to property '{property.Name}' since its value is not an array");

                    Throw<InvalidOperationException>.If(subsetOfClause is not null
                            && !((JArray)subsetOfClause.First())
                                .Intersect((JArray)propertyInConf.Value)
                                .SequenceEqual((JArray)propertyInConf.Value),
                        $"Metadata Policy 'subset_of' clause failed for '{property.Name}'");



                    Throw<InvalidOperationException>.If(supersetOfClause is not null && supersetOfClause.First() is not JArray,
                        "Metadata Policy 'superset_of' clause value is not an array");
                    Throw<InvalidOperationException>.If(supersetOfClause is not null
                            && propertyInConf.Value is not JArray,
                        $"Metadata Policy 'superset_of' clause cannot be applied to property '{property.Name}' since its value is not an array");
                    Throw<InvalidOperationException>.If(supersetOfClause is not null
                            && !((JArray)supersetOfClause.First())
                                .Intersect((JArray)propertyInConf.Value)
                                .SequenceEqual((JArray)supersetOfClause.First()),
                        $"Metadata Policy 'superset_of' clause failed for '{property.Name}'");
                }
            }

            var result = OpenIdConnectConfiguration.Create(openIdConfigurationObj.ToString());
            result.JsonWebKeySet = JsonWebKeySet.Create(JObject.Parse(opDecodedJwt)["metadata"]["openid_provider"]["jwks"].ToString());
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"An error occurred while applying the Policy Metadata: {ex.Message}", opDecodedJwt, metadataPolicy);
            return default;
        }
    }

    private JProperty? GetPropertyByName(JToken token, string name)
    {
        return token.FirstOrDefault(p => ((JProperty)p).Name.Equals(name, System.StringComparison.OrdinalIgnoreCase)) as JProperty;
    }

    private JToken? GetTokenByName(IJEnumerable<JToken> token, string name)
    {
        return token.FirstOrDefault(p => ((JProperty)p).Name.Equals(name, System.StringComparison.OrdinalIgnoreCase)) as JProperty;
    }
}


