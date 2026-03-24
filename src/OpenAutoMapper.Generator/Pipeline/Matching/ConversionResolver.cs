using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using OpenAutoMapper.Generator.Helpers;
using OpenAutoMapper.Generator.Models;

namespace OpenAutoMapper.Generator.Pipeline.Matching;

/// <summary>
/// Determines the conversion kind between source and destination property types.
/// </summary>
internal static class ConversionResolver
{
    private const string FlagsAttributeName = "System.FlagsAttribute";

    public static ConversionKind DetermineConversion(
        Compilation compilation,
        ITypeSymbol sourceType,
        ITypeSymbol destType)
    {
        // Dictionary detection (before collection check — dictionaries implement IEnumerable)
        if (sourceType.SpecialType != SpecialType.System_String
            && destType.SpecialType != SpecialType.System_String
            && IsDictionaryType(sourceType, out _, out _)
            && IsDictionaryType(destType, out _, out _))
        {
            return ConversionKind.Dictionary;
        }

        // Collection detection (before same-type check — collections must always be copied)
        // Exclude string (which implements IEnumerable<char>)
        if (sourceType.SpecialType != SpecialType.System_String
            && destType.SpecialType != SpecialType.System_String
            && IsCollectionType(sourceType, out _)
            && IsCollectionType(destType, out _))
        {
            return ConversionKind.Collection;
        }

        // Same type
        if (SymbolEqualityComparer.Default.Equals(sourceType, destType))
            return ConversionKind.Direct;

        // Nullable<T> handling
        var sourceIsNullable = IsNullableValueType(sourceType, out var sourceUnderlying);
        var destIsNullable = IsNullableValueType(destType, out var destUnderlying);

        if (sourceIsNullable && !destIsNullable)
        {
            // int? -> int: unwrap
            if (SymbolEqualityComparer.Default.Equals(sourceUnderlying, destType))
                return ConversionKind.NullableUnwrap;

            // int? -> string: ToString
            if (destType.SpecialType == SpecialType.System_String)
                return ConversionKind.ToString;

            // int? -> long: unwrap + convert
            return ConversionKind.NullableUnwrap;
        }

        if (!sourceIsNullable && destIsNullable)
        {
            // int -> int?: wrap (implicit conversion handles this)
            if (SymbolEqualityComparer.Default.Equals(sourceType, destUnderlying))
                return ConversionKind.ImplicitCast;

            // int -> long?: convert + wrap
            return ConversionKind.NullableWrap;
        }

        if (sourceIsNullable && destIsNullable)
        {
            // int? -> int?: same underlying = direct
            if (SymbolEqualityComparer.Default.Equals(sourceUnderlying, destUnderlying))
                return ConversionKind.Direct;

            // int? -> long?: unwrap + convert + wrap
            return ConversionKind.NullableConvert;
        }

        // Check for implicit/explicit conversion via CommonConversion
        var conversionResult = compilation.ClassifyCommonConversion(sourceType, destType);
        if (conversionResult.IsImplicit && !conversionResult.IsUserDefined)
            return ConversionKind.ImplicitCast;

        // Both are enums
        if (sourceType.TypeKind == TypeKind.Enum && destType.TypeKind == TypeKind.Enum)
        {
            // Check for [Flags] on both — use value cast
            if (HasFlagsAttribute(sourceType) && HasFlagsAttribute(destType))
                return ConversionKind.EnumFlagsByValue;

            return ConversionKind.EnumByName;
        }

        // Source is enum, dest is string
        if (sourceType.TypeKind == TypeKind.Enum && destType.SpecialType == SpecialType.System_String)
            return ConversionKind.ToString;

        // Source is string, dest is enum
        if (sourceType.SpecialType == SpecialType.System_String && destType.TypeKind == TypeKind.Enum)
            return ConversionKind.Parse;

        // Dest is string -- use ToString
        if (destType.SpecialType == SpecialType.System_String)
            return ConversionKind.ToString;

        // Explicit conversion exists (exists but not implicit means explicit)
        if (conversionResult.Exists && !conversionResult.IsImplicit)
            return ConversionKind.ExplicitCast;

        // Numeric conversions that require explicit cast
        if (IsNumericType(sourceType) && IsNumericType(destType))
            return ConversionKind.ExplicitCast;

        // Assume nested mapping for complex types
        if (sourceType.TypeKind == TypeKind.Class && destType.TypeKind == TypeKind.Class)
            return ConversionKind.Nested;

        if (sourceType.TypeKind == TypeKind.Struct && destType.TypeKind == TypeKind.Struct
            && sourceType.SpecialType == SpecialType.None && destType.SpecialType == SpecialType.None)
            return ConversionKind.Nested;

        return ConversionKind.Direct;
    }

