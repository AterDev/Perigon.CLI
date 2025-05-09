using System.CommandLine;
using Entity.StudioMod;

namespace CommandLine;

public class CommandBuilder
{
    public RootCommand RootCommand { get; set; }
    private readonly CommandRunner _runner;

    public CommandBuilder(CommandRunner runner)
    {
        RootCommand = new RootCommand("Welcome to use dry cli, a tool to help you generate source code!")
        {
            Name = "dry"
        };
        _runner = runner;
    }

    public RootCommand Build()
    {
        AddDto();
        AddManager();
        AddApi();
        AddNgService();
        AddRequest();
        AddStudio();
        AddCsharpClient();
        return RootCommand;
    }

    public void AddStudio()
    {
        System.CommandLine.Command studioCommand = new("studio", "studio management");
        System.CommandLine.Command update = new("update", "update studio");

        update.SetHandler(CommandRunner.UpdateStudio);
        studioCommand.SetHandler(CommandRunner.RunStudioAsync);

        studioCommand.AddCommand(update);
        RootCommand.Add(studioCommand);
    }

    /// <summary>
    /// DTO命令
    /// </summary>
    public void AddDto()
    {
        // dto 生成命令
        System.CommandLine.Command dtoCommand = new("dto", "generate entity dto files");
        Argument<string> path = new("entity path", "The entity file path");
        Option<string> outputOption = new(["--output", "-o"],
            "output project directory");
        outputOption.SetDefaultValue(PathConst.SharePath);
        Option<bool> forceOption = new(["--force", "-f"],
            "force overwrite file");
        forceOption.SetDefaultValue(true);

        dtoCommand.AddArgument(path);
        dtoCommand.AddOption(outputOption);
        dtoCommand.AddOption(forceOption);
        dtoCommand.SetHandler(_runner.GenerateDtoAsync, path, outputOption, forceOption);

        RootCommand.Add(dtoCommand);
    }
    /// <summary>
    /// 前端请求命令
    /// </summary>
    public void AddRequest()
    {
        System.CommandLine.Command reqCommand = new("request", "generate request service and interface from OpenAPI json");
        reqCommand.AddAlias("request");
        Argument<string> url = new("Url or Path", "openApi json file url");
        Option<string> outputOption = new(["--output", "-o"])
        {
            IsRequired = true,
            Description = "output path"
        };

        Option<RequestLibType> typeOption = new(["--type", "-t"], "request lib type:axios or NgHttp");
        reqCommand.AddArgument(url);
        reqCommand.AddOption(outputOption);
        reqCommand.AddOption(typeOption);
        reqCommand.SetHandler(_runner.GenerateRequestAsync, url, outputOption, typeOption);

        RootCommand.Add(reqCommand);
    }

    /// <summary>
    /// 后端manager服务
    /// </summary>
    public void AddManager()
    {
        // api 生成命令
        System.CommandLine.Command apiCommand = new("manager", "generate dtos, datastore, manager");
        apiCommand.AddAlias("manager");
        Argument<string> path = new("entity path", "The entity file path");
        Option<string> dtoOption = new(["--dto", "-d"],
            "dto project directory");
        dtoOption.SetDefaultValue(PathConst.SharePath);
        Option<string> storeOption = new(["--manager", "-m"],
            "application project directory");
        storeOption.SetDefaultValue(PathConst.ApplicationPath);
        Option<string> typeOption = new(["--type", "-t"],
            "api type, valid values:rest/grpc/graph");
        Option<bool> forceOption = new(["--force", "-f"],
           "force overwrite file");
        forceOption.SetDefaultValue(false);

        typeOption.SetDefaultValue("rest");
        apiCommand.AddArgument(path);
        apiCommand.AddOption(dtoOption);
        apiCommand.AddOption(storeOption);
        apiCommand.AddOption(forceOption);

        apiCommand.SetHandler(
            _runner.GenerateManagerAsync, path, dtoOption, storeOption, forceOption);

        RootCommand.Add(apiCommand);
    }
    /// <summary>
    /// 后端接口及仓储、dto
    /// </summary>
    public void AddApi()
    {
        // api 生成命令
        System.CommandLine.Command apiCommand = new("webapi", "generate dtos, managers and RestfulApi controllers");
        apiCommand.AddAlias("api");
        Argument<string> path = new("entity path", "The entity file path");
        Option<string> dtoOption = new(["--dto", "-d"],
            "dto project director");
        dtoOption.SetDefaultValue(PathConst.SharePath);

        Option<string> managerOption = new(["--manager", "-m"],
            "manager and datastore project directory");
        managerOption.SetDefaultValue(PathConst.ApplicationPath);

        Option<string> apiOption = new(["--output", "-o"],
            "api controller project directory");
        apiOption.SetDefaultValue(PathConst.APIPath);

        Option<bool> forceOption = new(["--force", "-f"],
      "force overwrite file");
        forceOption.SetDefaultValue(false);

        apiCommand.AddArgument(path);
        apiCommand.AddOption(dtoOption);
        apiCommand.AddOption(managerOption);
        apiCommand.AddOption(apiOption);
        apiCommand.AddOption(forceOption);

        apiCommand.SetHandler(_runner.GenerateApiAsync, path, dtoOption, managerOption, apiOption, forceOption);

        RootCommand.Add(apiCommand);
    }

    public void AddCsharpClient()
    {
        System.CommandLine.Command clientCommand = new("client", "generate csharp request   client from OpenAPI json");
        clientCommand.AddAlias("client");
        Argument<string> url = new("Url or Path", "openApi json file url or local path");
        Option<string> outputOption = new(["--output", "-o"])
        {
            IsRequired = true,
            Description = "client project path",
        };
        var language = new Option<LanguageType>(["--language", "-l"], "client language type, ");
        language.SetDefaultValue(LanguageType.CSharp);
        language.IsRequired = false;
        language.IsHidden = true;

        clientCommand.AddArgument(url);
        clientCommand.AddOption(outputOption);
        clientCommand.SetHandler(CommandRunner.GenerateCSharpApiClientAsync, url, outputOption, language);

        RootCommand.Add(clientCommand);
    }

    public void AddNgService()
    {
        System.CommandLine.Command ngCommand = new("angular", "generate angular request service and interface from OpenAPI json");
        ngCommand.AddAlias("ng");
        Argument<string> url = new("Url or Path", "openApi json file url or local path");
        Option<string> outputOption = new(["--output", "-o"])
        {
            IsRequired = true,
            Description = "angular project root path"
        };
        ngCommand.AddArgument(url);
        ngCommand.AddOption(outputOption);
        ngCommand.SetHandler(_runner.GenerateNgAsync, url, outputOption);

        RootCommand.Add(ngCommand);
    }
}
