using HotChocolate.Types;
using Pactly.Core.Domain;

namespace Pactly.Api.GraphQL;

public class UserType : ObjectType<User>
{
    protected override void Configure(IObjectTypeDescriptor<User> descriptor)
    {
        descriptor.Ignore(u => u.PasswordHash);
    }
}
