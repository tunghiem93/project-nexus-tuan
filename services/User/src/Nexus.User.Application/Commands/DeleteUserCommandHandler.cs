using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Nexus.User.Application.Common.Interfaces;
using Nexus.User.Domain.Entities;
using SendGrid.Helpers.Errors.Model;
using UserEntity = Nexus.User.Domain.Entities.User;

namespace Nexus.User.Application.Commands;

public sealed class DeleteUserCommandHandler
: IRequestHandler<DeleteUserCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUser _currentUser;


    public DeleteUserCommandHandler(
         IUserRepository userRepository,
         ICurrentUser currentUser)
    {
        _userRepository = userRepository;
        _currentUser = currentUser;
    }

    public async Task Handle(
        DeleteUserCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(
            request.UserId,
            cancellationToken);

        // 1. Trả về đúng NotFoundException (mã lỗi 404 chuẩn quy định)
        if (user is null || user.IsDeleted)
        {
            throw new NotFoundException(
                $"User '{request.UserId}' not found.");
        }

        // 2. Kiểm tra phân quyền xóa tài khoản
        var isAdmin = _currentUser.Roles.Contains("Admin");
        var isOwner = _currentUser.UserId == request.UserId;

        if (!isAdmin && !isOwner)
        {
            throw new ForbiddenException(
                "You are not allowed to delete this account.");
        }

        // 3. Sử dụng hàm SoftDelete đóng gói sẵn trong Entity User
        user.SoftDelete();

        // 4. Chỉ cần lưu thay đổi là Entity Framework tự cập nhật vào DB
        await _userRepository.SaveChangesAsync(cancellationToken);
    }
}

