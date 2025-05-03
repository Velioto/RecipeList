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

            migrationBuilder.CreateIndex(
                name: "IX_Recipes_PictureID",
                table: "Recipes",
                column: "PictureID",
                unique: true);

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
