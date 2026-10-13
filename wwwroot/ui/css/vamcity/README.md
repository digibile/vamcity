# VamCity UI theme policy

This folder contains presentation-only overrides for the existing Vue/Vuetify application. Keep the legacy app bundles, routes, API contracts, authentication, database operations, payment flows, role checks, and business rules unchanged.

## File boundaries

- `tokens-base.css`: global design tokens and document defaults only.
- `shell-navigation.css`: app toolbar, navigation drawer, active navigation state, and footer.
- `surfaces-actions.css`: reusable cards, sheets, buttons, chips, and expansion panels.
- `forms-tables.css`: input appearance, data tables, tabs, pagination, and progress indicators.
- `feedback-overlays.css`: alerts, dialogs, and transient feedback styling.
- `responsive.css`: small-screen layout adjustments only.
- `../vamcity-modern-theme.css`: ordered import manifest; do not place component rules here.

## Change policy

1. Put each new rule in the narrowest matching module. Do not create a giant catch-all stylesheet.
2. Prefer existing Vuetify classes and design tokens. Avoid broad selectors that could alter unrelated views.
3. Never use CSS to hide required controls, imply a successful financial state, or change workflow semantics.
4. Do not edit minified legacy JavaScript bundles as part of a visual-only task.
5. Before production rollout, test login/logout, role-based menus, loan listing/details, application submission, installment/payment views, reports, admin/agent/member areas, mobile layout, and browser console/network errors.
6. Keep deployment separate from code changes; do not claim production is updated until deployment and live checks succeed.
