using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UnitOfWork.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class initialDb1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RoleId",
                table: "CreateUserTb",
                newName: "IdRole_Id");

            migrationBuilder.CreateIndex(
                name: "IX_CreateUserTb_IdRole_Id",
                table: "CreateUserTb",
                column: "IdRole_Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CreateUserTb_RoleTb_IdRole_Id",
                table: "CreateUserTb",
                column: "IdRole_Id",
                principalTable: "RoleTb",
                principalColumn: "Role_Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CreateUserTb_RoleTb_IdRole_Id",
                table: "CreateUserTb");

            migrationBuilder.DropIndex(
                name: "IX_CreateUserTb_IdRole_Id",
                table: "CreateUserTb");

            migrationBuilder.RenameColumn(
                name: "IdRole_Id",
                table: "CreateUserTb",
                newName: "RoleId");
        }
    }
}
