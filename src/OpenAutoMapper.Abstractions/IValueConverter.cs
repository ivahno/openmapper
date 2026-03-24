#nullable enable

namespace OpenAutoMapper;

/// <summary>
/// Converts a source member value to a destination member value.
/// </summary>
public interface IValueConverter<in TSourceMember, TDestMember>
{
    TDestMember Convert(TSourceMember sourceMember, ResolutionContext context);
}
