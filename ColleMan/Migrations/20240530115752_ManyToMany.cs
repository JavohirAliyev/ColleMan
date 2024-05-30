using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ColleMan.Migrations
{
    /// <inheritdoc />
    public partial class ManyToMany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Items_Collections_CollectionId",
                table: "Items");

            migrationBuilder.DropForeignKey(
                name: "FK_Tags_Items_ItemId",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_Tags_ItemId",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "ItemId",
                table: "Tags");

            migrationBuilder.AlterColumn<int>(
                name: "CollectionId",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "ItemTag",
                columns: table => new
                {
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    TagsID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemTag", x => new { x.ItemId, x.TagsID });
                    table.ForeignKey(
                        name: "FK_ItemTag_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemTag_Tags_TagsID",
                        column: x => x.TagsID,
                        principalTable: "Tags",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItemTag_TagsID",
                table: "ItemTag",
                column: "TagsID");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Collections_CollectionId",
                table: "Items",
                column: "CollectionId",
                principalTable: "Collections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Items_Collections_CollectionId",
                table: "Items");

            migrationBuilder.DropTable(
                name: "ItemTag");

            migrationBuilder.AddColumn<int>(
                name: "ItemId",
                table: "Tags",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CollectionId",
                table: "Items",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_ItemId",
                table: "Tags",
                column: "ItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Collections_CollectionId",
                table: "Items",
                column: "CollectionId",
                principalTable: "Collections",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_Items_ItemId",
                table: "Tags",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "Id");
        }
    }
}
