using CRM_System.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM_System.Infrastructure.Migrations;

[DbContext(typeof(CrmDbContext))]
[Migration("20261114030000_AddTasksAndNotifications")]
partial class AddTasksAndNotifications
{
    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
        CrmDbContextModelSnapshot.ConfigureSnapshotModel(modelBuilder);
    }
}
