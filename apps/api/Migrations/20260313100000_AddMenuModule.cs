using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantSaas.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddMenuModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── MenuSections ──────────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "MenuSections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RestaurantId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuSections", x => x.Id);
                    table.ForeignKey("FK_MenuSections_Branches_BranchId", x => x.BranchId, "Branches", "Id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_MenuSections_Restaurants_RestaurantId", x => x.RestaurantId, "Restaurants", "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex("IX_MenuSections_BranchId_Name", "MenuSections", new[] { "BranchId", "Name" }, unique: true);
            migrationBuilder.CreateIndex("IX_MenuSections_RestaurantId", "MenuSections", "RestaurantId");

            // ── Products ──────────────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RestaurantId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    MenuSectionId = table.Column<Guid>(type: "uuid", nullable: true),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Simple"),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey("FK_Products_Branches_BranchId", x => x.BranchId, "Branches", "Id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_Products_Restaurants_RestaurantId", x => x.RestaurantId, "Restaurants", "Id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_Products_MenuSections_MenuSectionId", x => x.MenuSectionId, "MenuSections", "Id", onDelete: ReferentialAction.SetNull);
                    table.ForeignKey("FK_Products_Categories_CategoryId", x => x.CategoryId, "Categories", "Id", onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex("IX_Products_BranchId", "Products", "BranchId");
            migrationBuilder.CreateIndex("IX_Products_RestaurantId", "Products", "RestaurantId");
            migrationBuilder.CreateIndex("IX_Products_MenuSectionId", "Products", "MenuSectionId");
            migrationBuilder.CreateIndex("IX_Products_CategoryId", "Products", "CategoryId");

            // ── ProductVariants ───────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "ProductVariants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Sku = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductVariants", x => x.Id);
                    table.ForeignKey("FK_ProductVariants_Products_ProductId", x => x.ProductId, "Products", "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex("IX_ProductVariants_ProductId", "ProductVariants", "ProductId");

            // ── ModifierGroups ────────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "ModifierGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RestaurantId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SelectionType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Single"),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    MinSelections = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    MaxSelections = table.Column<int>(type: "integer", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModifierGroups", x => x.Id);
                    table.ForeignKey("FK_ModifierGroups_Branches_BranchId", x => x.BranchId, "Branches", "Id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_ModifierGroups_Restaurants_RestaurantId", x => x.RestaurantId, "Restaurants", "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex("IX_ModifierGroups_BranchId", "ModifierGroups", "BranchId");
            migrationBuilder.CreateIndex("IX_ModifierGroups_RestaurantId", "ModifierGroups", "RestaurantId");

            // ── ModifierOptions ───────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "ModifierOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ModifierGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PriceDelta = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModifierOptions", x => x.Id);
                    table.ForeignKey("FK_ModifierOptions_ModifierGroups_ModifierGroupId", x => x.ModifierGroupId, "ModifierGroups", "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex("IX_ModifierOptions_ModifierGroupId", "ModifierOptions", "ModifierGroupId");

            // ── ProductVariantModifierGroups ──────────────────────────────────
            migrationBuilder.CreateTable(
                name: "ProductVariantModifierGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductVariantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModifierGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductVariantModifierGroups", x => x.Id);
                    table.ForeignKey("FK_PVModGroups_Variants", x => x.ProductVariantId, "ProductVariants", "Id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_PVModGroups_ModifierGroups", x => x.ModifierGroupId, "ModifierGroups", "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex("IX_PVModGroups_VariantId_GroupId", "ProductVariantModifierGroups", new[] { "ProductVariantId", "ModifierGroupId" }, unique: true);

            // ── Bundles ───────────────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "Bundles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bundles", x => x.Id);
                    table.ForeignKey("FK_Bundles_Products_ProductId", x => x.ProductId, "Products", "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex("IX_Bundles_ProductId", "Bundles", "ProductId", unique: true);

            // ── BundleSlots ───────────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "BundleSlots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BundleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    MinChoices = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    MaxChoices = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BundleSlots", x => x.Id);
                    table.ForeignKey("FK_BundleSlots_Bundles_BundleId", x => x.BundleId, "Bundles", "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex("IX_BundleSlots_BundleId", "BundleSlots", "BundleId");

            // ── BundleSlotChoices ─────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "BundleSlotChoices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BundleSlotId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductVariantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PriceDelta = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BundleSlotChoices", x => x.Id);
                    table.ForeignKey("FK_BundleSlotChoices_Slots_BundleSlotId", x => x.BundleSlotId, "BundleSlots", "Id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_BundleSlotChoices_Variants_ProductVariantId", x => x.ProductVariantId, "ProductVariants", "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex("IX_BundleSlotChoices_SlotId_VariantId", "BundleSlotChoices", new[] { "BundleSlotId", "ProductVariantId" }, unique: true);

            // ── BranchProductVariants ─────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "BranchProductVariants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductVariantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsAvailable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    PriceOverride = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchProductVariants", x => x.Id);
                    table.ForeignKey("FK_BranchProductVariants_Branches_BranchId", x => x.BranchId, "Branches", "Id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_BranchProductVariants_Variants_ProductVariantId", x => x.ProductVariantId, "ProductVariants", "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex("IX_BranchProductVariants_BranchId_VariantId", "BranchProductVariants", new[] { "BranchId", "ProductVariantId" }, unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("BranchProductVariants");
            migrationBuilder.DropTable("BundleSlotChoices");
            migrationBuilder.DropTable("ProductVariantModifierGroups");
            migrationBuilder.DropTable("BundleSlots");
            migrationBuilder.DropTable("ModifierOptions");
            migrationBuilder.DropTable("Bundles");
            migrationBuilder.DropTable("ModifierGroups");
            migrationBuilder.DropTable("ProductVariants");
            migrationBuilder.DropTable("Products");
            migrationBuilder.DropTable("MenuSections");
        }
    }
}
