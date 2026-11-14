using System;
using CRM_System.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM_System.Infrastructure.Migrations;

[DbContext(typeof(CrmDbContext))]
[Migration("20261114020000_AddAcademicManagement")]
partial class AddAcademicManagement
{
    /// <inheritdoc />
    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
        CrmDbContextModelSnapshot.ConfigureSnapshotModel(modelBuilder);
    }
}
