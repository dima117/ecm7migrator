## Пример "миграции", не зависящей от СУБД ##

Если не использовать "прямого" выполнения SQL-запросов, то "миграции" не будут зависеть от конкретной СУБД. Изменения базы данных задаются при помощи объектной модели из библиотеки ECM7.Migrator.Framework и текущий SQL запросы будут сгенерированы в формате нужной СУБД.

```
using ECM7.Migrator.Framework;
using System.Data;

[Migration(20080805151231)]
public class AddCustomerTable : Migration
{
    public override void Apply()
    {
        Database.AddTable("Customer",
            new Column("name", DbType.String.WithSize(50), ColumnProperty.NotNull),
            new Column("address", DbType.String.WithSize(100), ColumnProperty.NotNull),
            new Column("age", DbType.Int32)
        );
    }

    public override void Revert()
    {
        Database.RemoveTable("Customer");
    }
}
```

## Составные ключи ##

Чтобы создать в БД таблицу с составным первичным ключом, создаем таблицу без указания перивичного ключа и добавляем его отдельно.

```
using ECM7.Migrator.Framework;
using System.Data;

[Migration(20080806151301)]
public class AddCustomerTable : Migration
{
    public override void Apply()
    {
        Database.AddTable("CustomerAddress",
            new Column("customer_id", DbType.Int32),
            new Column("address_id", DbType.Int32)
        );

        Database.AddPrimaryKey("CustomerAddress", "customer_id", "address_id");
    }

    public override void Revert()
    {
        Database.RemoveTable("CustomerAddress");
    }
}
```

Более легкий путь задать составной первичный ключ - указать нужным колонкам признак "первичный ключ" при создании таблицы.

```
using ECM7.Migrator.Framework;
using System.Data;

[Migration(20080806161420)]
public class AddCustomerTable : Migration
{
    public override void Apply()
    {
        Database.AddTable("CustomerAddress",
            new Column("customer_id", DbType.Int32, ColumnProperty.PrimaryKey),
            new Column("address_id", DbType.Int32, ColumnProperty.PrimaryKey)
        );
    }

    public override void Revert()
    {
        Database.RemoveTable("CustomerAddress");
    }
}
```

## Пример "миграции" с различными SQL командами для различных СУБД ##

Если необходимо выполнить некоторые изменения на определенной СУБД, используйте конструкцию Database.For`<`TDialect`>`(). Данные изменения выполнятся только в том случае, если текущий диалект соответствует указанному вами.

В приведенном примере "миграция" поддерживает SQLServer и Oracle, выполняя нужный блок, в зависимости от того, какой диалект сейчас используется. Т.е. если при запуске вы указали диалект SqlServerDialect, то выполнится только первый блок изменений.

```
[Migration(20080806160101)]
public class AddFooProcedure : Migration
{
    public override void Apply()
    {
        Database.ConditionalExecuteAction()
            .For<SqlServerDialect>(db => {
            
                db.ExecuteNonQuery(@"
                    CREATE PROCEDURE Foo 
                            @var int = 0
                    AS
                    BEGIN
                            SELECT @var
                    END");
            }).
            .For<OracleDialect>(db => {
            
                db.ExecteNonQuery(@"
                    CREATE OR REPLACE 
                    PROCEDURE Foo IS
                    BEGIN
                        dbms_output.put_line('It is work');
                    END;");
            })
            .Else(db => {
                // do something
            });
    }

    public override void Revert()
    {
    
        Database.ConditionalExecuteAction()
            .For<SqlServerDialect>(db => {
            
                db.ExecuteNonQuery(@"DROP PROCEDURE Foo");
            }).
            .For<OracleDialect>(db => {
            
                db.ExecuteNonQuery(@"DROP PROCEDURE Foo");
            })
            .Else(db => {
                // do something
            });
    }
}
```

## Добавление внешнего ключа ##

```
using ECM7.Migrator.Framework;
using System.Data;

[Migration(5)]
public class AddForeignKeyToTheBookAuthor : Migration
{
    private const string FK_NAME = "FK_Book_Author";
    
    public override void Apply()
    {
        Database.AddForeignKey(FK_NAME, "Book", "authorId", "Author", "id");
    }
    public override void Revert()
    {
        Database.RemoveForeignKey(FK_NAME);
    }
}
```

## Добавление колонки в существующую таблицу ##

```
using ECM7.Migrator.Framework;
using System.Data;

[Migration(6)]
public class AddMiddleNameToCustomer : Migration
{
    public override void Apply()
    {
        Database.AddColumn("Customer", "middle_name", DbType.String, 50);
    }
    public override void Revert()
    {
        Database.RemoveColumn("Customer", "middle_name");
    }
}
```

## Произвольный SQL ##

Если вы хотите выполнить какое-то действие, которое в данный момент не поддерживается ECM7.Migrator, вы можете использовать метод ExecuteNonQuery() для выполнения произвольного SQL запроса. В комбинации с _Database.ConditionalExecuteAction()_ вы можете обеспечить корректную работу "миграции" при запуске в различных СУБД.

```
using ECM7.Migrator.Framework;
using System.Data;

[Migration(6)]
public class AddMiddleNameToCustomer : Migration
{
    public override void Apply()
    {
        Database.ExecuteNonQuery("whatever SQL you want");
    }
    public override void Revert()
    {
        Database.ExecuteNonQuery("whatever SQL you want");
    }
}
```

## Далее ##
[Как выполнить миграции](HowToRun.md) 
