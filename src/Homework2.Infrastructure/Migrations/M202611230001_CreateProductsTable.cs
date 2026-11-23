using FluentMigrator;

namespace Homework2.Infrastructure.Migrations;

[Migration(202611230001, "Create products table")]
public class M202611230001_CreateProductsTable : Migration
{
    public override void Up()
    {
        Create.Table("products")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("title").AsString(255).NotNullable()
            .WithColumn("price").AsDecimal(12, 2).NotNullable()
            .WithColumn("weight").AsInt32().NotNullable()
            .WithColumn("type").AsInt32().NotNullable()
            .WithColumn("created_at").AsDate().NotNullable()
            .WithColumn("storage_id").AsGuid().NotNullable();

        Create.Index("ix_products_created_at")
            .OnTable("products")
            .OnColumn("created_at");

        Create.Index("ix_products_type")
            .OnTable("products")
            .OnColumn("type");

        Create.Index("ix_products_storage_id")
            .OnTable("products")
            .OnColumn("storage_id");
    }

    public override void Down()
    {
        Delete.Table("products");
    }
}
