#nullable enable

using System;

namespace OpenAutoMapper;

public interface IMapper
{
    TDestination Map<TDestination>(object source);

    TDestination Map<TSource, TDestination>(TSource source);

    TDestination Map<TSource, TDestination>(TSource source, TDestination destination);

    object Map(object source, Type sourceType, Type destinationType);

    object Map(object source, object destination, Type sourceType, Type destinationType);

    TDestination Map<TSource, TDestination>(TSource source, string mappingName);
}
