using Riok.Mapperly.Abstractions;

namespace OpenAutoMapper.Benchmarks;

[Mapper]
internal static partial class MapperlyOrderMapper
{
    internal static partial OrderDest MapOrder(OrderSource source);
}

[Mapper]
internal static partial class MapperlyCustomerMapper
{
    internal static partial CustomerDest MapCustomer(CustomerSource source);
    private static partial AddressDest MapAddress(AddressSource source);
}

[Mapper]
internal static partial class MapperlyCollectionMapper
{
    internal static partial OrderWithItemsDest MapOrder(OrderWithItemsSource source);
    private static partial LineItemDest MapItem(LineItemSource source);
}
