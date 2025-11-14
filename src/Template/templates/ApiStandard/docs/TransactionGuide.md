# 数据库事务使用指南

## 📋 什么时候需要使用事务

### ✅ **必须使用事务的场景**

1. **多表操作**
   ```csharp
   // 创建用户 + 设置角色
   await UpsertAsync(user);
   await _userRoleManager.SetUserRolesAsync(user.Id, roleIds);
   ```

2. **删除 + 插入组合**
   ```csharp
   // 清空现有关联 + 批量插入新关联
   await _dbSet.Where(ur => ur.UserId == userId).ExecuteDeleteAsync();
   await BulkUpsertAsync(userRoles);
   ```

3. **加载导航属性 + 修改关系**
   ```csharp
   // 加载菜单关联 + 更新菜单列表
   await _dbContext.Entry(role).Collection(r => r.Menus).LoadAsync();
   role.Menus = newMenus;
   await UpsertAsync(role);
   ```

4. **业务逻辑相关的多步骤操作**
   ```csharp
   // 用户状态变更 + 记录日志 + 清除缓存
   await UpdateUserStatus(userId, newStatus);
   await LogUserAction(userId, action);
   await ClearUserCache(userId);
   ```

### ❌ **不需要使用事务的场景**

1. **单一实体的CRUD操作**
   ```csharp
   // 单纯的创建、更新、删除单个实体
   await UpsertAsync(entity);
   await DeleteAsync([id], softDelete);
   ```

2. **只读查询操作**
   ```csharp
   // 所有查询操作都不需要事务
   await FindAsync(id);
   await ToPageAsync(filter);
   ```

## 🛠️ **使用方式**

### **推荐方式 - 使用ManagerBase的通用方法**

```csharp
public async Task<SystemUser> CreateUserWithRoles(UserDto dto, List<Guid> roleIds)
{
    return await ExecuteInTransactionAsync(async () =>
    {
        // 业务逻辑操作
        var user = dto.MapTo<SystemUser>();
        await UpsertAsync(user);
        await _userRoleManager.SetUserRolesAsync(user.Id, roleIds);
        return user;
    });
}
```

### **手动事务方式**（仅在特殊情况下使用）

```csharp
public async Task<bool> ComplexOperation()
{
    using var transaction = await _dbContext.Database.BeginTransactionAsync();
    try
    {
        // 复杂的业务逻辑
        await operation1();
        await operation2();
        await operation3();
        
        await transaction.CommitAsync();
        return true;
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();
        _logger.LogError(ex, "复杂操作失败");
        throw;
    }
}
```

## 📈 **性能注意事项**

1. **事务要尽可能短**
   - 事务持续时间越短越好
   - 避免在事务中进行耗时的外部调用

2. **避免嵌套事务**
   - 如果调用的方法本身已经有事务，要小心处理
   - 可以添加事务状态检查

3. **批量操作优化**
   ```csharp
   // 好的做法：使用批量操作
   await BulkUpsertAsync(entities);
   
   // 避免：循环中的多个单独操作
   foreach(var entity in entities)
   {
       await UpsertAsync(entity);  // 这会创建多个事务
   }
   ```

## 🎯 **最佳实践**

1. **明确事务边界**：在方法命名和注释中明确说明事务范围
2. **统一错误处理**：使用通用的事务方法确保错误处理一致
3. **适当的日志记录**：记录事务开始、提交、回滚的关键信息
4. **业务异常处理**：区分技术异常和业务异常的处理方式