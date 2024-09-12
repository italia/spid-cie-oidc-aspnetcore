using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Newtonsoft.Json.Linq;
using Spid.Cie.OIDC.AspNetCore.Helpers;
using System;
using System.Linq;

namespace Spid.Cie.OIDC.AspNetCore.Services;

class MetadataPolicyHandler : IMetadataPolicyHandler
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
            // For each property contained in the policy
            foreach (var property in metadataPolicyObj.Properties())
            {
                // Find the corresponding property in the metadata
                var propertyInConf = GetPropertyByName(openIdConfigurationObj, property.Name);
                if (propertyInConf is not null)
                {
                    // If there is a value operator in the policy, apply that and you are done.
                    var policyClauses = property.Values();

                    var valueClause = GetTokenByName(policyClauses, "value");
                    if (valueClause != null)
                    {
                        propertyInConf.Value = valueClause.First();
                        continue;
                    }

                    // Add whatever value is specified in an add operator.
                    var addClause = GetTokenByName(policyClauses, "add");
                    if (addClause != null && propertyInConf.Value is JArray)
                    {
                        var propertyInConfValues = (JArray)propertyInConf.Value;
                        var addClauseValue = addClause.First();
                        propertyInConfValues.Merge(addClauseValue);
                    }

                    // If the parameter still has no value apply the default if there is one.
                    var defaultClause = GetTokenByName(policyClauses, "default");
                    if (propertyInConf.Value.IsNullOrWhiteSpace() && defaultClause is not null)
                    {
                        propertyInConf.Value = defaultClause.First();
                    }

                    // Do the essential check. If essential is missing as an operator essential is to be treated as if set to false.
                    // If essential is defined to be true, then the claim MUST have a value by now. Otherwise applying the operator MUST fail.
                    var essentialClause = GetTokenByName(policyClauses, "essential");
                    Throw<InvalidOperationException>.If(essentialClause != null
                            && (bool)essentialClause.First()
                            && propertyInConf.Value.IsNullOrWhiteSpace(),
                        $"Metadata Policy 'essential' clause failed for property '{property.Name}'");

                    // Do the other checks.
                    var oneOfClause = GetTokenByName(policyClauses, "one_of");
                    var subsetOfClause = GetTokenByName(policyClauses, "subset_of");
                    var supersetOfClause = GetTokenByName(policyClauses, "superset_of");

                    Throw<InvalidOperationException>.If(oneOfClause is not null && (subsetOfClause is not null || supersetOfClause is not null),
                        "Metadata Policy 'one_of' clause cannot appear beside 'subset_of'/'superset_of' in a policy entry.");

                    // If default appears in a policy entry together with subset_of then the values of default MUST be a subset of subset_of.
                    Throw<InvalidOperationException>.If(subsetOfClause is not null && defaultClause is not null
                        && !((JArray)subsetOfClause.First())
                                .Intersect((JArray)defaultClause.First())
                                .SequenceEqual((JArray)defaultClause.First()),
                        "Metadata Policy 'subset_of' clause and 'default' clause mismatch");

                    // If default appears in a policy entry together with superset_of then the values of default MUST be a superset of superset_of.
                    Throw<InvalidOperationException>.If(supersetOfClause is not null && defaultClause is not null
                        && !((JArray)supersetOfClause.First())
                                .Intersect((JArray)defaultClause.First())
                                .SequenceEqual((JArray)supersetOfClause.First()),
                        "Metadata Policy 'superset_of' clause and 'default' clause mismatch");

                    // If add appears in a policy entry together with subset_of then the value/values of add MUST be a subset of subset_of.
                    Throw<InvalidOperationException>.If(subsetOfClause is not null && addClause is not null
                        && !((JArray)subsetOfClause.First())
                                .Intersect((JArray)addClause.First())
                                .SequenceEqual((JArray)addClause.First()),
                        "Metadata Policy 'subset_of' clause and 'add' clause mismatch");

                    // If add appears in a policy entry together with superset_of then the values of add MUST be a superset of superset_of.
                    Throw<InvalidOperationException>.If(supersetOfClause is not null && addClause is not null
                        && !((JArray)supersetOfClause.First())
                                .Intersect((JArray)addClause.First())
                                .SequenceEqual((JArray)supersetOfClause.First()),
                        "Metadata Policy 'superset_of' clause and 'add' clause mismatch");

                    // If subset_of and superset_of both appear as operators, then the list of values in subset_of MUST be a superset of the values in superset_of.
                    Throw<InvalidOperationException>.If(subsetOfClause is not null && supersetOfClause is not null
                        && !((JArray)supersetOfClause.First())
                                .Intersect((JArray)subsetOfClause.First())
                                .SequenceEqual((JArray)supersetOfClause.First()),
                        "Metadata Policy 'superset_of' clause and 'subset_of' clause mismatch");

                    // If add appears in a policy entry together with one_of then the value of add MUST be a member of one_of.
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

                    // If default appears in a policy entry together with one_of then the value default MUST be a member of one_of.
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

                    // 'add' clause. The value of the parameter MUST be one of the ones listed in this directive.
                    Throw<InvalidOperationException>.If(oneOfClause is not null && oneOfClause.First() is not JArray,
                        "Metadata Policy 'one_of' clause value is not an array");
                    Throw<InvalidOperationException>.If(oneOfClause is not null
                            && propertyInConf.Value is JArray,
                        $"Metadata Policy 'one_of' clause cannot be applied to property '{property.Name}' since its value is an array");
                    Throw<InvalidOperationException>.If(oneOfClause is not null
                            && !((JArray)oneOfClause.First()).Contains(propertyInConf.Value),
                        $"Metadata Policy 'one_of' clause failed for '{property.Name}'");

                    // 'subset_of' clause. The resulting value of the parameter will be the intersection of the values in the directive
                    // and the values of the parameter.
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

                    // 'superset_of' clause. The values of the parameter MUST contain the ones in the directive.
                    // We define superset the mathematical way, that is, equality is included.
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
                else
                {
                    var policyClauses = property.Values();
                    var essentialClause = GetTokenByName(policyClauses, "essential");
                    Throw<InvalidOperationException>.If(essentialClause != null
                            && (bool)essentialClause.First(),
                        $"Metadata Policy 'essential' clause failed for property '{property.Name}'");
                }
            }

            return OpenIdConnectConfiguration.Create(openIdConfigurationObj.ToString());
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