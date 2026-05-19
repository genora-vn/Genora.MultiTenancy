using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Genora.MultiTenancy.Migrations
{
    /// <inheritdoc />
    public partial class Add_TenantId_To_SalonBeautyBookingServices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "Salon",
                table: "AppSalonBeautyBookingServices",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE bs
                SET bs.[TenantId] = b.[TenantId]
                FROM [Salon].[AppSalonBeautyBookingServices] bs
                INNER JOIN [Salon].[AppSalonBeautyBookings] b ON bs.[BookingId] = b.[Id]
                WHERE bs.[TenantId] IS NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "Salon",
                table: "AppSalonBeautyBookingServices");
        }
    }
}
