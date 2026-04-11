using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UnitOfWork.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class initialDb2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CreateUserTb_RoleTb_IdRole_Id",
                table: "CreateUserTb");

            migrationBuilder.RenameColumn(
                name: "Role_Id",
                table: "RoleTb",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "IdRole_Id",
                table: "CreateUserTb",
                newName: "roleId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "CreateUserTb",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_CreateUserTb_IdRole_Id",
                table: "CreateUserTb",
                newName: "IX_CreateUserTb_roleId");

            migrationBuilder.AddForeignKey(
                name: "FK_CreateUserTb_RoleTb_roleId",
                table: "CreateUserTb",
                column: "roleId",
                principalTable: "RoleTb",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CreateUserTb_RoleTb_roleId",
                table: "CreateUserTb");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "RoleTb",
                newName: "Role_Id");

            migrationBuilder.RenameColumn(
                name: "roleId",
                table: "CreateUserTb",
                newName: "IdRole_Id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "CreateUserTb",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_CreateUserTb_roleId",
                table: "CreateUserTb",
                newName: "IX_CreateUserTb_IdRole_Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CreateUserTb_RoleTb_IdRole_Id",
                table: "CreateUserTb",
                column: "IdRole_Id",
                principalTable: "RoleTb",
                principalColumn: "Role_Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
