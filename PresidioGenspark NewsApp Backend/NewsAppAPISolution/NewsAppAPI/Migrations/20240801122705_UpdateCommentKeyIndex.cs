using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsAppAPI.Migrations
{
    public partial class UpdateCommentKeyIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Comments_UserId_ArticleId",
                table: "Comments");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_UserId_ArticleId",
                table: "Comments",
                columns: new[] { "UserId", "ArticleId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Comments_UserId_ArticleId",
                table: "Comments");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_UserId_ArticleId",
                table: "Comments",
                columns: new[] { "UserId", "ArticleId" },
                unique: true);
        }
    }
}
