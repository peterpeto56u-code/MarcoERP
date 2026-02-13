using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MarcoERP.Application.Common;
using MarcoERP.Application.DTOs.Settings;
using MarcoERP.Application.Interfaces;
using MarcoERP.Application.Interfaces.Settings;
using MarcoERP.Application.Mappers.Settings;
using MarcoERP.Domain.Entities.Settings;
using MarcoERP.Domain;
using MarcoERP.Domain.Enums;
using MarcoERP.Domain.Interfaces;
using MarcoERP.Domain.Interfaces.Settings;

namespace MarcoERP.Application.Services.Settings
{
    /// <summary>
    /// Application service for Feature Governance.
    /// Phase 2: Feature Governance Engine — safe mode.
    /// Records all enable/disable changes in FeatureChangeLog.
    /// </summary>
    [Module(SystemModule.Settings)]
    public sealed class FeatureService : IFeatureService
    {
        private readonly IFeatureRepository _featureRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly IDateTimeProvider _dateTime;

        public FeatureService(
            IFeatureRepository featureRepo,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            IDateTimeProvider dateTime)
        {
            _featureRepo = featureRepo ?? throw new ArgumentNullException(nameof(featureRepo));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _dateTime = dateTime ?? throw new ArgumentNullException(nameof(dateTime));
        }

        public async Task<ServiceResult<IReadOnlyList<FeatureDto>>> GetAllAsync(CancellationToken ct = default)
        {
            var entities = await _featureRepo.GetAllAsync(ct);
            return ServiceResult<IReadOnlyList<FeatureDto>>.Success(
                entities.Select(FeatureMapper.ToDto).ToList());
        }

        public async Task<ServiceResult<FeatureDto>> GetByKeyAsync(string featureKey, CancellationToken ct = default)
        {
            var entity = await _featureRepo.GetByKeyAsync(featureKey, ct);
            if (entity == null)
                return ServiceResult<FeatureDto>.Failure($"الميزة '{featureKey}' غير موجودة.");
            return ServiceResult<FeatureDto>.Success(FeatureMapper.ToDto(entity));
        }

        public async Task<ServiceResult<bool>> IsEnabledAsync(string featureKey, CancellationToken ct = default)
        {
            var entity = await _featureRepo.GetByKeyAsync(featureKey, ct);
            if (entity == null)
                return ServiceResult<bool>.Failure($"الميزة '{featureKey}' غير موجودة.");
            return ServiceResult<bool>.Success(entity.IsEnabled);
        }

        public async Task<ServiceResult> ToggleAsync(ToggleFeatureDto dto, CancellationToken ct = default)
        {
            var authCheck = AuthorizationGuard.Check(_currentUser, PermissionKeys.SettingsManage);
            if (authCheck != null) return authCheck;

            if (string.IsNullOrWhiteSpace(dto.FeatureKey))
                return ServiceResult.Failure("مفتاح الميزة مطلوب.");

            var entity = await _featureRepo.GetByKeyAsync(dto.FeatureKey, ct);
            if (entity == null)
                return ServiceResult.Failure($"الميزة '{dto.FeatureKey}' غير موجودة.");

            // No change needed
            if (entity.IsEnabled == dto.IsEnabled)
                return ServiceResult.Success();

            bool oldValue = entity.IsEnabled;

            if (dto.IsEnabled)
                entity.Enable();
            else
                entity.Disable();

            _featureRepo.Update(entity);

            // Record change log
            var log = new FeatureChangeLog(
                entity.Id,
                entity.FeatureKey,
                oldValue,
                dto.IsEnabled,
                _currentUser.Username ?? DomainConstants.SystemUser,
                _dateTime.UtcNow);

            await _featureRepo.AddChangeLogAsync(log, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return ServiceResult.Success();
        }
    }
}
