using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MachineMonitoring.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameMachineInputSourcesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_MachineInputSources",
                table: "MachineInputSources");

            migrationBuilder.RenameTable(
                name: "MachineInputSources",
                newName: "machine_input_sources");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "machine_input_sources",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "EndpointOrTopic",
                table: "machine_input_sources",
                type: "character varying(300)",
                maxLength: 300,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddPrimaryKey(
                name: "PK_machine_input_sources",
                table: "machine_input_sources",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_machine_input_sources_EndpointOrTopic",
                table: "machine_input_sources",
                column: "EndpointOrTopic");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_machine_input_sources",
                table: "machine_input_sources");

            migrationBuilder.DropIndex(
                name: "IX_machine_input_sources_EndpointOrTopic",
                table: "machine_input_sources");

            migrationBuilder.RenameTable(
                name: "machine_input_sources",
                newName: "MachineInputSources");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "MachineInputSources",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "EndpointOrTopic",
                table: "MachineInputSources",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(300)",
                oldMaxLength: 300);

            migrationBuilder.AddPrimaryKey(
                name: "PK_MachineInputSources",
                table: "MachineInputSources",
                column: "Id");
        }
    }
}
