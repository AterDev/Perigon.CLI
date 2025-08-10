namespace CodeGenerator.Test;

public class GenerateTest
{
    [Fact]
    public void Should_Parse_DbContext()
    {
        var helper = new DbContextAnalysisHelper(
            @"E:\codes\ater.dry.cli\src\Template\templates\ApiStandard\src\Definition\EntityFramework"
        );

        Console.WriteLine();

        var baseContext = helper.BaseDbContextName;
        var dbContexts = helper.DbContextNamedTypeSymbols;

        var matchDbContext = helper.GetDbContextType("User");

        Console.WriteLine();
    }
}