    public static bool IsCollectionType(ITypeSymbol type, out ITypeSymbol? elementType)
    {
        // Array
        if (type is IArrayTypeSymbol arrayType)
        {
            elementType = arrayType.ElementType;
            return true;
        }

        // Generic collection types
        if (type is INamedTypeSymbol named && named.IsGenericType && named.TypeArguments.Length == 1)
        {
            var originalDef = named.OriginalDefinition.ToDisplayString(
                SymbolDisplayFormat.FullyQualifiedFormat
                    .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted));

            switch (originalDef)
            {
                case "System.Collections.Generic.List<T>":
                case "System.Collections.Generic.IList<T>":
                case "System.Collections.Generic.ICollection<T>":
                case "System.Collections.Generic.IEnumerable<T>":
                case "System.Collections.Generic.IReadOnlyList<T>":
                case "System.Collections.Generic.IReadOnlyCollection<T>":
                case "System.Collections.Generic.HashSet<T>":
                case "System.Collections.Generic.ISet<T>":
                    elementType = named.TypeArguments[0];
                    return true;
            }
        }

        elementType = null;
        return false;
    }

    public static bool IsDictionaryType(ITypeSymbol type, out ITypeSymbol? keyType, out ITypeSymbol? valueType)
    {
        if (type is INamedTypeSymbol named && named.IsGenericType && named.TypeArguments.Length == 2)
        {
            var originalDef = named.OriginalDefinition.ToDisplayString(
                SymbolDisplayFormat.FullyQualifiedFormat
                    .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted));

            switch (originalDef)
            {
                case "System.Collections.Generic.Dictionary<TKey, TValue>":
                case "System.Collections.Generic.IDictionary<TKey, TValue>":
                case "System.Collections.Generic.IReadOnlyDictionary<TKey, TValue>":
                    keyType = named.TypeArguments[0];
                    valueType = named.TypeArguments[1];
                    return true;
            }
        }

        keyType = null;
        valueType = null;
        return false;
    }

    public static CollectionKind DetermineCollectionKind(ITypeSymbol destType)
    {
        if (destType is IArrayTypeSymbol)
            return CollectionKind.Array;

        if (destType is INamedTypeSymbol named && named.IsGenericType)
        {
            var originalDef = named.OriginalDefinition.ToDisplayString(
                SymbolDisplayFormat.FullyQualifiedFormat
                    .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted));

            switch (originalDef)
            {
                case "System.Collections.Generic.HashSet<T>":
                case "System.Collections.Generic.ISet<T>":
                    return CollectionKind.HashSet;
            }
        }

        return CollectionKind.List;
    }

    public static List<EnumMemberPair> MatchEnumMembers(ITypeSymbol sourceEnum, ITypeSymbol destEnum)
    {
        var result = new List<EnumMemberPair>();

        if (sourceEnum is not INamedTypeSymbol srcNamed || destEnum is not INamedTypeSymbol dstNamed)
            return result;

        var srcMembers = srcNamed.GetMembers()
            .OfType<IFieldSymbol>()
            .Where(f => f.HasConstantValue)
            .Select(f => f.Name)
            .ToList();

        var dstMembers = new HashSet<string>(
            dstNamed.GetMembers()
                .OfType<IFieldSymbol>()
                .Where(f => f.HasConstantValue)
                .Select(f => f.Name),
            StringComparer.Ordinal);

        foreach (var srcMember in srcMembers)
        {
            if (dstMembers.Contains(srcMember))
            {
                result.Add(new EnumMemberPair(srcMember, srcMember));
            }
        }

        return result;
    }

    private static bool HasFlagsAttribute(ITypeSymbol type)
    {
        return type.GetAttributes().Any(a =>
        {
            if (a.AttributeClass is null) return false;
            var name = TypeSymbolHelper.GetFullTypeName(a.AttributeClass);
            return string.Equals(name, FlagsAttributeName, StringComparison.Ordinal);
        });
    }

    private static bool IsNullableValueType(ITypeSymbol type, out ITypeSymbol? underlyingType)
    {
        if (type is INamedTypeSymbol named
            && named.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T
            && named.TypeArguments.Length == 1)
        {
            underlyingType = named.TypeArguments[0];
            return true;
        }

        underlyingType = null;
        return false;
    }

    private static bool IsNumericType(ITypeSymbol type)
    {
        switch (type.SpecialType)
        {
            case SpecialType.System_Byte:
            case SpecialType.System_SByte:
            case SpecialType.System_Int16:
            case SpecialType.System_UInt16:
            case SpecialType.System_Int32:
            case SpecialType.System_UInt32:
            case SpecialType.System_Int64:
            case SpecialType.System_UInt64:
            case SpecialType.System_Single:
            case SpecialType.System_Double:
            case SpecialType.System_Decimal:
                return true;
            default:
                return false;
        }
    }
}
