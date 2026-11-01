using Nutrihabitocc.Domain.Dtos;

namespace Nutrihabitocc.Domain.Ports.Outputs;

public interface ICurrentUserService
{
    CurrentUser GetCurrentUser();
}
