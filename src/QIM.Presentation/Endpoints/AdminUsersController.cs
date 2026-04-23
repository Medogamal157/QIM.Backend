using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QIM.Application.DTOs.Admin;
using QIM.Application.Features.AdminUsers;

namespace QIM.Presentation.Endpoints;

[Route("api/admin/users")]
[Authorize(Roles = "SuperAdmin")]
public class AdminUsersController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public AdminUsersController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => FromResult(await _mediator.Send(new GetAllAdminUsersQuery()));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAdminUserRequest request)
        => FromResult(await _mediator.Send(new CreateAdminUserCommand(request)));

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateAdminUserRequest request)
        => FromResult(await _mediator.Send(new UpdateAdminUserCommand(id, request)));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
        => FromResult(await _mediator.Send(new DeleteAdminUserCommand(id)));

    [HttpPatch("{id}/change-role")]
    public async Task<IActionResult> ChangeRole(string id, [FromBody] ChangeAdminRoleRequest request)
        => FromResult(await _mediator.Send(new ChangeAdminRoleCommand(id, request)));
}
