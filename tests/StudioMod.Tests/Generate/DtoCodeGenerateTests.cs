using Xunit;
using CodeGenerator.Generate;
using CodeGenerator.Models;
using Entity.StudioMod;
using System.Collections.Generic;
using System.Linq;

namespace StudioMod.Tests.Generate;

public class DtoCodeGenerateTests
{
    [Fact]
    public void GetAddDto_ShouldGenerateCorrectProperties()
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
                new PropertyInfo { Name = "Age", Type = "int" },
                new PropertyInfo { Name = "Id", Type = "Guid" }, // Should be ignored in AddDto usually? No, usually Id is ignored if auto-generated.
                new PropertyInfo { Name = "CreatedTime", Type = "DateTime" } // Should be ignored
            }
        };

        var generator = new DtoCodeGenerate(entityInfo, new List<string>());

        // Act
        var dto = generator.GetAddDto();

        // Assert
        Assert.Equal("TestEntityAddDto", dto.Name);
        Assert.Contains(dto.Properties, p => p.Name == "Name");
        Assert.Contains(dto.Properties, p => p.Name == "Age");
        
        // Verify ignored properties (based on default IgnoreProperties in EntityInfo)
        Assert.DoesNotContain(dto.Properties, p => p.Name == "CreatedTime");
    }

    [Fact]
    public void GetUpdateDto_ShouldMakePropertiesNullable()
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
                new PropertyInfo { Name = "Age", Type = "int" }
            }
        };

        var generator = new DtoCodeGenerate(entityInfo, new List<string>());

        // Act
        var dto = generator.GetUpdateDto();

        // Assert
        Assert.Equal("TestEntityUpdateDto", dto.Name);
        // UpdateDto logic in DtoCodeGenerate sets all properties to nullable
        Assert.True(dto.Properties.All(p => p.IsNullable));
    }
}
