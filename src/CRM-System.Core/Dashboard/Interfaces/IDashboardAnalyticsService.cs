using CRM_System.Core.Common;
using CRM_System.Core.Dashboard.DTOs;

namespace CRM_System.Core.Dashboard.Interfaces;

public interface IDashboardAnalyticsService
{
    Task<ServiceResult<DashboardOverviewDto>> GetDashboardOverviewAsync(
        DashboardFilterDto? filter,
        AccessContext access,
        DateTime? referenceUtcNow = null,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<AcademicAnalyticsDto>> GetAcademicAnalyticsAsync(
        DashboardFilterDto? filter,
        AccessContext access,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<AttendanceAnalyticsDto>> GetAttendanceAnalyticsAsync(
        DashboardFilterDto? filter,
        AccessContext access,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<TaskProductivityAnalyticsDto>> GetTaskProductivityAnalyticsAsync(
        DashboardFilterDto? filter,
        AccessContext access,
        DateTime? referenceUtcNow = null,
        CancellationToken cancellationToken = default);
}
