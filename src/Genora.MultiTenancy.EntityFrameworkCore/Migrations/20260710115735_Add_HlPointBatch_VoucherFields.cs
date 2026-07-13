using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Genora.MultiTenancy.Migrations
{
    /// <inheritdoc />
    public partial class Add_HlPointBatch_VoucherFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AccumulatedPoints",
                schema: "HL",
                table: "AppHlPointBatches",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AccumulatedSales",
                schema: "HL",
                table: "AppHlPointBatches",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VoucherCode",
                schema: "HL",
                table: "AppHlPointBatches",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VoucherName",
                schema: "HL",
                table: "AppHlPointBatches",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VoucherType",
                schema: "HL",
                table: "AppHlPointBatches",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "VoucherValue",
                schema: "HL",
                table: "AppHlPointBatches",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccumulatedPoints",
                schema: "HL",
                table: "AppHlPointBatches");

            migrationBuilder.DropColumn(
                name: "AccumulatedSales",
                schema: "HL",
                table: "AppHlPointBatches");

            migrationBuilder.DropColumn(
                name: "VoucherCode",
                schema: "HL",
                table: "AppHlPointBatches");

            migrationBuilder.DropColumn(
                name: "VoucherName",
                schema: "HL",
                table: "AppHlPointBatches");

            migrationBuilder.DropColumn(
                name: "VoucherType",
                schema: "HL",
                table: "AppHlPointBatches");

            migrationBuilder.DropColumn(
                name: "VoucherValue",
                schema: "HL",
                table: "AppHlPointBatches");
        }
    }
}
