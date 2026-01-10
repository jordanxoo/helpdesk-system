using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserService.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOutboxTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OutboxMessage_InboxState_InboxMessageId_InboxConsumerId",
                table: "OutboxMessage");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_InboxState_TempId_TempId1",
                table: "InboxState");

            migrationBuilder.DropColumn(
                name: "TempId",
                table: "InboxState");

            migrationBuilder.DropColumn(
                name: "TempId1",
                table: "InboxState");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_InboxState_MessageId_ConsumerId",
                table: "InboxState",
                columns: new[] { "MessageId", "ConsumerId" });

            migrationBuilder.AddForeignKey(
                name: "FK_OutboxMessage_InboxState_InboxMessageId_InboxConsumerId",
                table: "OutboxMessage",
                columns: new[] { "InboxMessageId", "InboxConsumerId" },
                principalTable: "InboxState",
                principalColumns: new[] { "MessageId", "ConsumerId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OutboxMessage_InboxState_InboxMessageId_InboxConsumerId",
                table: "OutboxMessage");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_InboxState_MessageId_ConsumerId",
                table: "InboxState");

            migrationBuilder.AddColumn<Guid>(
                name: "TempId",
                table: "InboxState",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TempId1",
                table: "InboxState",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddUniqueConstraint(
                name: "AK_InboxState_TempId_TempId1",
                table: "InboxState",
                columns: new[] { "TempId", "TempId1" });

            migrationBuilder.AddForeignKey(
                name: "FK_OutboxMessage_InboxState_InboxMessageId_InboxConsumerId",
                table: "OutboxMessage",
                columns: new[] { "InboxMessageId", "InboxConsumerId" },
                principalTable: "InboxState",
                principalColumns: new[] { "TempId", "TempId1" });
        }
    }
}
