using Xunit;
using CodeGenerator.Generate;
using CodeGenerator.Models;
using Entity.StudioMod;
using System.Collections.Generic;

namespace StudioMod.Tests.Generate;

public class ManagerGenerateTests
{
    [Fact]
    public void GetManagerContent_ShouldGenerateCorrectContent()
    {
        // Arrange
        var entityInfo = new EntityInfo
        {
            Name = "TestEntity",
            NamespaceName = "TestNamespace",
            FilePath = "TestEntity.cs",
            PropertyInfos = new List<PropertyInfo>
            {
                new PropertyInfo { Name = "Name", Type = "string", IsRequired = true },
                new PropertyInfo { Name = "UserId", Type = "Guid" }
            }
        };

        var generator = new ManagerGenerate(entityInfo, new List<string> { "UserId" });
        var tplContent = "namespace @Model.Namespace; public class @(Model.EntityName)Manager {}";

        // Act
        var content = generator.GetManagerContent(tplContent, "TestNamespace.Managers");

        // Assert
        Assert.Contains("TestEntityManager", content);
        Assert.Contains("namespace TestNamespace.Managers", content);
    }
}
