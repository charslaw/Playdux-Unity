# CHANGELOG

This project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## Version 2.1.0

*Merged to master on 2020-02-26*

Add `StoreBehaviour`, a `MonoBehaviour` wrapper for a `Store`. Also extracts interfaces for `Store`.

### ADDED

- `StoreBehaviour`
- `StoreBehaviour` examples

### CHANGED

- Extract `IStore`, `IActionDispatcher`, and `IStateContainer` from `Store`.
- Simplify, improve the example `Copy` behavior for example classes.

### FIXED

- Date format in change log is wrong 🤦‍♂️

---

## Version 2.0.0

*Merged to master on 2020-02-25*

Moves `Store` and `IAction` to the `Store` namespace. Though small, this is a breaking change, thus v2.0.0.

### ADDED

- Added logo to readme.

### CHANGED

- Update readme with more detailed usage info.
- Modify change log formatting to make it more readable.
- Moved `Store` to `AReSSO.Store` namespace.
- Moved `IAction` to `AReSSO.Store` namespace.
- Moved `PropertyChange` to `AReSSO.CopyUtils` namespace.

---

## Version 1.0.0

*Merged to master on 2020-02-23*

This is the first functional version of AReSSO.

This version adds the `Store`. The `Store` presents the ability to `Dispatch` actions, get the current root state,
and get an observable version of the store that updates when the store changes.

### ADDED

- Add `Store`
- Add `IAction`
- Add tests for `Store`
- Add a few example state objects with documentation.
- Add `PropertyChange` utility to make writing `Copy` methods easier.

---

## Version 0.0.0

*Merged to master on 2020-02-20*

Initial version.
