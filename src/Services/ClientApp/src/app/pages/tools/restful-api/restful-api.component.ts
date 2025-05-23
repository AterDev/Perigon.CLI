import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToolsService } from 'src/app/services/tools/tools.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatButton } from '@angular/material/button';
import { FormsModule } from '@angular/forms';
import { MatFormField, MatLabel } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { MatSlideToggle } from '@angular/material/slide-toggle';
import { EditorComponent } from 'ngx-monaco-editor-v2';

@Component({
    selector: 'app-restful-api',
    templateUrl: './restful-api.component.html',
    styleUrl: './restful-api.component.css',
    imports: [MatButton, FormsModule, MatFormField, MatLabel, MatInput, MatSlideToggle, EditorComponent]
})
export class RestfulAPIComponent {
  editorOptions = { theme: 'vs-dark', language: 'csharp', minimap: { enabled: false } };
  classCode: string | null = null;
  editor: any;
  modelName: string | null = null;
  description: string | null = null;
  useDto = false;

  constructor(
    private service: ToolsService,
    private snb: MatSnackBar
  ) {
  }

  onInit(editor: any) {
    this.editor = editor;
  }

  ngOnInit(): void {

  }

  convert(): void {
    if (!this.modelName && !this.description) {
      this.snb.open('请输入模型名称和说明', '关闭', { duration: 2000 });
      return;
    }
    const addDto = this.useDto ? this.modelName + 'AddDto' : this.modelName;
    const updateDto = this.useDto ? this.modelName + 'UpdateDto' : this.modelName;
    const filterDto = this.useDto ? this.modelName + 'FilterDto' : 'FilterBase';

    this.classCode = `
/// <summary>
/// 获取${this.description}列表
/// </summary>
/// <param name="filter"></param>
/// <returns></returns>
[HttpPost]
public async Task<ActionResult<List<${this.modelName}>>> Get${this.modelName}List(${filterDto} filter)
{
    return await _manager.FilterAsync(filter);
}

/// <summary>
/// 添加${this.description}
/// </summary>
/// <param name="entity"></param>
/// <returns></returns>
[HttpPost]
public async Task<ActionResult<${this.modelName}>> Add${this.modelName}Async(${addDto} entity)
{
    return await _manager.CreateAsync(entity);
}

/// <summary>
/// 更新${this.description}
/// </summary>
/// <param name="entity"></param>
/// <returns></returns>
[HttpPut]
public async Task<ActionResult<${this.modelName}>> Update${this.modelName}Async(${updateDto} dto)
{
    var entity = await manager.GetCurrentAsync(id);
    if (entity == null)
    {
        return NotFound("未找到该对象");
    }
    return await _manager.UpdateAsync(entity, dto);
}

/// <summary>
/// 获取${this.description}
/// </summary>
/// <param name="id"></param>
/// <returns></returns>
[HttpGet("{id}")]
public async Task<ActionResult<${this.modelName}>> Get${this.modelName}Async(Guid id)
{
    return await _manager.FindAsync(id);
}

/// <summary>
/// 删除${this.description}
/// </summary>
/// <param name="id"></param>
/// <returns></returns>
[HttpDelete("{id}")]
public async Task<ActionResult<${this.modelName}>> Delete${this.modelName}Async(Guid id)
{
    var entity = await manager.FindAsync(id);
    if (entity == null)
    {
        return NotFound("未找到该对象");
    }
    return await _manager.DeleteAsync(id);
}
    `
  }
}
