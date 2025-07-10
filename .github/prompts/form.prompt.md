# Form Code Generation

## rules

1. 根据提供的描述或代码生成前端表单代码。
2. 要严格遵循指定的UI框架，如FluentUI，AntDesign或Angular Material等。
3. 如果说明要弹窗形式，要有相关的自定义组件代码。
4. 要分析代码中的字段类型和相关特性和注解，如必须和长度限制等，是否为枚举等，以选择合适的表单控件。

## example

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
