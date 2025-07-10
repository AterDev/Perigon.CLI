using System.ComponentModel;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Server;

namespace AterStudio.McpTools;

[McpServerPromptType]
public static class Prompts
{
    [McpServerPrompt, Description("create a prompt for create entity class")]
    public static ChatMessage CreateEntity()
    {
        return new ChatMessage(
            ChatRole.User,
            """
            <rules>
            实体模型需要创建在`src/Definition`目录下的`Entity`项目中；

            如果指定了模块名称，则需要在`Entity`程序集下创建一个新的文件夹，命名为模块名称+Mod，如`UserMod`，如果没有模块名称，则不创建文件夹；

            创建一个C#实体模型类，要遵循以下规范:
            1 命名空间必须使用文件范围命名空间;
            2 继承自`EntityBase`类;
            3 所属属性要添加 xml 注释，注释优先使用中文描述
            4 枚举类型属性要使用英文命名，且必须添加[Description]标签，也使用英文;
            5 如果有明确的模块名称，则为类添加如[Module(Modules.xxx)]标签，其中Modules.xxx是模块的静态定义，在Entity程序集的Modules.cs中定义，如果该文件中没有常量定义，则添加该常量；
            6 属性严格遵循可空类型的定义，必需的属性使用required关键词；
            7 所有string类型，都要添加[MaxLength]标签，如果没有指定长度，则默认为200；
            8 如果是List类型，默认使用默认值`= []`;
            9 将枚举类型定义跟实体类型定义放在同一个文件中，枚举类型放在实体类型的后面；
            </rules>
            """
        );
    }

    [McpServerPrompt, Description("create a prompt for generate form")]
    public static ChatMessage GenerateForm()
    {
        return new ChatMessage(
            ChatRole.User,
            """
            <rules>
            1. 根据提供的描述或代码生成前端表单代码。
            2. 要严格遵循指定的UI框架，如FluentUI，AntDesign或Angular Material等。
            3. 如果说明要弹窗形式，要有相关的自定义组件代码。
            4. 要分析代码中的字段类型和相关特性和注解，如必须和长度限制等，是否为枚举等，以选择合适的表单控件。
            </rules>

            <example>
            对于 FluentUI 框架，组件应该包含Placeholder属性，以及Class="w-100"属性，
            生成的表单代码如下所示：
            ```razor
            @implements IDialogContentComponent

            <FluentDialogHeader ShowDismiss="false">
            <FluentLabel Typo="Typography.PaneHeader">{标题}</FluentLabel>
            <FluentLabel Typo="Typography.Subject" Color="Color.Accent">
                {描述}
            </FluentLabel>
            </FluentDialogHeader>
            <FluentBodyContent>
            <EditForm EditContext="editContext">
                <DataAnnotationsValidator />
                <FluentValidationSummary />
                <FluentStack Orientation="Orientation.Vertical" VerticalGap="12">
                    <FluentStack Orientation="Orientation.Vertical" VerticalGap="0">
                        {控件内容1}
                    </FluentStack>
                    <FluentStack Orientation="Orientation.Vertical" VerticalGap="0">
                        {控件内容2}
                    </FluentStack>

                    // 更多控件内容...
                </FluentStack>
                
            </EditForm>
                
            </FluentBodyContent>
            <FluentDialogFooter>
                <FluentButton Appearance="Appearance.Accent"
                              Type="ButtonType.Button"
                              OnClick="SaveAsync">
                    @Lang(Localizer.Confirm)
                </FluentButton>
                <FluentButton Appearance="Appearance.Neutral"
                              OnClick="CancelAsync">
                    @Lang(Localizer.Cancel)
                </FluentButton>
            </FluentDialogFooter>

            @code{
                [CascadingParameter]
                FluentDialog Dialog { get; set; } = null!;
                EditContext? editContext;

                // 以下替换为给出的模型类
                AddProjectDto AddDto { get; set; } = default!;

                bool formValid { get;set; }

                protected override void OnInitialized()
                {
                    AddDto = new AddProjectDto
                    {
                        ProjectName = string.Empty,
                        ProjectDirectory = string.Empty
                    };
                    editContext = new EditContext(AddDto);
                    editContext.OnFieldChanged += HandleFieldChanged;
                }
                
                private void HandleFieldChanged(object? sender, FieldChangedEventArgs e)
                {
                    if (editContext is not null)
                    {
                        formValid = editContext.Validate();
                        StateHasChanged();
                    }
                }

                public void Dispose()
                {
                    if (editContext is not null)
                    {
                        editContext.OnFieldChanged -= HandleFieldChanged;
                    }
                }

                private async Task SaveAsync()
                {
                    if (!editContext!.Validate())
                    {
                        ToastService.ShowError(Lang(Localizer.FormValidFailed));
                        return;
                    }
                    // 保存逻辑
                }
                private async Task CancelAsync()
                {
                    await Dialog.CancelAsync();
                }
            }
            ```
            </example>
            """
        );
    }
}
