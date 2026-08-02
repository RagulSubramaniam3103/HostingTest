using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EWOMS_ClassLibrary.Migrations
{
    /// <inheritdoc />
    public partial class AddComicAccess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF COL_LENGTH('dbo.EWO_MasterUser', 'ComicAccess') IS NULL
                BEGIN
                    ALTER TABLE [dbo].[EWO_MasterUser]
                    ADD [ComicAccess] bit NOT NULL CONSTRAINT [DF_EWO_MasterUser_ComicAccess] DEFAULT 0;
                END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF COL_LENGTH('dbo.EWO_MasterUser', 'ComicAccess') IS NOT NULL
                BEGIN
                    DECLARE @constraintName nvarchar(128);

                    SELECT @constraintName = dc.name
                    FROM sys.default_constraints dc
                    INNER JOIN sys.columns c ON c.default_object_id = dc.object_id
                    INNER JOIN sys.tables t ON t.object_id = c.object_id
                    INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
                    WHERE s.name = 'dbo'
                        AND t.name = 'EWO_MasterUser'
                        AND c.name = 'ComicAccess';

                    IF @constraintName IS NOT NULL
                    BEGIN
                        EXEC('ALTER TABLE [dbo].[EWO_MasterUser] DROP CONSTRAINT [' + @constraintName + ']');
                    END

                    ALTER TABLE [dbo].[EWO_MasterUser] DROP COLUMN [ComicAccess];
                END");
        }
    }
}
