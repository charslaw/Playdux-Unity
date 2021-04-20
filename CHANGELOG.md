# CHANGELOG

This project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## Version 7.0.1

*Merged to master on 2021-04-20*

### FIXED

- Fixed the Store not initializing itself with the initial state.

---

## Version 7.0.0

*Merged to master on 2021-04-20*

### CHANGED

- Return to using a synchronous action pipeline
  - This was done because the previous implementation caused problems due to being required to use the main thread to interop with UnityEngine APIs.

---

## Version 6.0.0

*Merged to master on 2021-04-19*

### CHANGED

- Hoist `RegisterSideEffector` and `UnregisterSideEffector` into the `IActionDispatcher` interface.
- Add generic `TRootState` parameter to `IActionDispatcher` interface.
- `Store.ObservableFor` will now correctly emit `onError` if an error occurs in an `onNext` handler.
- Set a default execution order for `StoreBehaviour`.

---

## Version 5.0.0

*Merged to master on 2021-04-19*

### CHANGED

- Changed signature for `ISideEffector.PreEffect` and `ISideEffector.PostEffect` to accept an `IStore` rather than an `IActionDispatcher`.
  - This allows side effectors to inspect state as well as dispatch new actions.
- Additionally, the `ISideEffector.PostEffect` signature was changed to remove the `state` parameter as the new state can now be retrieved from the provided `IStore`.

---

## Version 4.0.0

*Merged to master on 2021-04-15*

### ADDED

- Added Side Effectors! This feature allows code to hook into the action stream and initiate side effects as well as interrupt actions and inject new ones
  - Side Effectors can hook into the pipeline at two locations:
    - `PreEffect` occurs before the action reaches the reducer.
      At this point, the side effector can launch some asynchronous task, dispatch another action, or *prevent* the incoming action from being sent on to the reducer.
    - `PostEffect` occurs after the action has been processed by the reducer.
      In a post effect, a side effector can inspect the updated state after the reducer, launch an async task, or dispatch another action.
  - Side Effectors can define a `Priority` which determines the order in which they'll be executed.

### CHANGED

- Adding Side Effectors has required some re-arrangement of the action pipeline and how it handles concurrency.
  *The pipeline is no longer synchronous*, meaning that you can't be guaranteed that the state will be updated immediately after a call to `Dispatch`.
  Instead, consumers should make use of the observable pattern presented via `ObservableFor`.

---

## Version 3.0.0

*Merged to master on 2021-04-12*

### ADDED

- Add `InitializeAction`, which handles initializing or reinitializing a `Store`.

### CHANGED

- Rename project to "Playdux".
- Updated to Unity 2021.2 for C#9 support.
  - Use `record` whenever possible as opposed to `class`.
  - Use `#nullable enable` in all files.

---

## Version 2.1.2

*Merged to master on 2020-03-01*

Adds the missing aresso_icon meta file as not having it causes error messages in projects that import the AReSSO package.

### FIXED

- Add aresso_icon.svg.meta.

---

## Version 2.1.1

*Merged to master on 2020-02-26*

### FIXED

- Update package.json version.
- Update package.json with UniRx dependency.

---

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
