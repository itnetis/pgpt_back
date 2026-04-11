using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UnitOfWork.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class propmtlogchangedonparamteradded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ResponseTime",
                table: "PromptLogTb",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResponseTime",
                table: "PromptLogTb");
        }
    }
}
