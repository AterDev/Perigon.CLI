using Xunit;
using CodeGenerator.Generate;
using System.Collections.Generic;
using Share.Models;

namespace CoreMod.Tests.Generate;

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

        // Act
        var content = generator.GetManagerContent("TestNamespace");

        // Assert
        Assert.Contains("TestEntityManager", content);
        Assert.Contains(".Where(q => q.UserId == _userContext.UserId)", content);
        Assert.Contains("namespace TestNamespace.Managers;", content);
    }
}
