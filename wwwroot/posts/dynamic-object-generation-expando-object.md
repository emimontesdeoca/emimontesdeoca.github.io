I've been in the need of converting an Excel files with its own defined columns to a new one with dynamic columns, and I was kinda confused on how to do it properly without some serious performance issues.

I came up to a tutorial that you can find [here](https://www.oreilly.com/content/building-c-objects-dynamically/) that uses .NET [ExpandoObject](https://docs.microsoft.com/en-us/dotnet/api/system.dynamic.expandoobject?view=netframework-4.8) which pretty much lets you create an object and add dynamic memebers.

## ExpandoObject

The Microsoft definition is:

> Represents an object whose members can be dynamically added and removed at run time.

And it has a few remarks:

>The ExpandoObject class enables you to add and delete members of its instances at run time and also to set and get values of these members. This class supports dynamic binding, which enables you to use standard syntax like sampleObject.sampleMember instead of more complex syntax like sampleObject.GetAttribute("sampleMember").


## Current behaviour

We have an `ExcelExportService` that, by passing a `List<T>`, in this case `ExcelItem`, will use `Reflection` to create an `xlsx` file.

So far, our code looks like this:

```csharp
class ExcelItem
  {
    public int Id { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Age { get; set; }
    public string CreatedAt { get; set; }
    public string UpdatedAt { get; set; }
  }

```

`ExcelItem` is the object with all the properties that are being used to generate the excel file.


```csharp
public static byte[] GetExcelBytes() {
    List<ExcelItem> excelItems = new List<ExcelItem>();
    ExcelExportService exportService = new ExcelExportService();

    foreach (var items in itemsToExcel)
    {
        excelItems.Add(new ExcelItem()
        {
            Id = item.Id,
            Name = item.Name,
            Surname = item.Surname,
            Age = item.Age,
            CreatedAt = item.CreatedAt
            UpdatedAt = item.UpdatedAt
        });
    }

    return exportService.ExportToExcel(excelItems);
}
```

This approach works perfectly, the `xlsx` file is generated without problems with each column being each property, you would use this `GetExcelBytes` method like this, for example, to save it into a file:

```csharp
File.WriteAllBytes("path-to-somewhere/my-file.xlsx", GetExcelBytes());
```

## The problem

Since what we are doing right know is using all the properties of the `ExcelItem` object, all I want is to actually not use all of them, just using maybe 2 o 3 of them. 

Also a requirement is that I don't want to be changing the code, eveything should be done by the `administrator`, who will decide which columns should be shown, and he may or may not know the properties in the code.

TLDR: everything should be dynamic, we have an object with properties and we must make sure that the generated file have `N` properties of that object, but obviously not hardcoded.

## Dynamic columns

The change would be to use a `dynamic` object, because we would like to set what properties of the object will be used to generate the list of columns.

Let's say that we have an object with a lot of properties, like, *a ton* of them, and we don't really want to be changing the `ExcelItem` object everytime that we make a change, we create a `ColumnExcelItem` table, which will be used to generate that `ExcelItem`.

### Database structure

We save this definition in our database with something like this:

```
Id    PropertyName
---------------------
1     Name
2     Surname
3     Age
```

Note that we have less values that the existing properties of the `ExcelItem` object, with it having `6` properties and we only having `3` listed on the database.

Also note that the `PropertyName` must match the property name of the object that I'll use for the dynamic assignment.

## Building the object

Now that we have the database table, we need to build the repository to get that and be able to use them in the code.

I will not be making a tutorial of this part, I'll skip right at the start of the new `GetExcelBytes()` function.


```csharp
public static byte[] GetExcelBytes(List<string> columns) {
    List<ExcelItem> excelItems = new List<ExcelItem>();
    List<dynamic> objectsToExcel = new List<dynamic>();
    ExcelExportService exportService = new ExcelExportService();

    foreach (var item in itemsToExcel)
    {
        dynamic newObject = new ExpandoObject();
        foreach (var col in columns)
        {
            this.AddProperty(newObject, col, product.GetType().GetProperty(col).GetValue(item, null));
        }

        objectsToExcel.Add(newObject);
    }

    return exportService.ExportToExcel(objectsToExcel);
}
```

Now we will be having creating an object with some of the properties of `ExcelItem`, but completely dynamic. Instead of using all the 6 properties, we are just using 3 of them, the one that we only want to send.

Now let's pretend that this `ExcelItem` object has 200 properties, *crazy*, but it could happen.

The only thing that I have to do is to insert those properties that I want to render into the Excel file to that `ColumnExcelItem` table and that's it.

## Cases

In this case we just used a single case, but let's say that you want to have different reports for some users. Let's say that the `admin` role should get all the properties, then you would create something like this structure in the database

```
ReportingTemplates

Id    PropertyName
---------------------
1     Admin
2     Users
```


```
ReportingTemplateItems

Id    TemplateId    PropertyName
---------------------
1     1             Id
2     1             Name
3     1             Surname
4     1             Age
5     1             CreatedAt
6     1             UpdatedAt
7     2             Name
8     2             Surname
9     2             Age
```

Then by getting the template you could have different columns, all dynamically and no code related. 

If we have an user with the role `Admins`, the file will contain columns `Id`, `Name`, `Surname`, `Age`, `CreatedAt`, `UpdatedAt`. 

In the case if you generate it using the `Users` role, it will have `Name`, `Age`. `Surname`.

## Conclusion

This is a cool way of learing how the `dynamic` works in .NET, and it's a really good solution when you need to have different roles or templates for different users, and not really interested on investing time hardcoding stuff.