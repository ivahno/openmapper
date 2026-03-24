using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using OpenAutoMapper.Generator.Helpers;
using OpenAutoMapper.Generator.Models;

namespace OpenAutoMapper.Generator.Pipeline.Matching;

/// <summary>
/// Creates PropertyMatchDescriptor instances from type information.
/// </summary>
internal static class PropertyMatchFactory
{
    public static PropertyMatchDescriptor CreatePropertyMatch(
        Compilation compilation,
        string sourcePropertyName,
        ITypeSymbol sourceType,
        IPropertySymbol destProp,
        ConversionKind convKind)
    {
        var sourceFullType = TypeSymbolHelper.GetFullTypeName(sourceType);
        var destFullType = TypeSymbolHelper.GetFullTypeName(destProp.Type);
        var isInitOnly = destProp.SetMethod is not null && destProp.SetMethod.IsInitOnly;

        if (convKind == ConversionKind.Collection)
        {
            ConversionResolver.IsCollectionType(sourceType, out var srcElement);
            ConversionResolver.IsCollectionType(destProp.Type, out var dstElement);

            var destCollectionKind = ConversionResolver.DetermineCollectionKind(destProp.Type);

            return new PropertyMatchDescriptor(
                sourcePropertyName,
                sourceFullType,
                destProp.Name,
                destFullType,
                convKind,
                srcElement is not null ? TypeSymbolHelper.GetFullTypeName(srcElement) : null,
                dstElement is not null ? TypeSymbolHelper.GetFullTypeName(dstElement) : null,
                destCollectionKind,
                null, null, null, null, null, null, null, null, null,
                isInitOnly);
        }

        if (convKind == ConversionKind.Dictionary)
        {
            ConversionResolver.IsDictionaryType(sourceType, out _, out _);
            ConversionResolver.IsDictionaryType(destProp.Type, out var destKeyType, out var destValueType);

            return new PropertyMatchDescriptor(
                sourcePropertyName,
                sourceFullType,
                destProp.Name,
                destFullType,
                convKind,
                null,
                null,
                CollectionKind.None,
                null,
                destKeyType is not null ? TypeSymbolHelper.GetFullTypeName(destKeyType) : null,
                destValueType is not null ? TypeSymbolHelper.GetFullTypeName(destValueType) : null,
                null, null, null, null, null, null,
                isInitOnly);
        }

        if (convKind == ConversionKind.EnumByName)
        {
            var enumMembers = ConversionResolver.MatchEnumMembers(sourceType, destProp.Type);
            if (enumMembers.Count > 0)
            {
                return new PropertyMatchDescriptor(
                    sourcePropertyName,
                    sourceFullType,
                    destProp.Name,
                    destFullType,
                    convKind,
                    null,
                    null,
                    CollectionKind.None,
                    new EquatableArray<EnumMemberPair>(enumMembers.ToImmutableArray()),
                    null, null, null, null, null, null, null, null,
                    isInitOnly);
            }
        }

        return new PropertyMatchDescriptor(
            sourcePropertyName,
            sourceFullType,
            destProp.Name,
            destFullType,
            convKind,
            null, null, CollectionKind.None, null, null, null,
            null, null, null, null, null, null,
            isInitOnly);
    }

    public static PropertyMatchDescriptor WithMemberConfig(
        PropertyMatchDescriptor match,
        MemberConfigReference config)
    {
        if (config.ConditionExpression is null
            && config.PreConditionExpression is null
            && config.NullSubstituteExpression is null
            && config.ValueResolverTypeName is null
            && config.MemberValueResolverTypeName is null)
            return match;

        return new PropertyMatchDescriptor(
            match.SourcePropertyName,
            match.SourcePropertyType,
            match.DestPropertyName,
            match.DestPropertyType,
            match.ConversionKind,
            match.SourceElementType,
            match.DestElementType,
            match.DestCollectionKind,
            match.EnumMembers,
            match.DictKeyType,
            match.DictValueType,
            config.ConditionExpression,
            config.PreConditionExpression,
            config.NullSubstituteExpression,
            config.ValueResolverTypeName,
            match.DestPathIntermediateTypes,
            config.MemberValueResolverTypeName,
            match.IsInitOnly);
    }
}
