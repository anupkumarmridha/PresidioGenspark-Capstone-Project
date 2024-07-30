using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsAppAPI.Migrations
{
    public partial class reactionCommentkey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reactions_UserId",
                table: "Reactions");

            migrationBuilder.DropIndex(
                name: "IX_Comments_UserId",
                table: "Comments");

            migrationBuilder.CreateIndex(
                name: "IX_Reactions_UserId_ArticleId",
                table: "Reactions",
                columns: new[] { "UserId", "ArticleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comments_UserId_ArticleId",
                table: "Comments",
                columns: new[] { "UserId", "ArticleId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reactions_UserId_ArticleId",
                table: "Reactions");

            migrationBuilder.DropIndex(
                name: "IX_Comments_UserId_ArticleId",
                table: "Comments");

            migrationBuilder.CreateIndex(
                name: "IX_Reactions_UserId",
                table: "Reactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_UserId",
                table: "Comments",
                column: "UserId");
        }
    }
}
