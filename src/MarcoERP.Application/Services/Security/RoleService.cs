using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MarcoERP.Application.Common;
using MarcoERP.Application.DTOs.Security;
using MarcoERP.Application.Interfaces;
using MarcoERP.Application.Interfaces.Security;
using MarcoERP.Application.Mappers.Security;
using MarcoERP.Domain.Entities.Security;
using MarcoERP.Domain.Enums;
using MarcoERP.Domain.Interfaces;
using MarcoERP.Domain.Interfaces.Security;

namespace MarcoERP.Application.Services.Security
{
    /// <summary>
    /// Application service for role management.
    /// Supports CRUD operations with permission management.
    /// </summary>
    [Module(SystemModule.Security)]
    public sealed class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly IValidator<CreateRoleDto> _createValidator;
        private readonly IValidator<UpdateRoleDto> _updateValidator;

        public RoleService(
            IRoleRepository roleRepo,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            IValidator<CreateRoleDto> createValidator,
            IValidator<UpdateRoleDto> updateValidator)
        {
            _roleRepo = roleRepo ?? throw new ArgumentNullException(nameof(roleRepo));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _createValidator = createValidator ?? throw new ArgumentNullException(nameof(createValidator));
            _updateValidator = updateValidator ?? throw new ArgumentNullException(nameof(updateValidator));
        }

        public async Task<ServiceResult<IReadOnlyList<RoleListDto>>> GetAllAsync(CancellationToken ct = default)
        {
            var entities = await _roleRepo.GetAllWithPermissionsAsync(ct);
            return ServiceResult<IReadOnlyList<RoleListDto>>.Success(
                entities.Select(RoleMapper.ToListDto).ToList());
        }

        public async Task<ServiceResult<RoleDto>> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var entity = await _roleRepo.GetByIdWithPermissionsAsync(id, ct);
            if (entity == null)
                return ServiceResult<RoleDto>.Failure("الدور غير موجود.");
            return ServiceResult<RoleDto>.Success(RoleMapper.ToDto(entity));
        }

        public async Task<ServiceResult<RoleDto>> CreateAsync(CreateRoleDto dto, CancellationToken ct = default)
        {
            var authCheck = AuthorizationGuard.Check<RoleDto>(_currentUser, PermissionKeys.RolesManage);
            if (authCheck != null) return authCheck;

            var vr = await _createValidator.ValidateAsync(dto, ct);
            if (!vr.IsValid)
                return ServiceResult<RoleDto>.Failure(vr.Errors[0].ErrorMessage);

            if (await _roleRepo.NameExistsAsync(dto.NameEn ?? dto.NameAr, null, ct))
                return ServiceResult<RoleDto>.Failure("اسم الدور موجود مسبقاً.");

            var entity = new Role(dto.NameAr, dto.NameEn ?? dto.NameAr, dto.Description);
            entity.SetPermissions(dto.Permissions ?? new List<string>());

            await _roleRepo.AddAsync(entity, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            var saved = await _roleRepo.GetByIdWithPermissionsAsync(entity.Id, ct);
            return ServiceResult<RoleDto>.Success(RoleMapper.ToDto(saved));
        }

        public async Task<ServiceResult<RoleDto>> UpdateAsync(UpdateRoleDto dto, CancellationToken ct = default)
        {
            var authCheck = AuthorizationGuard.Check<RoleDto>(_currentUser, PermissionKeys.RolesManage);
            if (authCheck != null) return authCheck;

            var vr = await _updateValidator.ValidateAsync(dto, ct);
            if (!vr.IsValid)
                return ServiceResult<RoleDto>.Failure(vr.Errors[0].ErrorMessage);

            var entity = await _roleRepo.GetByIdWithPermissionsAsync(dto.Id, ct);
            if (entity == null)
                return ServiceResult<RoleDto>.Failure("الدور غير موجود.");

            if (await _roleRepo.NameExistsAsync(dto.NameEn ?? dto.NameAr, dto.Id, ct))
                return ServiceResult<RoleDto>.Failure("اسم الدور موجود مسبقاً.");

            entity.Update(dto.NameAr, dto.NameEn ?? dto.NameAr, dto.Description);
            entity.SetPermissions(dto.Permissions ?? new List<string>());

            await _unitOfWork.SaveChangesAsync(ct);

            var saved = await _roleRepo.GetByIdWithPermissionsAsync(entity.Id, ct);
            return ServiceResult<RoleDto>.Success(RoleMapper.ToDto(saved));
        }

        public async Task<ServiceResult> DeleteAsync(int id, CancellationToken ct = default)
        {
            var authCheck = AuthorizationGuard.Check(_currentUser, PermissionKeys.RolesManage);
            if (authCheck != null) return authCheck;

            var entity = await _roleRepo.GetByIdWithPermissionsAsync(id, ct);
            if (entity == null)
                return ServiceResult.Failure("الدور غير موجود.");

            if (entity.IsSystem)
                return ServiceResult.Failure("لا يمكن حذف دور النظام.");

            if (entity.Users != null && entity.Users.Count > 0)
                return ServiceResult.Failure("لا يمكن حذف دور مرتبط بمستخدمين.");

            _roleRepo.Remove(entity);
            await _unitOfWork.SaveChangesAsync(ct);

            return ServiceResult.Success();
        }
    }
}
