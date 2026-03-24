using OpenAutoMapper;

namespace OpenAutoMapper.Projection.Tests;

public sealed class ProjectionProfile : Profile
{
    public ProjectionProfile()
    {
        // Flat projection
        CreateProjection<Customer, CustomerDto>();

        // Nested object projection (Customer → CustomerWithAddressDto with Address sub-object)
        CreateProjection<Customer, CustomerWithAddressDto>();
        CreateProjection<Address, AddressDto>();

        // Navigation flattening + computed properties
        // CustomerName is auto-flattened from Customer.Name by convention
        // LineCount is explicitly ignored (complex navigation expression not yet supported by generator)
        CreateProjection<Order, OrderSummaryDto>()
            .Ignore(d => d.LineCount);

        // Nested collection projection
        CreateProjection<Order, OrderWithLinesDto>();
        CreateProjection<OrderLine, OrderLineDto>();
    }
}
