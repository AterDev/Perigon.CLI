# Angular Reference

This reference belongs to the Perigon skill and covers Angular 21+ frontend work for this template.

## When to use

- Build CRUD pages, forms, routes, shared components, and Material-based layouts.
- Generate typed request clients from backend OpenAPI through Perigon.
- Implement or review frontend changes in a Perigon-based project.
- When editing Angular files: component, template, route, form, Material UI, signals, i18n, generated request client

## Project structure

```sh
src/
  ├── main.ts
  ├── app/
  │   ├── app.config.ts
  │   ├── app.routes.ts
  │   ├── layout/
  │   ├── pages/
  │   ├── modules/
  │   │   ├── share/          # 打包的基础依赖，供所有业务模块复用
  │   │   │   ├── components/
  │   │   │   ├── pipe/
  │   │   │   ├── auth.guard.ts
  │   │   │   ├── custom-paginator-intl.ts
  │   │   │   └── i18n-keys.ts
  │   │   └── {module}/       # 业务前端模块
  │   └── services/
  ├── assets/i18n/
  ├── environments/
  ├── styles/
  └── proxy.conf.json
```

## Core rules

- 组件默认standalone模式，默认拆分成html/scss/ts文件.
- UI 优先使用 Angular Material；样式优先复用现有样式`styles.scss`及`theme.scss`，布局使用Bootstrap flex，避免使用row&col，避免内联样式。
- 状态优先使用 signals、async pipe 或框架推荐响应式写法；模板中避免调用复杂函数。
- 表单使用 Typed Reactive Forms；在 FormGroup 内优先用 `[formControl]` + getter，不优先使用 `formControlName`。
- Import `I18N_KEYS` from `src/app/modules/share/i18n-keys` and expose it on each translated component. In templates use `i18nKeys.common.save | translate`; for `TranslateService` use `translate.instant(this.i18nKeys.common.save)`. Do not use literal or constructed translation keys.
- Keep generated request contracts untouched and regenerate them when the backend changes.
- Avoid inline styles and prefer shared styles / Material tokens.
- Put reusable frontend infrastructure in `src/app/modules/share` and import it through `src/app/modules/share/...`; do not create `src/app/share`.

## 页面组件和UX指南

根据数据结构选择合适的Material组件进行展示和交互，如：

- 在筛选组件中对 类别、枚举、目录等数据不多的，使用`mat-select`，以进行选择。
- 在筛选组件中对 用户或其他列表选择时，使用`Autocomplete`，以支持搜索和选择。
- 在列表的筛选页面，筛选控件通常与添加按钮放到同一行，并垂直对齐(align-items-center)，此时`<mat-form-field>`要添加`subscriptSizing="dynamic"`。
- 不要使用浏览器的弹窗提示，而是使用material dialog实现，弹窗要有适合的宽度和长度，通常长度不超过`96vh`，宽度不超过`900px`，最小宽度一般在`400px`.

在UX设计中，要遵循：

- 视觉体验交互良好。如使用不同颜色和风格标记组件或按钮。如删除要添加`error`class。
- 遵循整体的主题设计，不要自己添加自定义字体颜色。

## Recommended workflow

1. Generate or refresh request clients from backend OpenAPI through Perigon.
2. Create the page or component structure.
3. Configure route/menu(`menus.json`) and shared UI pieces.
4. Implement the TS / HTML / SCSS logic and validate the interaction states.
5. Run `pnpm build` (and optionally `pnpm start`) to verify the result.

## UX and validation checklist

- Check layout density, empty / loading / error states, and responsive behavior.
- Prefer Material components and existing theme styles over custom visual overrides.
- Keep user-visible strings in i18n and avoid hard-coded text.

## Verification

- Run `pnpm build` after frontend changes.
- Prefer `pnpm start` for live validation when the page is non-trivial.
