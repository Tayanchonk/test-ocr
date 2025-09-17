using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OcrApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomsReceipts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Organization = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomsReceipts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DocumentData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OcrResultId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocumentNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocumentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PersonName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TaxId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentMethod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomsDeclarationNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomsPaymentNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomsPaymentDate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImportDuty = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Vat = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    OtherFees = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Department = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalAmountText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsCustomsReceipt = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentData", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FinancialDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PersonName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TaxId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocumentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAmountText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Department = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocumentType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomsDeclarationNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomsPaymentNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomsPaymentDate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImportDuty = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Vat = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialDocuments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ItemsInfos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomsReceiptId = table.Column<int>(type: "int", nullable: false),
                    ImportDuty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Vat = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Other = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemsInfos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemsInfos_CustomsReceipts_CustomsReceiptId",
                        column: x => x.CustomsReceiptId,
                        principalTable: "CustomsReceipts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReferenceInfos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomsReceiptId = table.Column<int>(type: "int", nullable: false),
                    Skc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Hawb = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VehicleNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TaxId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImportExportDate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeclarantName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeclarationNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentRef = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentDate = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReferenceInfos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReferenceInfos_CustomsReceipts_CustomsReceiptId",
                        column: x => x.CustomsReceiptId,
                        principalTable: "CustomsReceipts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SignInfos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomsReceiptId = table.Column<int>(type: "int", nullable: false),
                    Receiver = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Officer = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SignInfos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SignInfos_CustomsReceipts_CustomsReceiptId",
                        column: x => x.CustomsReceiptId,
                        principalTable: "CustomsReceipts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TotalInfos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomsReceiptId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AmountText = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TotalInfos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TotalInfos_CustomsReceipts_CustomsReceiptId",
                        column: x => x.CustomsReceiptId,
                        principalTable: "CustomsReceipts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExpenseItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FinancialDocumentDataId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpenseItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExpenseItems_FinancialDocuments_FinancialDocumentDataId",
                        column: x => x.FinancialDocumentDataId,
                        principalTable: "FinancialDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OcrResults",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ExtractedText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FinancialDataId = table.Column<int>(type: "int", nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConfidenceScore = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OcrResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OcrResults_FinancialDocuments_FinancialDataId",
                        column: x => x.FinancialDataId,
                        principalTable: "FinancialDocuments",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseItems_FinancialDocumentDataId",
                table: "ExpenseItems",
                column: "FinancialDocumentDataId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemsInfos_CustomsReceiptId",
                table: "ItemsInfos",
                column: "CustomsReceiptId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OcrResults_FinancialDataId",
                table: "OcrResults",
                column: "FinancialDataId");

            migrationBuilder.CreateIndex(
                name: "IX_ReferenceInfos_CustomsReceiptId",
                table: "ReferenceInfos",
                column: "CustomsReceiptId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SignInfos_CustomsReceiptId",
                table: "SignInfos",
                column: "CustomsReceiptId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TotalInfos_CustomsReceiptId",
                table: "TotalInfos",
                column: "CustomsReceiptId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentData");

            migrationBuilder.DropTable(
                name: "ExpenseItems");

            migrationBuilder.DropTable(
                name: "ItemsInfos");

            migrationBuilder.DropTable(
                name: "OcrResults");

            migrationBuilder.DropTable(
                name: "ReferenceInfos");

            migrationBuilder.DropTable(
                name: "SignInfos");

            migrationBuilder.DropTable(
                name: "TotalInfos");

            migrationBuilder.DropTable(
                name: "FinancialDocuments");

            migrationBuilder.DropTable(
                name: "CustomsReceipts");
        }
    }
}
