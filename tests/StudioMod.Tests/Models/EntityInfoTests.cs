using Xunit;
using CodeGenerator.Models;
using Entity.StudioMod;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioMod.Tests.Models;

public class EntityInfoTests
{
    [Fact]
    public void EntityInfo_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        var entityInfo = new EntityInfo
        {
            Name = "User",
            NamespaceName = "Domain.Entities",
            FilePath = "/path/to/User.cs",
            ModuleName = "UserModule"
        };

        // Assert
        Assert.Equal("User", entityInfo.Name);
        Assert.Equal("Domain.Entities", entityInfo.NamespaceName);
        Assert.Equal("/path/to/User.cs", entityInfo.FilePath);
        Assert.Equal("UserModule", entityInfo.ModuleName);
    }

    [Fact]
    public void GetDtoNamespace_ShouldReturnShareNamespace()
    {
        // Arrange
        var entityInfo = new EntityInfo
        {
            Name = "Product",
            NamespaceName = "Domain.Entities",
            FilePath = "Product.cs",
            ModuleName = "ProductModule"
        };

        // Act
        var dtoNamespace = entityInfo.GetDtoNamespace();

        // Assert
        Assert.Equal("ProductModule", dtoNamespace);
    }

    [Fact]
    public void GetShareNamespace_ShouldReturnModuleNameOrDefault()
    {
        // Arrange
        var entityInfoWithModule = new EntityInfo
        {
            Name = "Order",
            NamespaceName = "Domain.Entities",
            FilePath = "Order.cs",
            ModuleName = "OrderModule"
        };

        var entityInfoWithoutModule = new EntityInfo
        {
            Name = "Setting",
            NamespaceName = "Domain.Entities",
            FilePath = "Setting.cs",
            ModuleName = string.Empty
        };

        // Act
        var withModule = entityInfoWithModule.GetShareNamespace();
        var withoutModule = entityInfoWithoutModule.GetShareNamespace();

        // Assert
        Assert.Equal("OrderModule", withModule);
        Assert.Equal("Share", withoutModule); // ConstVal.ShareName
    }

    [Fact]
    public void GetCommonNamespace_ShouldReturnModuleNameOrDefault()
    {
        // Arrange
        var entityInfoWithModule = new EntityInfo
        {
            Name = "Invoice",
            NamespaceName = "Domain.Entities",
            FilePath = "Invoice.cs",
            ModuleName = "InvoiceModule"
        };

        var entityInfoWithoutModule = new EntityInfo
        {
            Name = "Config",
            NamespaceName = "Domain.Entities",
            FilePath = "Config.cs",
            ModuleName = string.Empty
        };

        // Act
        var withModule = entityInfoWithModule.GetCommonNamespace();
        var withoutModule = entityInfoWithoutModule.GetCommonNamespace();

        // Assert
        Assert.Equal("InvoiceModule", withModule);
        Assert.Equal("CommonMod", withoutModule); // ConstVal.CommonMod
    }

    [Fact]
    public void GetFilterProperties_ShouldReturnFilterableProperties()
    {
        // Arrange
        var entityInfo = new EntityInfo
        {
            Name = "User",
            NamespaceName = "Domain.Entities",
            FilePath = "User.cs",
            PropertyInfos = new List<PropertyInfo>
            {
                new PropertyInfo { Name = "Id", Type = "Guid", IsIndex = true },
                new PropertyInfo { Name = "Name", Type = "string", IsRequired = true },
                new PropertyInfo { Name = "Email", Type = "string" },
                new PropertyInfo { Name = "Age", Type = "int", IsRequired = true },
                new PropertyInfo { Name = "Bio", Type = "string", MaxLength = 500 }, // Too long
                new PropertyInfo { Name = "IsActive", Type = "bool" },
                new PropertyInfo { Name = "CreatedTime", Type = "DateTime" }, // Ignored
                new PropertyInfo { Name = "UserNavigation", Type = "User", IsNavigation = true }, // Navigation
                new PropertyInfo { Name = "Tags", Type = "List<Tag>", IsList = true } // List
            }
        };

        // Act
        var filterProperties = entityInfo.GetFilterProperties();

        // Assert
        Assert.Contains(filterProperties, p => p.Name == "Name");
        Assert.Contains(filterProperties, p => p.Name == "Age");
        Assert.Contains(filterProperties, p => p.Name == "IsActive");
        Assert.DoesNotContain(filterProperties, p => p.Name == "Bio"); // Too long
        Assert.DoesNotContain(filterProperties, p => p.Name == "Email"); // Not required/indexed/enum/bool
        Assert.DoesNotContain(filterProperties, p => p.Name == "CreatedTime"); // Ignored
        Assert.DoesNotContain(filterProperties, p => p.Name == "UserNavigation"); // Navigation
        Assert.DoesNotContain(filterProperties, p => p.Name == "Tags"); // List
    }

    [Fact]
    public void GetRequiredNavigationProperties_ShouldReturnNavigationIds()
    {
        // Arrange
        var entityInfo = new EntityInfo
        {
            Name = "Order",
            NamespaceName = "Domain.Entities",
            FilePath = "Order.cs",
            Navigations = new List<EntityNavigation>
            {
                new EntityNavigation
                {
                    Name = "Customer",
                    Type = "Customer",
                    ForeignKey = "CustomerId",
                    IsRequired = true,
                    IsCollection = false,
                    IsSkipNavigation = false
                },
                new EntityNavigation
                {
                    Name = "Items",
                    Type = "OrderItem",
                    ForeignKey = "OrderId",
                    IsRequired = false,
                    IsCollection = true, // Should be excluded
                    IsSkipNavigation = false
                }
            }
        };

        // Act
        var navigationProps = entityInfo.GetRequiredNavigationProperties();

        // Assert
        Assert.Single(navigationProps);
        Assert.Contains(navigationProps, p => p.Name == "CustomerId");
        Assert.DoesNotContain(navigationProps, p => p.Name == "OrderId");
    }

    [Fact]
    public void EntityInfo_ShouldIgnoreSpecificProperties()
    {
        // Arrange
        var entityInfo = new EntityInfo
        {
            Name = "Document",
            NamespaceName = "Domain.Entities",
            FilePath = "Document.cs",
            PropertyInfos = new List<PropertyInfo>
            {
                new PropertyInfo { Name = "Id", Type = "Guid" },
                new PropertyInfo { Name = "CreatedTime", Type = "DateTime" },
                new PropertyInfo { Name = "UpdatedTime", Type = "DateTime" },
                new PropertyInfo { Name = "IsDeleted", Type = "bool" },
                new PropertyInfo { Name = "TenantId", Type = "Guid" }
            }
        };

        // Act & Assert
        Assert.Contains("Id", EntityInfo.IgnoreProperties);
        Assert.Contains("CreatedTime", EntityInfo.IgnoreProperties);
        Assert.Contains("UpdatedTime", EntityInfo.IgnoreProperties);
        Assert.Contains("IsDeleted", EntityInfo.IgnoreProperties);
        Assert.Contains("TenantId", EntityInfo.IgnoreProperties);
    }
}
