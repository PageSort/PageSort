using PageSort.Core;
using PageSort.Core.Attributes;
using PageSort.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Xunit;

namespace PageSort.Tests
{
    public class User
    {
        public int Age { get; set; }
        public int Balance { get; set; }
        public string Name { get; set; }

        [MarkAsSensitive]
        public string Password { get; set; }

        public string Age90Daysago
        {
            get
            {
                return $"{Age - 90}";
            }
        }
    }

    public class UserDTO
    {
        public int Age { get; set; }
        public string Name { get; set; }

        public string Age90Daysago { get; set; } = string.Empty;

    }

    public class FilteredUser
    {
        public int Age { get; set; }
        public string Name { get; set; }
    }

    public class PageQueryTests
    {
        readonly List<User> users = new();

        public PageQueryTests()
        {
            for (int i = 0; i < 100; i++)
            {
                users.Add(new User
                {
                    Age = i,
                    Balance = i,
                    Name = $"User{i}",
                    Password = $"Password{i}"
                });
            }
        }

        [Fact]
        public void Test_Original_GeneratePaging_With_Valid_Data()
        {
            var pageQuery = new PageSort.Core.PageQuery
            {
                PageNumber = 2,
                PageSize = 10,
                SortProperty = "Age",
                SortDirection = System.ComponentModel.ListSortDirection.Descending
            };


            var pagedResult = PageSort.Core.Page<User>.GeneratePaging(users.AsQueryable(), pageQuery);
            Assert.Equal(2, pagedResult.CurrentPage);
            Assert.True(pagedResult.PreviousPage);
            Assert.True(pagedResult.NextPage);
            var firstUserInPage = pagedResult.Collection?.FirstOrDefault();
            Assert.NotNull(firstUserInPage);
        }

        [Fact]
        public void Test_Original_GeneratePaging_With_Invalid_PageNumber()
        {
            var pageQuery = new PageSort.Core.PageQuery
            {
                PageNumber = 0,
                PageSize = 10
            };

            var pagedResult = PageSort.Core.Page<User>.GeneratePaging(users.AsQueryable(), pageQuery);
            Assert.Equal(1, pagedResult.CurrentPage);
        }

        [Fact]
        public void Test_Original_GeneratePaging_With_Invalid_PageSize()
        {
            var pageQuery = new PageSort.Core.PageQuery
            {
                PageNumber = 1,
                PageSize = -5
            };
            Assert.Throws<ArgumentException>(() =>
            {
                var pagedResult = PageSort.Core.Page<User>.GeneratePaging(users.AsQueryable(), pageQuery);
            });
        }

        [Fact]
        public void Test_Original_GeneratePaging_With_Fields()
        {
            var pageQuery = new PageSort.Core.AdvancedPageQuery
            {
                PageNumber = 1,
                PageSize = 10,
                Fields = ["Name", "Age"]
            };
            Assert.Throws<InvalidOperationException>(() =>
            {
                var pagedResult = PageSort.Core.Page<User>.GeneratePaging(users.AsQueryable(), pageQuery);
            });
        }

        [Fact]
        public void Test_GeneratePagingDynamic_With_Valid_Data()
        {
            var pageQuery = new PageSort.Core.AdvancedPageQuery
            {
                PageNumber = 1,
                PageSize = 10,
                Filters =
                [
                    Filter.Create(field: "Age",  @operator: OperatorType.GreaterThan, value: "2")
                ],
                Fields = ["Name", "Age"],
                SortProperty = "Age",
                SortDirection = System.ComponentModel.ListSortDirection.Ascending
            };
            var pagedResult = Core.Page<User>.GeneratePaging(users.AsQueryable(), pageQuery);

            Assert.Equal(1, pagedResult.CurrentPage);
            Assert.False(pagedResult.PreviousPage);
            Assert.True(pagedResult.NextPage);
            var firstUserInPage = pagedResult.Collection?.FirstOrDefault();

            Assert.NotNull(firstUserInPage);
        }

        [Fact]
        public void Test_GeneratePagingDynamic_With_Invalid_Filters()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                var pageQuery = new PageSort.Core.AdvancedPageQuery
                {
                    PageNumber = 1,
                    PageSize = 10,
                    Filters =
                    [
                        Filter.Create(field: "Age",  @operator: OperatorType.Contains, value: "20")
                    ],
                    Fields = ["Name", "Age"],
                    SortProperty = "Age",
                    SortDirection = System.ComponentModel.ListSortDirection.Ascending
                };
            });

        }

        [Fact]
        public void Test_GeneratePagingDynamic_With_Wrong_SortProerty()
        {
            var pageQuery = new PageSort.Core.AdvancedPageQuery
            {
                PageNumber = 1,
                PageSize = 10,
                Fields = ["Name", "Age"],
                SortProperty = "Balance",
                SortDirection = System.ComponentModel.ListSortDirection.Ascending
            };

            Assert.Throws<InvalidOperationException>(() =>
            {
                var pagedResult = PageSort.Core.Page<User>.GeneratePaging(users.AsQueryable(), pageQuery);
            });
        }

        [Fact]
        public void Test_GeneratePagingDynamic_With_Missing_Fields()
        {
            var pageQuery = new PageSort.Core.AdvancedPageQuery
            {
                PageNumber = 1,
                PageSize = 10,
                SortProperty = "Balance",
                SortDirection = System.ComponentModel.ListSortDirection.Ascending
            };
            Assert.Throws<InvalidOperationException>(() =>
            {
                var pagedResult = PageSort.Core.Page<User>.GeneratePaging(users.AsQueryable(), pageQuery);
            });
        }

        [Fact]
        public void Test_GeneratePagingDynamic_With_Sensitive_Field()
        {
            var pageQuery = new PageSort.Core.AdvancedPageQuery
            {
                PageNumber = 1,
                PageSize = 10,
                Fields = ["Name", "Age", "Password"],
                SortProperty = "Age",
                SortDirection = System.ComponentModel.ListSortDirection.Ascending
            };
            Assert.Throws<UnauthorizedAccessException>(() =>
            {
                var pagedResult = PageSort.Core.Page<User>.GeneratePaging(users.AsQueryable(), pageQuery);
            });
        }


        [Fact]
        public void Test_GeneratePagingDynamic_Correct_Field()
        {
            var pageQuery = new PageSort.Core.AdvancedPageQuery
            {
                PageNumber = 1,
                PageSize = 10,
                Filters =
                [
                    Filter.Create(field: "Age",  @operator: OperatorType.LessThan, value: "10")
                ],
                Fields = ["Name", "Age"],
                SortProperty = "Age",
                SortDirection = System.ComponentModel.ListSortDirection.Ascending
            };

            var pagedResult = PageSort.Core.Page<User>.GeneratePaging<User, UserDTO>(users.AsQueryable(), pageQuery);

            Assert.Equal(1, pagedResult.CurrentPage);

        }
    }
}
