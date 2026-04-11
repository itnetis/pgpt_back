using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UnitOfWork.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class initialDb3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PromptId",
                table: "PromptTb",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ModelId",
                table: "ModelTb",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "PromptId",
                table: "CreateUserTb",
                newName: "promptId");

            migrationBuilder.RenameColumn(
                name: "ModelId",
                table: "CreateUserTb",
                newName: "modelId");

            migrationBuilder.CreateIndex(
                name: "IX_CreateUserTb_modelId",
                table: "CreateUserTb",
                column: "modelId");

            migrationBuilder.CreateIndex(
                name: "IX_CreateUserTb_promptId",
                table: "CreateUserTb",
                column: "promptId");

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

            migrationBuilder.DropIndex(
                name: "IX_CreateUserTb_modelId",
                table: "CreateUserTb");

            migrationBuilder.DropIndex(
                name: "IX_CreateUserTb_promptId",
                table: "CreateUserTb");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "PromptTb",
                newName: "PromptId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "ModelTb",
                newName: "ModelId");

            migrationBuilder.RenameColumn(
                name: "promptId",
                table: "CreateUserTb",
                newName: "PromptId");

            migrationBuilder.RenameColumn(
                name: "modelId",
                table: "CreateUserTb",
                newName: "ModelId");
        }
    }
}
