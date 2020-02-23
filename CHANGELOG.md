# CHANGELOG

This project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## 2.0.0 2020-23-2

Moves `Store` and `IAction` to the `Store` namespace. Though small, this is a breaking change, thus v2.0.0.

### ADDED

- Added logo to readme.

### CHANGED

- Moved `Store` to `AReSSO.Store` namespace.
- Moved `IAction` to `AReSSO.Store` namespace.
- Moved `PropertyChange` to `AReSSO.CopyUtils` namespace.

## 1.0.0 2020-23-2

This is the first functional version of AReSSO.

This version adds the `Store`. The `Store` presents the ability to `Dispatch` actions, get the current root state,
and get an observable version of the store that updates when the store changes.

### ADDED

- Add `Store`
- Add `IAction`
- Add tests for `Store`
- Add a few example state objects with documentation.
- Add `PropertyChange` utility to make writing `Copy` methods easier.

## 0.0.0 2020-20-2

Initial version.
