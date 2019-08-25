# PageSort

# What is PageSort
PageSort is a simple .Net Standard library that is built to ease paging, sort and filtering of Iquerable collections.
This is the main repository for PageSort and its extensions.


## Getting Started

You need to reference the PageSort namespace in your project.

```csharp
Using PageSort;
```

### Paging Example

#### Using the IQuarable Extension

collection - collection.Page(1, 20);

Thhe Page Iquarable extension takes the pageNumber and the pageSize.
It returns page collection of the specified source query.

#### Using the GeneratePaging Method.

```csharp
var pageQuery = new PageQuery{
    PageNumber = 1,
    PageSize = 20,
};

var collection = new IQuarable<Student>();
var pagedCollection = GeneratePaging(collection, pageQuery);
```

The GeneratePaging() method takes an Iquarable collection and a PageQuery object.
This method returns a PageResult object. 

### Sorting Example

#### Using the IQuarable Extension.

```csharp
var sortedCollection = collection.OrderByProperty(sortProperty);

var sortedCollection = collection.OrderByDescendingProperty(sortProperty);
```

#### Page and Sort at ago

You can also using the GeneratePaging Method to perform both paging and sorting Operations on a collection

```csharp
var pageQuery = new PageQuery{
    PageNumber = 1,
    PageSize = 20,
    SortProperty = "DateOfBirth",
    SortOrder = ListSortDirection.Descending
};

var collection = new IQuarable<Student>();
var pagedCollection = GeneratePaging(collection, pageQuery);
```

#### PageResult

Below is an example of a PageResult

```csharp
{
    CurrentPage = 1,
    PageSize = 20,
    TotalCount = 200,
    TotalPages = 10,
    Collection = collection,
    PreviousPage = true,
    NextPage = true

}
```

**CurrentPage** is an integer that represents the current page of the collection that is being returned
**PageSize** is the size of the collection that is to be returned per page.
**TotalCount** is the total number of Items in the collection for which paging is done.
**TotalPages** is the total number of pages that can be generated from the collection. This value is dependant on the PageSize value that is passed.
**Collection** is the list of items in the current page.
**PreviousPage** is a boolean that tells whether there are previous pages or not. This is set to false if PageNumber is 1 or the collection can only have one page.
**NextPage** is a boolean that tells whether there are more pages or not. It is set to false if there is only one page, or we are on the last page of the collection

### Installing

First, [install NuGet](http://docs.nuget.org/docs/start-here/installing-nuget). Then, install [PageSort](https://www.nuget.org/packages/PageSort/) from the package manager console:

```
PM> Install-Package PageSort -Version 1.0.2
```

You can also install the PageSort Package using the .NET CLI

```
dotnet add package PageSort --version 1.0.2
```

You can also add the package reference to the project file of your project.

```
<PackageReference Include="PageSort" Version="1.0.2" />
```

### Do you have an issue?

If you run into problems, please share it with me via email at stanleybarna@gmail.com


## License, e.t.c.

PageSort is Copyright &copy; 2019 [Stanley Barnabas Nagwere](https://xente.co/home/team/) and other contributors under the [MIT license](LICENSE.txt).


