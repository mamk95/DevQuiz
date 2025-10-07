# Vue 3 + TypeScript + SCSS + Tailwind — Code Style Guide

Audience: developers and coding agents working on this codebase.  
Scope: code style, formatting and SFC conventions.

## General formatting

- Indentation: 2 spaces. Line length soft limit: 140.
- Quotes: single quotes. No semicolons.
- Trailing commas: where valid in ES5.
- Line endings: LF. Always end files with a final newline.
- Equality: use `===`/`!==`. Allow `== null` only for "nullish" checks (matching both `null` and `undefined`).
- Console: no `console.*` in committed code (use a logger or remove before commit).
- Keep code shallow. Prefer early returns over deep nesting.
- Use guard clauses at the start of functions to validate input, fail fast, and avoid nested conditionals.
- For simple guard clauses with just a return statement, keep them on one line: `if (!value) return`
- Only expand to multiple lines when the condition or statement is complex.
- Soft limits: ~200 lines per file, ~30 lines per function.

## Single‑File Component layout

Write SFC blocks in this order and keep one blank line between each block:

1. `<template>`
2. `<script setup lang="ts">`
3. `<style scoped lang="scss">`

## Template rules

- Put a blank line between all sibling elements at any nesting level. This improves scanability in templates.
- Attribute wrapping: if a tag wraps, place one attribute per line, aligned and indented once.
- Casing: use kebab‑case for event names in templates; use camelCase in scripts. Use PascalCase for component names in templates.
- Lists: always provide a stable `:key` for `v-for`.
- Avoid inline complex logic in templates. Move conditionals and transforms into computed properties or small helpers.

## Script setup rules

Maintain this strict ordering inside `<script setup>` to keep files predictable:

1. Imports
2. Module‑level constants, enums, types, and interfaces
3. `defineProps` (types first, defaults next)
4. `defineEmits`
5. `defineModel` / `useModel` (if used)
6. `defineSlots` (if used)
7. Exposed API (`defineExpose`) (if used)
8. Lifecycle hooks (`onMounted`, etc.)
9. Reactive state (`ref`, `reactive`)
10. Derived state (`computed`)
11. Effects (`watch`, `watchEffect`) - add blank lines between individual watchers
12. Functions (business logic, event handlers, utilities)

### Functions

- Prefer function declarations for top‑level functions and handlers.
- Use arrow functions where they read best and capture `this` is irrelevant — primarily in `computed`, `watch`, small callbacks, and inline helpers.

## Readability

- Prefer readable, straightforward code over micro-optimizations.
- Use common, recognizable patterns instead of clever one-liners.
- Favor small, well-named helpers over inline complex expressions.

## Comments

- Keep comments short and purposeful. Explain why something is done or any assumptions/invariants, not what the code already states.
- Place comments above the line or block they describe. Use brief inline end‑of‑line comments only for quick context.
- Prefer comment style consistent with the language context:
  - Template: HTML comments only when needed; linter directives live directly above the relevant line.
  - TypeScript/JS: line comments for clarifications; link to external references when it improves understanding.
  - SCSS/CSS: concise property‑level comments are acceptable; keep them short and focused.
- In docblocks/JSDoc/XML‑style documentation: Avoid redundant headers and generated boilerplate.
- Comments summarizing a function should be docblocks/JSDoc.
- Do not leave commented‑out code in commits.
- Think of what an average developer knows, and comment where something would be difficult for an average developer to understand.
- Avoid self-explanatory comments like "Computed properties" and "Functions".
- Keep helpful inline comments that explain the purpose of code sections (e.g., "// Update markers on time scrub" before a watcher).
- In script sections, use comments to clarify the intent of watchers, complex logic, or non-obvious behavior.
- Template comments should be used sparingly, mainly for conditional rendering logic or accessibility notes.

## v-model / useModel

- Prefer `defineModel<T>()` for two‑way bindings in new components.
- For multiple models, use `v-model:foo` with explicit types.
- Do not mutate props. Update via emits or model setters.
- For arrays/objects, emit new references (no in‑place mutation).

## TypeScript

- Use typed `defineProps`/`defineEmits`/`defineModel`. Avoid `any`.
- Name types and interfaces in PascalCase.
- Keep public component API (props, emits, slots) fully typed.
- For external library types or truly dynamic data, use proper type assertions or unions instead of `any`.
- Avoid `as` type assertions; prefer type guards and proper typing.

## Styling (Tailwind + SCSS)

- Use Tailwind where it is more readable than SCSS.
- Use SCSS when Tailwind becomes complicated (stateful styles, complex selectors, long utility chains).
- Root CSS class for a component: PascalCase (e.g., `.MyComponent`). Non‑root classes: kebab‑case.
- Keep SCSS nesting shallow (readable selectors, avoid deep chains).

## Whitespace & spacing details

- One blank line between logical sections (imports/types/props/… blocks as listed above).
- One blank line between all template siblings at any nesting level.
- No multiple consecutive blank lines.
- Space after commas and around infix operators.
- Object/array literals: multiline when they exceed the line length; trailing comma on multiline.

## Naming

- Prefer descriptive, non-abbreviated names (e.g., `resizeObserver` not `ro`).
- Functions and methods: use clear verb–object names.
- Booleans read positively.
- Avoid single-letter names except for short-lived loop indices (i, j, k).
- Avoid cryptic abbreviations - variable names should clearly indicate their purpose.
- Use consistent domain terminology across files.

- Components and files: PascalCase (e.g., `MyComponent.vue`).
- Composables: `useXxx.ts`.
- Refs/computed variables: meaningful, camelCase.
- Events: present‑tense, kebab‑case in templates (e.g., `row-click`), camelCase in code (`rowClick`).
- Custom events should follow `verb-noun` pattern (e.g., `update-selection`, `load-file`, `delete-item`).

## File Organization

- Group related components in folders (e.g., `/sidebar`, `/chart`, `/parts`)
- Co-locate composables near their usage when specific to a feature
- Keep shared composables in `/composables`
- Store modules should be split when they grow large (e.g., `/stores/modules/`)

## Import Organization

Within each script section, organize imports in this order:

1. Vue core imports (`vue`, `vue-router`, etc.)
2. Third-party libraries
3. Store imports (`@/stores/...`)
4. Composables (`@/composables/...`)
5. Components (`@/components/...`)
6. Types (prefer `type` imports when possible)
7. Utils/helpers (`@/lib/...`)
8. Styles (if any)

Do not leave a blank line between import groups.

## Component Size Guidelines

- Soft limit: ~200 lines per component file
- When a component exceeds this limit, consider:
  1. Extracting logic into composables (for reusable logic)
  2. Breaking into smaller sub-components (for UI chunks)
  3. Moving complex computed properties to utility functions
  4. Splitting template sections into child components

## Error Handling

- Use early returns for validation at the start of functions
- Prefer try-catch at the action/API level, not in every function
- Check for null/undefined before accessing nested properties
- Log errors meaningfully with context (what failed, not just the error)
