using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UnitOfWork.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class final : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CreateUserTb_ModelTb_modelId",
                table: "CreateUserTb");

            migrationBuilder.DropForeignKey(
                name: "FK_CreateUserTb_PromptTb_promptId",
                table: "CreateUserTb");

            migrationBuilder.DropForeignKey(
                name: "FK_CreateUserTb_RoleTb_roleId",
                table: "CreateUserTb");

            migrationBuilder.DropColumn(
                name: "PromptName",
                table: "PromptTb");

            migrationBuilder.AlterColumn<string>(
                name: "Role_Name",
                table: "RoleTb",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "PromptAllowed",
                table: "PromptTb",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ModelName",
                table: "ModelTb",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "roleId",
                table: "CreateUserTb",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "promptId",
                table: "CreateUserTb",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "modelId",
                table: "CreateUserTb",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "CreateUserTb",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "TokenAllowed",
                table: "CreateUserTb",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "CreateUserTb",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddForeignKey(
                name: "FK_CreateUserTb_ModelTb_modelId",
                table: "CreateUserTb",
                column: "modelId",
                principalTable: "ModelTb",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CreateUserTb_PromptTb_promptId",
                table: "CreateUserTb",
                column: "promptId",
                principalTable: "PromptTb",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CreateUserTb_RoleTb_roleId",
                table: "CreateUserTb",
                column: "roleId",
                principalTable: "RoleTb",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CreateUserTb_ModelTb_modelId",
                table: "CreateUserTb");

            migrationBuilder.DropForeignKey(
                name: "FK_CreateUserTb_PromptTb_promptId",
                table: "CreateUserTb");

            migrationBuilder.DropForeignKey(
                name: "FK_CreateUserTb_RoleTb_roleId",
                table: "CreateUserTb");

            migrationBuilder.DropColumn(
                name: "PromptAllowed",
                table: "PromptTb");

            migrationBuilder.AlterColumn<string>(
                name: "Role_Name",
                table: "RoleTb",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PromptName",
                table: "PromptTb",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "ModelName",
                table: "ModelTb",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "roleId",
                table: "CreateUserTb",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "promptId",
                table: "CreateUserTb",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "modelId",
                table: "CreateUserTb",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "CreateUserTb",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TokenAllowed",
                table: "CreateUserTb",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "CreateUserTb",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CreateUserTb_ModelTb_modelId",
                table: "CreateUserTb",
                column: "modelId",
                principalTable: "ModelTb",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CreateUserTb_PromptTb_promptId",
                table: "CreateUserTb",
                column: "promptId",
                principalTable: "PromptTb",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CreateUserTb_RoleTb_roleId",
                table: "CreateUserTb",
                column: "roleId",
                principalTable: "RoleTb",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
