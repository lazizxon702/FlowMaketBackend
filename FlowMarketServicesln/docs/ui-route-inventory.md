# FlowMarket UI Route Inventory

This inventory is derived from the existing backend feature surface and the provided Figma entry reference.
It defines production routes/components that the frontend now implements and that can be refined once detailed screenshots are provided.

## App routes

- `/auth/login`
- `/auth/register`
- `/auth/forgot-password`
- `/dashboard`
- `/products`
- `/products/[id]`
- `/cart`
- `/orders`
- `/profile`

## Core layout blocks

- Auth layout with centered card and form controls.
- App layout with sidebar navigation, top bar, content container.
- Shared page header with title/subtitle/actions.
- Data card and table/list wrappers.

## Reusable UI components

- `Button` (primary/secondary/ghost/destructive, disabled/loading)
- `Input` (label, helper text, error state)
- `PasswordInput` (visibility toggle)
- `FormField` wrapper for RHF + Zod integration
- `Badge` for statuses
- `ProductCard`
- `OrderRow`
- `EmptyState`
- `LoadingState`

## Required interaction states

- Loading (skeleton/spinner) on queries.
- Inline validation errors on forms.
- API error notifications for failed mutations.
- Empty list states for products/orders/cart.
- Protected route redirect to login when unauthenticated.
