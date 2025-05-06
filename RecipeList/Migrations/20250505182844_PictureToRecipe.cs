using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecipeList.Migrations
{
    /// <inheritdoc />
    public partial class PictureToRecipe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PictureID",
                table: "Recipes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Pictures",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PicturePath = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pictures", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "PublicRecipes",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Calories = table.Column<int>(type: "int", nullable: false),
                    Fats = table.Column<int>(type: "int", nullable: false),
                    Carbs = table.Column<int>(type: "int", nullable: false),
                    Proteins = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OriginalRecipeId = table.Column<int>(type: "int", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    PictureID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicRecipes", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PublicRecipes_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PublicRecipes_Pictures_PictureID",
                        column: x => x.PictureID,
                        principalTable: "Pictures",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Recipes_PictureID",
                table: "Recipes",
                column: "PictureID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PublicRecipes_PictureID",
                table: "PublicRecipes",
                column: "PictureID");

            migrationBuilder.CreateIndex(
                name: "IX_PublicRecipes_UserId",
                table: "PublicRecipes",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Recipes_Pictures_PictureID",
                table: "Recipes",
                column: "PictureID",
                principalTable: "Pictures",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Recipes_Pictures_PictureID",
                table: "Recipes");

            migrationBuilder.DropTable(
                name: "PublicRecipes");

            migrationBuilder.DropTable(
                name: "Pictures");

            migrationBuilder.DropIndex(
                name: "IX_Recipes_PictureID",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "PictureID",
                table: "Recipes");
        }
    }
}
